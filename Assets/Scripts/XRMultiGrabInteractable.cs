using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRMultiGrabInteractable : XRGrabInteractable, IMultiInteractable
{
    static readonly List<Pose> k_InteractorPoses = new List<Pose>();


    List<XRBaseInteractor> m_SecondaryInteractors = new List<XRBaseInteractor>();

    bool m_SecondarySelection = false;

    protected override void Awake()
    {
        base.Awake();

        // We must have an attach point, so make one if it's not set or is the object itself
        if (attachTransform == null || attachTransform == transform)
        {
            var newAttach = new GameObject("Attach").transform;
            newAttach.parent = transform;
            attachTransform = newAttach;
        }
    }
    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        // No grab? Use base
        if (!isSelected)
        {
            base.OnSelectEntering(args);
            return;
        }

        // Existing grab? Just store the interactor
        m_SecondaryInteractors.Add(args.interactor);
        m_SecondarySelection = true;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        // Pre-existing selection - we ignore this
        if (!m_SecondarySelection)
        {
            base.OnSelectEntered(args);
        }
        else
        {
            // Hack - If using a dynamic attach, re-trigger the recentering code
            RecenterDynamicAttach();
        }
        m_SecondarySelection = false;
    }

    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        // If a secondary selection is exiting, just remove from the list
        var index = m_SecondaryInteractors.IndexOf(args.interactor);
        if (index == -1)
        {
            base.OnSelectExiting(args);
            return;
        }

        RemoveSecondarySelection(index);
        m_SecondarySelection = true;
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        // secondary selection - ignore
        if (!m_SecondarySelection)
        {
            base.OnSelectExited(args);

            // Transfer the secondary interactor over to this object if it exists
            // If not, disable the secondary interactable so it doesn't go around stealing inputs early
            if (m_SecondaryInteractors.Count > 0)
            {
                var primaryInteractor = m_SecondaryInteractors[0];
                RemoveSecondarySelection(0);
                interactionManager.ForceSelect(primaryInteractor, this);
            }
        }
        else
        {
            m_SecondarySelection = false;
            // Hack - If using a dynamic attach, re-trigger the recentering code
            RecenterDynamicAttach();
        }
    }

    /// <summary>
    /// Remove the secondary selection at the given index from the list 
    /// </summary>
    /// <param name="index">Selection index</param>
    private void RemoveSecondarySelection(int index)
    {
        m_SecondaryInteractors.RemoveAt(index);
    }

    // Process function that looks for secondary interactor and mediates the attach point
    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        // Cache the local position of the attach transform as we may be messing with it
        var localAttachPosition = attachTransform.localPosition;
        var localAttachRotation = attachTransform.localRotation;

        if (isSelected)
        {
            // If the secondary interactors are available perform multi-grab processing
            if (m_SecondaryInteractors.Count > 0)
            {
                var primaryTransform = selectingInteractor.attachTransform;

                k_InteractorPoses.Clear();

                // The first interactor pose is the identity, as everything is in primary-interactor space
                k_InteractorPoses.Add(new Pose(primaryTransform.position, primaryTransform.rotation));

                // For each secondary interactor
                // We transform the secondary attach point as if the primary interactor has complete control
                // We then tranform the secondary interactor positions into this local space
                for (var i = 0; i < m_SecondaryInteractors.Count; i++)
                {
                    var secondaryTransform = m_SecondaryInteractors[i].attachTransform;
                    k_InteractorPoses.Add(new Pose(secondaryTransform.position, secondaryTransform.rotation));
                }

                var finalPose = ProcessesMultiGrab(k_InteractorPoses);

                // Put final pose into primary transform space
                attachTransform.position = attachTransform.position + (primaryTransform.position - finalPose.position);
                attachTransform.rotation = attachTransform.rotation * Quaternion.Inverse(Quaternion.Inverse(primaryTransform.rotation) * finalPose.rotation);
                k_InteractorPoses.Clear();
            }
        }
        base.ProcessInteractable(updatePhase);

        // Restore the attach transform back to normal
        attachTransform.localPosition = localAttachPosition;
        attachTransform.localRotation = localAttachRotation;
    }

    /// <summary>
    /// Process multiple grab influences down to a single pose
    /// </summary>
    /// <param name="influences">Poses of all interactors selecting this object, in world space.  Sorted in chronological order. There will always be at least two poses.</param>
    /// <param name="relativeInfluences">Poses of all interactors selecting this object, relative to their individual attach points</param>
    /// <returns>The desired 'final' pose to be used to position the interactable</returns>
    public virtual Pose ProcessesMultiGrab(List<Pose> influences)
    {
        // Average positions and rotations together
        var finalPose = new Pose((influences[0].position + influences[1].position) * 0.5f, Quaternion.Slerp(influences[0].rotation, influences[1].rotation, 0.5f));
        
        return finalPose;
    }

    void RecenterDynamicAttach()
    {
        // Hack - If using a dynamic attach, re-trigger the recentering code
        var dynamicAttach = attachTransform.GetComponent<DynamicAttach>();
        if (dynamicAttach != null)
        {
            var primaryTransform = selectingInteractor.attachTransform;
            if (m_SecondaryInteractors.Count > 0)
            {
                k_InteractorPoses.Clear();

                // The first interactor pose is the identity, as everything is in primary-interactor space
                k_InteractorPoses.Add(new Pose(primaryTransform.position, primaryTransform.rotation));

                // For each secondary interactor
                // We transform the secondary attach point as if the primary interactor has complete control
                // We then tranform the secondary interactor positions into this local space
                for (var i = 0; i < m_SecondaryInteractors.Count; i++)
                {
                    var secondaryTransform = m_SecondaryInteractors[i].attachTransform;
                    k_InteractorPoses.Add(new Pose(secondaryTransform.position, secondaryTransform.rotation));
                }

                var finalPose = ProcessesMultiGrab(k_InteractorPoses);
                dynamicAttach.Recenter(finalPose.position, finalPose.rotation);
            }
            else
                dynamicAttach.Recenter(primaryTransform.position, primaryTransform.rotation);
        }
    }
}
