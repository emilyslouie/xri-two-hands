using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VirtualTransformChild : MonoBehaviour
{
    [SerializeField]
    Transform m_VirtualParent;

    protected virtual void OnEnable()
    {
        Application.onBeforeRender += OnBeforeRender;
    }

    protected virtual void OnDisable()
    {
        Application.onBeforeRender -= OnBeforeRender;
    }

    [BeforeRenderOrder(XRInteractionUpdateOrder.k_BeforeRenderOrder + 2)]
    protected virtual void OnBeforeRender()
    {
        if (m_VirtualParent == null)
            return;

        transform.SetPositionAndRotation(m_VirtualParent.position, m_VirtualParent.rotation);
    }
}
