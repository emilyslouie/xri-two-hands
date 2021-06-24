using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRMultiGrabBowInteractable : XRMultiGrabInteractable
{
  /// <summary>
  /// Process multiple grab influences down to a single pose
  /// </summary>
  /// <param name="influences">Poses of all interactors selecting this object, in world space.  Sorted in chronological order. There will always be at least two poses.</param>
  /// <param name="relativeInfluences">Poses of all interactors selecting this object, relative to their individual attach points</param>
  /// <returns>The desired 'final' pose to be used to position the interactable</returns>
  public override Pose ProcessesMultiGrab(List<Pose> influences)
  {
    // Average positions and rotations together
    var finalPose = new Pose((influences[0].position + influences[1].position) * 0.5f, Quaternion.Slerp(influences[0].rotation, influences[1].rotation, 0.5f));

    return finalPose;
  }

}
