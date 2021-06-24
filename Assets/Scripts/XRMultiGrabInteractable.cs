using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRMultiGrabInteractable : XRGrabInteractable, IMultiInteractable
{
    [SerializeField, Range(0f, 1f)]
    float m_LerpParameter = 0.5f;

    public float lerpParameter
    {
        get => m_LerpParameter;
        set => m_LerpParameter = value;
    }

    [SerializeField]
    float m_Dot;

    [SerializeField]
    bool m_ShortWay;

    [SerializeField]
    int m_SlerpMethod;

    [SerializeField]
    bool m_InverseQuaternion0;
    [SerializeField]
    bool m_InverseQuaternion1;
    [SerializeField]
    bool m_InverseQuaternionSlerp;
    [SerializeField]
    bool m_InverseQuaternionFinal;

    readonly List<Pose> k_InteractorPoses = new List<Pose>();
    protected List<Pose> interactorPoses => k_InteractorPoses;

    readonly List<XRBaseInteractor> m_SecondaryInteractors = new List<XRBaseInteractor>();
    protected List<XRBaseInteractor> secondaryInteractors => m_SecondaryInteractors;

    protected bool m_SecondarySelection;
    protected bool secondarySelection => m_SecondarySelection;

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

                Pose? attachTransformPose = null;
                Pose? originalAttachTransformPose = null;
                Transform originalAttachTransform = null;
                if (primaryInteractor is XRRayInteractor)
                {
                    attachTransformPose = new Pose(primaryInteractor.attachTransform.position, primaryInteractor.attachTransform.rotation);
                    originalAttachTransform = primaryInteractor.transform.Find($"[{primaryInteractor.name}] Original Attach");
                    if (originalAttachTransform != null)
                        originalAttachTransformPose = new Pose(originalAttachTransform.position, originalAttachTransform.rotation);
                }

                interactionManager.ForceSelect(primaryInteractor, this);

                if (attachTransformPose.HasValue)
                    primaryInteractor.attachTransform.SetPositionAndRotation(attachTransformPose.Value.position, attachTransformPose.Value.rotation);

                if (originalAttachTransformPose.HasValue)
                    originalAttachTransform.SetPositionAndRotation(originalAttachTransformPose.Value.position, originalAttachTransformPose.Value.rotation);
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
                if (m_InverseQuaternionFinal)
                {
                    attachTransform.rotation = Quaternion.Inverse(attachTransform.rotation);
                }
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
        m_Dot = Quaternion.Dot(influences[0].rotation, influences[1].rotation);

        // Average positions and rotations together
        var position = Vector3.Lerp(influences[0].position, influences[1].position, m_LerpParameter);
        var rotation0 = m_InverseQuaternion0 ? Quaternion.Inverse(influences[0].rotation) : influences[0].rotation;
        var rotation1 = m_InverseQuaternion1 ? Quaternion.Inverse(influences[1].rotation) : influences[1].rotation;
        //var rotation = m_Dot < 0f ? Slerp(rotation0, rotation1, m_LerpParameter, m_ShortWay) : Quaternion.Slerp(rotation0, rotation1, m_LerpParameter);
        //var rotation = m_SlerpMethod == 0 ? Quaternion.Slerp(rotation0, rotation1, m_LerpParameter) : Slerp(rotation0, rotation1, m_LerpParameter, m_ShortWay);
        var rotation = Slerp(rotation0, rotation1, m_LerpParameter);
        if (m_InverseQuaternionSlerp)
            rotation = Quaternion.Inverse(rotation);
        var finalPose = new Pose(position, rotation);

        return finalPose;
    }

    void RecenterDynamicAttach()
    {
        // Hack - If using a dynamic attach, re-trigger the recentering code
        var dynamicAttach = attachTransform.GetComponent<DynamicAttach>();
        if (dynamicAttach != null && dynamicAttach.enabled)
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

    static Quaternion Invert(Quaternion q)
    {
        return new Quaternion(-q.x, -q.y, -q.z, -q.w);
    }

    Quaternion Slerp(Quaternion p, Quaternion q, float t)
    {
        switch (m_SlerpMethod)
        {
            case 0:
                return m_Dot < 0f ? Slerp(p, q, t, m_ShortWay) : Quaternion.Slerp(p, q, t);
            case 1:
                return Quaternion.Slerp(p, q, t);
            default:
                return Slerp(p, q, t, m_ShortWay);
        }
    }

    static Quaternion Slerp(Quaternion p, Quaternion q, float t, bool shortWay)
    {
        float dot = Quaternion.Dot(p, q);
        float sign = (shortWay && dot < 0.0f) ? -1f : 1f;
        float angle = Mathf.Acos(dot * sign);
        float division = 1f / Mathf.Sin(angle);
        float t0 = Mathf.Sin((1f - t) * angle) * division * sign;
        float t1 = Mathf.Sin((t) * angle) * division;
        return new Quaternion(
            p.x * t0 + q.x * t1,
            p.y * t0 + q.y * t1,
            p.z * t0 + q.z * t1,
            p.w * t0 + q.w * t1
        );
    }
}