using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRMultiGrabStabilizedInteractable : XRMultiGrabInteractable
{
    Vector3 m_LastUp = Vector3.up;
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (m_SecondarySelection)
        {
            m_LastUp = selectingInteractor.attachTransform.up;
        }
            

        base.OnSelectEntered(args);
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        if (m_SecondarySelection)
        {
            m_LastUp = selectingInteractor.attachTransform.up;
        }

        base.OnSelectExited(args);
    }

    /// <summary>
    /// Process multiple grab influences down to a single pose
    /// </summary>
    /// <param name="influences">Poses of all interactors selecting this object, in world space.  Sorted in chronological order. There will always be at least two poses.</param>
    /// <param name="relativeInfluences">Poses of all interactors selecting this object, relative to their individual attach points</param>
    /// <returns>The desired 'final' pose to be used to position the interactable</returns>
    public override Pose ProcessesMultiGrab(List<Pose> influences)
    {
        // Position is the primary
        var position = influences[0].position;

        // Forward is the difference from second hand to first
        var forward = (influences[1].position - influences[0].position).normalized;
        
        // Get the unified up value from controllers
        var unifiedUp = Vector3.Slerp(influences[0].up, influences[1].up, 0.5f);
        var up = unifiedUp;

        // Three ranges - 0 - .5 - pure y (set lastY)
        // .5 to .75 - lerp land (update lastY)
        // .75 - 1 - lastY (update lastY)
        var angleDot = Mathf.Abs(Vector3.Dot(unifiedUp, forward));
        var upDot = Vector3.Dot(unifiedUp, m_LastUp);
        if (upDot < 0.0f)
            up = -up;

        const float k_LerpThreshHold = 0.5f;
        const float k_HistoryThreshhold = 0.75f;

        if (angleDot > k_LerpThreshHold)
        {
            if (angleDot < k_HistoryThreshhold)
            {
                // Lerp Land
                var lerpPercent = (angleDot - k_LerpThreshHold) / (k_HistoryThreshhold - k_LerpThreshHold);
                up = Vector3.Slerp(up, m_LastUp, lerpPercent);
            }
            else
            {
                up = m_LastUp;
            }
        }

        var left = Vector3.Cross(forward, up);
        up = Vector3.Cross(left, forward);
        var rotation = Quaternion.LookRotation(forward, up);


        //var up = SelectUpVector(influences[0], forward, ref m_PrimaryUp);
        //var secondaryUp = SelectUpVector(influences[1], forward, ref m_SecondaryUp);
        //up = Vector3.Slerp(up, secondaryUp, 0.5f);
        // Check to see if an axis-aligned up is too similar to the forward vector
        // In that case, we use the pure last-up value
        // Use controller up/down as hint for up y contribution is smaller
        var finalPose = new Pose(influences[0].position, rotation);
        m_LastUp = finalPose.up;

        return finalPose;
    }
}
