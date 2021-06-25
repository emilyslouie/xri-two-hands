using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Cutter : MonoBehaviour
{
    public enum AngleStatus
    {
        WaitingForRelease,
        WaitingForActuate,
    }

    [SerializeField]
    float m_AngleThreshold = -3f;

    [SerializeField]
    float m_AngleReleaseThreshold = -25f;

    [SerializeField]
    HingeJoint m_HingeJoint;

    [SerializeField] // Debug
    float m_Angle;

    [SerializeField] // Debug
    AngleStatus m_AngleStatus;

    [SerializeField]
    UnityEvent m_Actuated;

    [SerializeField]
    LayerMask m_OverlapLayerMask = -1;

    [SerializeField] // Debug
    List<Vector3> m_PositiveVertices = new List<Vector3>();
    [SerializeField] // Debug
    List<int> m_PositiveTriangles = new List<int>();

    [SerializeField] // Debug
    List<Vector3> m_NegativeVertices = new List<Vector3>();
    [SerializeField] // Debug
    List<int> m_NegativeTriangles = new List<int>();

    [SerializeField]
    bool m_ShowDebugPlane;

    [SerializeField]
    float m_DebugDestroyDelay = 1f;

    bool m_ActuatedThisFrame;

    readonly Collider[] m_OverlapColliders = new Collider[10];

    protected void Awake()
    {
        if (m_HingeJoint == null)
        {
            m_HingeJoint = GetComponentInParent<HingeJoint>();
            if (m_HingeJoint == null)
            {
                Debug.LogError("Could not find HingeJoint", this);
                enabled = false;
            }
        }
    }

    protected void Update()
    {
        m_Angle = m_HingeJoint.angle;
        m_ActuatedThisFrame = false;
        switch (m_AngleStatus)
        {
            case AngleStatus.WaitingForRelease when m_HingeJoint.angle < m_AngleReleaseThreshold:
                m_AngleStatus = AngleStatus.WaitingForActuate;
                break;
            case AngleStatus.WaitingForActuate when m_HingeJoint.angle >= m_AngleThreshold:
                m_AngleStatus = AngleStatus.WaitingForRelease;
                m_ActuatedThisFrame = true;
                break;
        }

        if (m_ActuatedThisFrame)
            OnActuated();
    }

    protected virtual void OnActuated()
    {
        //Debug.Log($"{Time.frameCount} Actuated", this);
        var halfExtents = transform.lossyScale / 2f;
        var numHits = Physics.OverlapBoxNonAlloc(transform.position, halfExtents, m_OverlapColliders, transform.rotation, m_OverlapLayerMask);
        for (var index = 0; index < numHits; ++index)
        {
            var hit = m_OverlapColliders[index];
            Debug.Log($"{Time.frameCount} Cut {hit.name}", this);

            var cuttable = hit.GetComponent<Cuttable>();
            if (cuttable == null)
                continue;

            var a = cuttable.transform.InverseTransformPoint(transform.position);
            var b = cuttable.transform.InverseTransformPoint(transform.position + transform.right);
            var c = cuttable.transform.InverseTransformPoint(transform.position - transform.forward);
            var plane = new Plane(a, b, c);
            Cut(cuttable, plane, out var positiveMesh, out var negativeMesh);

            // Debug visualization
            //var positiveGameObject = CreateSideGameObject(positiveMesh, cuttable.transform);
            //positiveGameObject.name = $"{cuttable.name} Positive";

            //var negativeGameObject = CreateSideGameObject(negativeMesh, cuttable.transform);
            //negativeGameObject.name = $"{cuttable.name} Negative";

            var positiveGameObject = Instantiate(cuttable.gameObject, cuttable.transform.parent, true);
            var positiveMeshFilter = positiveGameObject.GetComponent<MeshFilter>();
            positiveMeshFilter.mesh = positiveMesh;
            var positiveBody = GetOrAddComponent<Rigidbody>(positiveGameObject);
            positiveBody.useGravity = true;
            positiveBody.isKinematic = false;

            var negativeMeshFilter = cuttable.GetComponent<MeshFilter>();
            negativeMeshFilter.mesh = negativeMesh;
        }

        // Create a copy to visualize where the plane cut
        if (m_ShowDebugPlane)
        {
            var planeVisualization = GameObject.CreatePrimitive(PrimitiveType.Plane);
            planeVisualization.transform.SetPositionAndRotation(transform.position, transform.rotation);
            planeVisualization.transform.localScale = transform.lossyScale;
            SafeDestroyComponent<Collider>(planeVisualization);
            Destroy(planeVisualization, Mathf.Max(m_DebugDestroyDelay, 0f));
        }

        m_Actuated?.Invoke();
    }

    protected virtual void Cut(Cuttable cuttable, Plane plane, out Mesh positiveMesh, out Mesh negativeMesh)
    {
        var meshFilter = cuttable.GetComponent<MeshFilter>();
        var mesh = meshFilter.mesh;
        var vertices = mesh.vertices;
        var triangles = mesh.triangles;

        m_PositiveVertices.Clear();
        m_PositiveTriangles.Clear();
        m_NegativeVertices.Clear();
        m_NegativeTriangles.Clear();

        for (var i = 0; i < triangles.Length; i += 3)
        {
            var vertex0 = vertices[triangles[i]];
            var vertex1 = vertices[triangles[i + 1]];
            var vertex2 = vertices[triangles[i + 2]];

            var vertex0Side = plane.GetSide(vertex0);
            var vertex1Side = plane.GetSide(vertex1);
            var vertex2Side = plane.GetSide(vertex2);

            if (vertex0Side == vertex1Side && vertex0Side == vertex2Side)
            {
                // All 3 vertices are on the same side of the cut plane
                if (vertex0Side)
                    AddVertices(m_PositiveVertices, m_PositiveTriangles, vertex0, vertex1, vertex2);
                else
                    AddVertices(m_NegativeVertices, m_NegativeTriangles, vertex0, vertex1, vertex2);
            }
            else if (vertex0Side == vertex1Side || vertex0Side == vertex2Side)
            {
                var doubleSideVertices = vertex0Side ? m_PositiveVertices : m_NegativeVertices;
                var doubleSideTriangles = vertex0Side ? m_PositiveTriangles : m_NegativeTriangles;
                var singleSideVertices = vertex0Side ? m_NegativeVertices : m_PositiveVertices;
                var singleSideTriangles = vertex0Side ? m_NegativeTriangles : m_PositiveTriangles;

                if (vertex0Side == vertex1Side)
                {
                    // Vertex 0 and 1 are on the same side of the cut plane
                    // 0        1
                    // ----------
                    // \        |
                    //   \      |
                    // ====\====|===
                    //       \  |
                    //         \
                    //          2
                    var vertex02 = GetPlaneIntersectionPoint(vertex0, vertex2, ref plane);
                    var vertex12 = GetPlaneIntersectionPoint(vertex1, vertex2, ref plane);

                    // Using intersection points, construct 2 triangles from double side & 1 triangle from single side
                    AddVertices(doubleSideVertices, doubleSideTriangles, vertex0, vertex1, vertex12);
                    AddVertices(doubleSideVertices, doubleSideTriangles, vertex0, vertex12, vertex02);
                    AddVertices(singleSideVertices, singleSideTriangles, vertex12, vertex2, vertex02);
                }
                else
                {
                    // Vertex 0 and 2 are on the same side of the cut plane
                    var vertex01 = GetPlaneIntersectionPoint(vertex0, vertex1, ref plane);
                    var vertex12 = GetPlaneIntersectionPoint(vertex1, vertex2, ref plane);

                    // Using intersection points, construct 2 triangles from double side & 1 triangle from single side
                    AddVertices(doubleSideVertices, doubleSideTriangles, vertex0, vertex01, vertex12);
                    AddVertices(doubleSideVertices, doubleSideTriangles, vertex0, vertex12, vertex2);
                    AddVertices(singleSideVertices, singleSideTriangles, vertex01, vertex1, vertex12);
                }
            }
            else
            {
                // Vertex 1 and 2 are on the same side of the cut plane
                var doubleSideVertices = vertex0Side ? m_NegativeVertices : m_PositiveVertices;
                var doubleSideTriangles = vertex0Side ? m_NegativeTriangles : m_PositiveTriangles;
                var singleSideVertices = vertex0Side ? m_PositiveVertices : m_NegativeVertices;
                var singleSideTriangles = vertex0Side ? m_PositiveTriangles : m_NegativeTriangles;

                var vertex01 = GetPlaneIntersectionPoint(vertex0, vertex1, ref plane);
                var vertex02 = GetPlaneIntersectionPoint(vertex0, vertex2, ref plane);

                // Using intersection points, construct 2 triangles from double side & 1 triangle from single side
                AddVertices(doubleSideVertices, doubleSideTriangles, vertex01, vertex1, vertex2);
                AddVertices(doubleSideVertices, doubleSideTriangles, vertex01, vertex2, vertex02);
                AddVertices(singleSideVertices, singleSideTriangles, vertex0, vertex01, vertex02);
            }
        }

        positiveMesh = new Mesh
        {
            vertices = m_PositiveVertices.ToArray(),
            triangles = m_PositiveTriangles.ToArray(),
        };
        positiveMesh.RecalculateNormals();

        negativeMesh = new Mesh
        {
            vertices = m_NegativeVertices.ToArray(),
            triangles = m_NegativeTriangles.ToArray(),
        };
        negativeMesh.RecalculateNormals();
    }

    static void AddVertices(List<Vector3> vertices, List<int> triangles, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2)
    {
        var startIndex = vertices.Count;
        vertices.Add(vertex0);
        vertices.Add(vertex1);
        vertices.Add(vertex2);
        triangles.Add(startIndex);
        triangles.Add(startIndex + 1);
        triangles.Add(startIndex + 2);
    }

    static Vector3 GetPlaneIntersectionPoint(Vector3 a, Vector3 b, ref Plane plane)
    {
        var ray = new Ray(a, b - a);
        plane.Raycast(ray, out var distance);
        return ray.GetPoint(distance);
    }

    protected GameObject CreateSideGameObject(Mesh mesh, Transform source)
    {
        var go = new GameObject();
        var meshFilter = go.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;
        go.AddComponent<MeshRenderer>();
        go.transform.SetPositionAndRotation(source.position, source.rotation);
        go.transform.localScale = source.lossyScale;

        return go;
    }

    static T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        var comp = go.GetComponent<T>();
        if (comp == null)
            comp = go.AddComponent<T>();
        return comp;
    }

    static void SafeDestroyComponent<T>(GameObject go) where T : Component
    {
        var comp = go.GetComponent<T>();
        if (comp != null)
            Destroy(comp);
    }
}
