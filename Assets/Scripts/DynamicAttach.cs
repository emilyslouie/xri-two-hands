using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DynamicAttach : MonoBehaviour
{
    [SerializeField]
    [Tooltip("If true, attach point will only shift around within the range of the grab interactable's colliders")]
    bool m_ConstrainToCollider = true;

    [SerializeField]
    [Tooltip("If set, uses these colliders instead of the XRGrabInteractable colliders to shift within")]
    List<Collider> m_ConstrainToSpecficColliders = default;

    [SerializeField]
    [Tooltip("If true, the attach point's position will match the position of the interactor upon grab")]
    bool m_DynamicPosition = true;

    [SerializeField]
    [Tooltip("If true, the attach point's rotation will match the rotation of the interactor upon grab")]
    bool m_DynamicRotation = true;

    XRGrabInteractable m_GrabInteractable;

    Pose m_OriginalAttach;

    void Start()
    {
        m_GrabInteractable = GetComponentInParent<XRGrabInteractable>();
        if (m_GrabInteractable == null)
        {
            Debug.LogWarning($"No grab interactable found for {name}.");
            enabled = false;
        }

        // Cache the original attach point for use with sockets
        m_OriginalAttach = new Pose(transform.localPosition, transform.localRotation);

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
        if (args.interactor is XRSocketInteractor)
        {
            // We don't recenter with sockets
            transform.localPosition = m_OriginalAttach.position;
            transform.localRotation = m_OriginalAttach.rotation;

            return;
        }
        var interactorAttachPoint = args.interactor.attachTransform;

        Vector3 attachPosition = interactorAttachPoint.position;
        Quaternion attachRotation = interactorAttachPoint.rotation;

        Recenter(attachPosition, attachRotation);
    }

    internal void Recenter(Vector3 position, Quaternion rotation)
    {
        if (m_ConstrainToSpecficColliders != null && m_ConstrainToSpecficColliders.Count > 0)
        {
            position = ClosestAttachPointOnColliders(position, m_ConstrainToSpecficColliders);
        }
        else if (m_ConstrainToCollider)
        {
            position = ClosestAttachPointOnColliders(position, m_GrabInteractable.colliders);
        }

        if (m_DynamicPosition)
            transform.position = position;

        if (m_DynamicRotation)
            transform.rotation = rotation;
    }

    static Vector3 ClosestAttachPointOnColliders(Vector3 position, List<Collider> colliders)
    {
        var attachDistanceSq = 10000000.0f;
        var closestPosition = position;
        foreach (var collider in colliders)
        {
            var newPosition = collider.ClosestPoint(position);
            var distanceSq = (newPosition - position).sqrMagnitude;
            if (distanceSq < attachDistanceSq)
            {
                attachDistanceSq = distanceSq;
                closestPosition = newPosition;
            }
        }
        return closestPosition;
    }
}
