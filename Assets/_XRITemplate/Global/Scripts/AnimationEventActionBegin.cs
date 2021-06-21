using UnityEngine;

namespace Unity.XRContent.Animation
{
    /// <summary>
    /// Enables a monobehaviour to react to the 'ActionBegin' animation event
    /// </summary>
    public interface IAnimationEventActionBegin
    {
        void ActionBegin(string label);
    }

    /// <summary>
    /// Calls the 'ActionFinished' function on any supported monobehaviour when the target animation exits
    /// </summary>
    public class AnimationEventActionBegin : StateMachineBehaviour
    {
        [SerializeField]
        [Tooltip("A label identifying the animation that has started.")]
        string m_Label;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var eventReceiver = animator.GetComponentInParent<IAnimationEventActionBegin>();
            if (eventReceiver != null)
                eventReceiver.ActionBegin(m_Label);
        }
    }
}
