using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class XRTwoHandGrabAndTwist : XRMultiGrabInteractable
{
    public enum GrabControlMode
    {
        Top,
        Bottom,
        First,
        Second
    }

    [SerializeField, Tooltip("The method for choosing the main twist influence (determines the interactable forward direction when held with two hands).")]
    GrabControlMode m_TwistControlMode = GrabControlMode.Top;

    void Start()
    {
        m_PrimarySelectInputAction = new InputAction("smoothAction");
        m_PrimarySelectInputAction.Disable();
        
        
        m_SecondarySelectInputAction = new InputAction("smoothAction");
        m_SecondarySelectInputAction.Disable();
    }

    InputAction m_PrimarySelectInputAction;
    
    InputAction m_SecondarySelectInputAction;
    
    // Many activate actions are configured as buttons - these constants are to replace those common bindings with analog versions appropriate for animation
    static readonly string[] k_ButtonLabels = { "triggerPressed", "triggerButton", "gripPressed", "gripButton" };
    static readonly string[] k_SwapLabels = { "{Trigger}", "{Trigger}", "{Grip}", "{Grip}" };

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        var secondarySelection = m_SecondarySelection;
        
        base.OnSelectEntered(args);
        if (secondarySelection)
        {
            var primaryInfluence = selectingInteractor.attachTransform;
            var secondaryInfluence = args.interactor.attachTransform;
            var secondaryPose = new Pose(secondaryInfluence.position, secondaryInfluence.rotation);
            var primaryPose = new Pose(primaryInfluence.position, primaryInfluence.rotation);
            attachTransform.rotation = GetFinalRotation(secondaryPose, primaryPose, attachTransform.parent.up);
        }

        var inputAction = secondarySelection ? m_SecondarySelectInputAction : m_PrimarySelectInputAction;

        // Get the controller from the interactor, and then the activation control from there
        var controllerInteractor = args.interactor as XRBaseControllerInteractor;
        if (controllerInteractor == null)
        {
            Debug.LogWarning($"Selected by {args.interactor.name}, which is not an XRBaseControllerInteractor");
            return;
        }
        var controller = controllerInteractor.xrController as ActionBasedController;
        if (controller == null)
        {
            Debug.LogWarning($"Selected by {controllerInteractor.xrController.name}, which is not an ActionBasedController");
            return;
        }

        // We grab the controller action and borrow its bindings - this is usually set to button so we can't just use this action directly
        var controllerBindings = controller.selectAction.action.bindings;
        foreach (var currentBinding in controllerBindings)
        {
            // Similarly, convert any button bindings to ones with analog readings
            var bindingString = currentBinding.path;
            for (var swapIndex = 0; swapIndex < k_SwapLabels.Length; swapIndex++)
            {
                bindingString = bindingString.Replace(k_ButtonLabels[swapIndex], k_SwapLabels[swapIndex]);
            }

            inputAction.AddBinding(bindingString, currentBinding.interactions, currentBinding.processors, currentBinding.groups);
        }

        inputAction.Enable();
    }

    protected override void OnActivated(ActivateEventArgs args)
    {
        base.OnActivated(args);
        ResetGrabbingRotation();
    }

    public void ResetGrabbingRotation()
    {
        attachTransform.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// Process multiple grab influences down to a single pose
    /// </summary>
    /// <param name="influences">Poses of all interactors selecting this object, in world space.  Sorted in chronological order. There will always be at least two poses.</param>
    /// <param name="relativeInfluences">Poses of all interactors selecting this object, relative to their individual attach points</param>
    /// <returns>The desired 'final' pose to be used to position the interactable</returns>
    public override Pose ProcessesMultiGrab(List<Pose> influences)
    {
        var primaryPose = influences[0];
        var finalPos = primaryPose.position;
        var finalRot = primaryPose.rotation;

        if (influences.Count > 1)
        {
            var secondaryPose = influences[1];
            finalRot = GetFinalRotation(secondaryPose, primaryPose, secondaryPose.position - primaryPose.position);
        }

        var finalPose = new Pose(finalPos, finalRot);
        return finalPose;
    }

    Quaternion GetFinalRotation(Pose secondaryPose, Pose primaryPose, Vector3 grabAxis)
    {
        var alignAxis = attachTransform.up;
        if (Vector3.Dot(alignAxis, grabAxis) < 0)
        {
            grabAxis *= -1f;
        }

        var upToHands = Quaternion.FromToRotation(alignAxis, grabAxis);

        var finalRot = upToHands * attachTransform.rotation;

        var alignForward = finalRot * Vector3.forward;
        bool primaryIsMainInfluence = true;

        var attachParent = attachTransform.parent;
        var primarySelectLocalY = attachParent.InverseTransformPoint(primaryPose.position).y;
        var secondarySelectLocalY = attachParent.InverseTransformPoint(secondaryPose.position).y;
        switch (m_TwistControlMode)
        {
            case GrabControlMode.Top:
                primaryIsMainInfluence = primarySelectLocalY > secondarySelectLocalY;
                break;
            case GrabControlMode.Bottom:
                primaryIsMainInfluence = primarySelectLocalY < secondarySelectLocalY;
                break;
            case GrabControlMode.First:
                primaryIsMainInfluence = true;
                break;
            case GrabControlMode.Second:
                primaryIsMainInfluence = false;
                break;
            default:
                primaryIsMainInfluence = primarySelectLocalY > secondarySelectLocalY;
                break;

        }

        var mainInfluence = primaryIsMainInfluence ? primaryPose : secondaryPose;
        var secondaryInfluence = primaryIsMainInfluence ? secondaryPose : primaryPose;

        var mainInfluencePerpendicular = (1f - Mathf.Abs(Vector3.Dot(grabAxis, mainInfluence.forward)));
        var secondaryInfluencePerpendicular = (1f - Mathf.Abs(Vector3.Dot(grabAxis, secondaryInfluence.forward)));
        var mainInfluenceTwistWeight = (mainInfluencePerpendicular) / (mainInfluencePerpendicular + secondaryInfluencePerpendicular);
        var weightAverageTwistForward = Vector3.Slerp(secondaryInfluence.forward, mainInfluence.forward, mainInfluenceTwistWeight);
        
        var finalTwistForward = Vector3.Slerp(weightAverageTwistForward, mainInfluence.forward, mainInfluenceTwistWeight);
        var grabForward = Vector3.ProjectOnPlane(finalTwistForward, grabAxis);
        
        var forwardToForwad = Quaternion.FromToRotation(alignForward, grabForward);
        finalRot = forwardToForwad * finalRot;
        return finalRot;
    }
}
