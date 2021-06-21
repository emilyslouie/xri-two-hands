using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Unity.SpatialFramework.Rendering
{
    public class InteractableVisualsController : MonoBehaviour
    {
        const float k_ShineTime = 0.5f;

#pragma warning disable 649
        [SerializeField]
        [Tooltip("The hover audio source.")]
        AudioSource m_AudioHover;

        [SerializeField]
        [Tooltip("The click audio source.")]
        AudioSource m_AudioClick;

        [SerializeField]
        [Tooltip("Material capture settings")]
        HighlightController m_HighlightController = new HighlightController();

        [SerializeField]
        [Tooltip("The outline highlight for selection.")]
        OutlineHighlight m_OutlineHighlight;

        [SerializeField]
        [Tooltip("The material highlight for hover.")]
        MaterialHighlight m_MaterialHighlight;

        [SerializeField]
        [Tooltip("The outline hover color.")]
        Color m_HoverColor = new Color(0.09411765f, 0.4392157f, 0.7137255f, 1f);

        [SerializeField]
        [Tooltip("The outline selection color.")]
        Color m_SelectionColor = new Color(1f, 0.4f, 0f, 1f);

        [SerializeField]
        [Tooltip("To play material activate anim.")]
        bool m_PlayMaterialActivateAnim = false;

        [SerializeField]
        [Tooltip("To play outline activate anim.")]
        bool m_PlayOutlineActivateAnim = false;
#pragma warning restore 649

        XRGrabInteractable m_GrabInteractable;

        Transform m_Transform;
        Material m_PulseMaterial;
        bool m_Selected;
        bool m_Hovering;
        float m_StartingAlpha;
        float m_StartingWidth;

        bool m_PlayShine = false;
        float m_ShineTimer = 0.0f;

        void Awake()
        {
            // Find the grab interactable
            m_GrabInteractable = GetComponentInParent<XRGrabInteractable>();

            // Hook up to events 
            m_GrabInteractable.firstHoverEntered.AddListener(PerformEntranceActions);
            m_GrabInteractable.lastHoverExited.AddListener(PerformExitActions);
            m_GrabInteractable.selectEntered.AddListener(PerformSelectEnteredActions);
            m_GrabInteractable.selectExited.AddListener(PerformSelectExitedActions);
            m_GrabInteractable.activated.AddListener(PerformActivatedActions);
            m_GrabInteractable.deactivated.AddListener(PerformDeactivatedActions);

            // Cache materials for highlighting
            m_HighlightController.RendererSource = m_GrabInteractable.transform;

            // Tell the highlight objects to get renderers starting at the grab interactable down
            if (m_MaterialHighlight != null)
            {
                m_HighlightController.RegisterCacheUser(m_MaterialHighlight);
                m_PulseMaterial = m_MaterialHighlight.HighlightMaterial;

                if (m_PulseMaterial != null)
                    m_StartingAlpha = m_PulseMaterial.GetFloat("_PulseMinAlpha");
            }
            if (m_OutlineHighlight != null)
                m_HighlightController.RegisterCacheUser(m_OutlineHighlight);

            m_HighlightController.Initialize();
            m_StartingWidth = m_OutlineHighlight.outlineScale;
        }

        void Update()
        {
            m_HighlightController.Update();
            if (m_MaterialHighlight != null)
            {
                if (m_Hovering || m_Selected)
                {
                    var vec = new Vector3(m_Transform.position.x, m_Transform.position.y, m_Transform.position.z);
                    m_PulseMaterial.SetVector("_Center", vec);
                }

                // Do timer count up/count down
                if (m_PlayShine)
                {
                    m_ShineTimer += Time.deltaTime;

                    var shinePercent = Mathf.Clamp01(m_ShineTimer / k_ShineTime);
                    var shineValue = Mathf.PingPong(shinePercent, 0.5f) * 2.0f;

                    m_PulseMaterial.SetFloat("_PulseMinAlpha", Mathf.Lerp(m_StartingAlpha, 1f, shineValue));

                    if (shinePercent >= 1.0f)
                    {
                        m_PlayShine = false;
                        m_ShineTimer = 0.0f;
                    }
                }
            }
        }

        void PerformEntranceActions(HoverEnterEventArgs arg0)
        {
            if (m_AudioHover != null)
                m_AudioHover.Play();

            if (m_MaterialHighlight != null)
                m_PulseMaterial.color = m_HoverColor;

            if (m_OutlineHighlight != null)
                m_OutlineHighlight.outlineColor = m_HoverColor;

            m_HighlightController.Highlight();

            m_Transform = arg0.interactor.transform;
            m_Hovering = true;
        }

        void PerformExitActions(HoverExitEventArgs arg0)
        {
            if (!m_Selected)
                m_HighlightController.Unhighlight();

            m_Hovering = false;
        }

        void PerformSelectEnteredActions(SelectEnterEventArgs arg0)
        {
            if (m_AudioClick != null)
                m_AudioClick.Play();

            if (m_OutlineHighlight != null)
            {
                m_OutlineHighlight.outlineColor = m_SelectionColor;
                m_OutlineHighlight.PlayPulseAnimation();
            }

            if (m_MaterialHighlight != null)
                m_PulseMaterial.color = m_SelectionColor;

            m_Selected = true;
        }

        void PerformSelectExitedActions(SelectExitEventArgs arg0)
        {
            if (m_Hovering)
            {
                if (m_OutlineHighlight != null)
                    m_OutlineHighlight.outlineColor = m_HoverColor;
                if (m_MaterialHighlight != null)
                    m_PulseMaterial.color = m_HoverColor;

                m_OutlineHighlight.PlayPulseAnimation();
            }
            else
                m_HighlightController.Unhighlight();

            m_Selected = false;
        }

        void PerformActivatedActions(ActivateEventArgs arg0)
        {
            if (m_OutlineHighlight != null)
            {
                if (m_PlayMaterialActivateAnim)
                    m_PlayShine = true;

                if (m_PlayOutlineActivateAnim)
                {
                    m_OutlineHighlight.outlineScale = 1f;
                    m_OutlineHighlight.PlayPulseAnimation();
                }
            }
        }

        void PerformDeactivatedActions(DeactivateEventArgs arg0)
        {
            if (m_OutlineHighlight != null)
            {
                if (m_PlayOutlineActivateAnim)
                {
                    m_OutlineHighlight.outlineScale = m_StartingWidth;
                    m_OutlineHighlight.PlayPulseAnimation();
                }
            }
        }
    }
}
