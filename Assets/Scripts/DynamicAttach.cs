using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DynamicAttach : MonoBehaviour
{
    [SerializeField]
    [Tooltip("If true, attach point will only shift around within the range of the grab interactable's colliders")]
    bool m_ConstrainToCollider = true;

    [SerializeField]
    [Tooltip("If true, the attach point's position will match the position of the interactor upon grab")]
    bool m_DynamicPosition = true;

    [SerializeField]
    [Tooltip("If true, the attach point's rotation will match the rotation of the interactor upon grab")]
    bool m_DynamicRotation = true;

    XRGrabInteractable m_GrabInteractable;

    void Start()
    {
        m_GrabInteractable = GetComponentInParent<XRGrabInteractable>();

        if (m_GrabInteractable == null)
        {
            Debug.LogWarning($"No grab interactable found for {name}.");
            enabled = false;
        }

        // If this is the set attach point, then listen for selection events
        // Otherwise, assume we are a 'secondary' attach point 
        if (m_GrabInteractable.attachTransform == transform)
            m_GrabInteractable.selectEntered.AddListener(SelectEntered);
    }

    void OnDestroy()
    {
        if (m_GrabInteractable != null)
            m_GrabInteractable.selectEntered.RemoveListener(SelectEntered);
    }
    void SelectEntered(SelectEnterEventArgs args)
    {
        var interactorAttachPoint = args.interactor.attachTransform;

        Vector3 attachPosition = interactorAttachPoint.position;
        Quaternion attachRotation = interactorAttachPoint.rotation;

        Recenter(attachPosition, attachRotation);
    }

    internal void Recenter(Vector3 position, Quaternion rotation)
    {
        if (m_ConstrainToCollider)
        {
            var attachDistanceSq = 10000000.0f;
            var closestPosition = position;
            foreach (var collider in m_GrabInteractable.colliders)
            {
                var newPosition = collider.ClosestPoint(position);
                var distanceSq = (newPosition - position).sqrMagnitude;
                if (distanceSq < attachDistanceSq)
                {
                    distanceSq = attachDistanceSq;
                    closestPosition = newPosition;
                }
            }
            position = closestPosition;
        }

        if (m_DynamicPosition)
            transform.position = position;

        if (m_DynamicRotation)
            transform.rotation = rotation;
    }
}
