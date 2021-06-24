using UnityEngine;
using TwoHandedBow;

[ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer))]
public class StringRenderer : MonoBehaviour
{
    [Header("Settings")]
    public Gradient pullColor = null;

    [Header("References")]
    public PullMeasurer pullMeasurer = null;

    [Header("Render Positions")]
    public Transform start = null;
    public Transform middle = null;
    public Transform end = null;

    private LineRenderer lineRenderer = null;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (Application.isEditor && !Application.isPlaying)
            UpdatePositions();
    }

    private void OnEnable()
    {
        Application.onBeforeRender += UpdatePositions;

        pullMeasurer.Pulled.AddListener(UpdateColor);
    }

    private void OnDisable()
    {
        Application.onBeforeRender -= UpdatePositions;  
        pullMeasurer.Pulled.RemoveListener(UpdateColor);
    }

    private void UpdatePositions()
    {
        Vector3[] positions = new Vector3[] { start.position, middle.position, end.position };
        lineRenderer.SetPositions(positions);
    }

    private void UpdateColor(Vector3 pullPosition, float pullAmount)
    {
        //middle.position = pullPosition;
        Color color = pullColor.Evaluate(pullAmount);
        lineRenderer.material.color = color;
    }
}
