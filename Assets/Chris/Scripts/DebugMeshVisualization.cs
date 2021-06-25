using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugMeshVisualization : MonoBehaviour
{
    protected void Start()
    {
        var meshFilter = GetComponent<MeshFilter>();
        var sharedMesh = meshFilter.sharedMesh;
        var vertices = sharedMesh.vertices;
        var triangles = sharedMesh.triangles;

        for (var index = 0; index < triangles.Length; index += 3)
        {
            var container = new GameObject($"{index},{index + 1},{index + 2}").transform;
            container.parent = transform;
            container.localPosition = Vector3.zero;
            container.localRotation = Quaternion.identity;
            container.localScale = Vector3.one;

            CreateCube(vertices, triangles[index], container);
            CreateCube(vertices, triangles[index + 1], container);
            CreateCube(vertices, triangles[index + 2], container);
        }

        //for (var index = 0; index < vertices.Length; ++index)
        //{
        //    CreateCube(vertices, index, transform);
        //}

        static GameObject CreateCube(Vector3[] vertices, int index, Transform parent)
        {
            var vertex = vertices[index];
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"{index}";
            go.transform.parent = parent;
            go.transform.localPosition = vertex;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            var goCollider = go.GetComponent<Collider>();
            if (goCollider != null)
                Destroy(goCollider);

            return go;
        }
    }
}
