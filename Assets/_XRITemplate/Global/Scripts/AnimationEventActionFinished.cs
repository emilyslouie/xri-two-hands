using UnityEngine;

namespace Unity.XRContent.Animation
{
    /// <summary>
    /// Enables a monobehaviour to react to the 'ActionFinished' animation event
    /// </summary>
    public interface IAnimationEventActionFinished
    {
        void ActionFinished(string label);
    }

    /// <summary>
    /// Calls the 'ActionFinished' function on any supported monobehaviour when the target animation exits
    /// </summary>
    public class AnimationEventActionFinished : StateMachineBehaviour
    {
        [SerializeField]
        [Tooltip("A label identifying the animation that has finished.")]
        string m_Label;

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var eventReceiver = animator.GetComponentInParent<IAnimationEventActionFinished>();
            if (eventReceiver != null)
                eventReceiver.ActionFinished(m_Label);
        }
    }
}
