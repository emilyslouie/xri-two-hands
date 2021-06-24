
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.XR.Interaction.Toolkit;

public class SymmetricalAttach : MonoBehaviour
{
  
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

        var camera = Camera.main;
        if (camera != null)
        {
            var rightHand = interactorAttachPoint.GetComponentInParent<ActionBasedController>().positionAction.action.activeControl.device.usages.Contains(new InternedString("RightHand"));
            if (rightHand)
            {
                FlipForRightHand();
            }
            else
            {
                transform.localRotation = m_OriginalAttach.rotation;
            }
        }

        //transform.position = interactorAttachPoint.position;
    }

    void FlipForRightHand()
    {
        var newForward = m_OriginalAttach.forward;
        newForward.y *= -1f;
        var newUp = m_OriginalAttach.up;
        newUp.y *= -1f;

        transform.localRotation = Quaternion.LookRotation(newForward, newUp);
    }
}
