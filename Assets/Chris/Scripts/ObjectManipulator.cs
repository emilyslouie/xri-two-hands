using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ObjectManipulator : XRMultiGrabInteractable
{
    [SerializeField]
    float m_DistanceScaleMultiplier = 1f;

    [SerializeField]
    bool m_AveragePoses = true;

    bool m_ComputeInitialDistance;

    float m_InitialDistance;
    Vector3 m_InitialLocalScale;

    /// <inheritdoc />
    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        base.OnSelectEntering(args);

        if (secondarySelection)
        {
            m_ComputeInitialDistance = true;
        }
    }

    /// <inheritdoc />
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        m_ComputeInitialDistance = false;
    }

    /// <inheritdoc />
    public override Pose ProcessesMultiGrab(List<Pose> influences)
    {
        var pose = m_AveragePoses ? base.ProcessesMultiGrab(influences) : influences[0];

        if (m_ComputeInitialDistance)
        {
            m_InitialDistance = Vector3.Distance(interactorPoses[0].position, interactorPoses[1].position);
            m_InitialLocalScale = transform.localScale;
            m_ComputeInitialDistance = false;
        }

        var distance = Vector3.Distance(influences[0].position, influences[1].position);
        var factor = !Mathf.Approximately(m_InitialDistance, 0f) ? distance / m_InitialDistance : 1f;
        transform.localScale = m_InitialLocalScale * factor * m_DistanceScaleMultiplier;

        return pose;
    }
}
