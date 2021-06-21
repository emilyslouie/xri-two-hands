using Unity.XRContent.Animation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class TargetRing : MonoBehaviour, IAnimationEventActionBegin, IAnimationEventActionFinished
{
    static readonly string k_ActiveLabel = "active";

    [SerializeField]
    UnityEvent m_OnHit;

    [SerializeField]
    UnityEvent m_OnActive;

    [SerializeField]
    UnityEvent m_OnInactive;

    public void OnHit()
    {
        m_OnHit.Invoke();
    }

    void IAnimationEventActionBegin.ActionBegin(string label)
    {
        if (label == k_ActiveLabel)
            m_OnActive.Invoke();
    }

    void IAnimationEventActionFinished.ActionFinished(string label)
    {
        if (label == k_ActiveLabel)
            m_OnInactive.Invoke();
    }
}
