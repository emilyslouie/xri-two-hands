using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace Unity.XRContent.Interaction
{
    /// <summary>
    /// An interactable that can be pressed by a direct interactor
    /// </summary>
    public class XRButton : XRBaseInteractable
    {
        [SerializeField]
        [Tooltip("The object that is visually pressed down")]
        Transform m_Button = null;

        [SerializeField]
        [Tooltip("The distance the button can be pressed")]
        float m_PressDistance = 0.1f;

        [SerializeField]
        [Tooltip("Events to trigger when the button is pressed")]
        UnityEvent m_OnPress;

        [SerializeField]
        [Tooltip("Events to trigger when the button is released")]
        UnityEvent m_OnRelase;

        bool m_Hovered = false;
        bool m_Selected = false;

        /// <summary>
        /// The object that is visually pressed down
        /// </summary>
        public Transform Button { get { return m_Button; } set { m_Button = value; } }

        /// <summary>
        /// The distance the button can be pressed
        /// </summary>
        public float PressDistance { get { return m_PressDistance; } set { m_PressDistance = value; } }

        /// <summary>
        /// Events to trigger when the button is pressed
        /// </summary>
        public UnityEvent OnPress => m_OnPress;

        /// <summary>
        /// Events to trigger when the button is released
        /// </summary>
        public UnityEvent OnRelease => m_OnRelase;

        void Start()
        {
            SetButtonHeight(0.0f);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            selectEntered.AddListener(StartPress);
            selectExited.AddListener(EndPress);
            hoverEntered.AddListener(StartHover);
            hoverExited.AddListener(EndHover);
        }

        protected override void OnDisable()
        {
            selectEntered.RemoveListener(StartPress);
            selectExited.RemoveListener(EndPress);
            hoverEntered.AddListener(StartHover);
            hoverExited.AddListener(EndHover);
            base.OnDisable();
        }

        void StartPress(SelectEnterEventArgs args)
        {
            SetButtonHeight(-m_PressDistance);
            m_OnPress.Invoke();
            m_Selected = true;
        }

        void EndPress(SelectExitEventArgs args)
        {
            if (m_Hovered)
                m_OnRelase.Invoke();

            SetButtonHeight(0.0f);
            m_Selected = false;
        }

        void StartHover(HoverEnterEventArgs args)
        {
            m_Hovered = true;
            if (m_Selected)
                SetButtonHeight(-m_PressDistance);
        }

        void EndHover(HoverExitEventArgs args)
        {
            m_Hovered = false;
            SetButtonHeight(0.0f);
        }

        void SetButtonHeight(float height)
        {
            if (m_Button == null)
                return;

            Vector3 newPosition = m_Button.localPosition;
            newPosition.y = height;
            m_Button.localPosition = newPosition;
        }

        void OnDrawGizmosSelected()
        {
            var pressStartPoint = transform.position;
            var pressDownDirection = -transform.up;

            if (m_Button != null)
            {
                pressStartPoint = m_Button.position;
                pressDownDirection = -m_Button.up;
            }

            Gizmos.color = Color.green;
            Gizmos.DrawLine(pressStartPoint, pressStartPoint + (pressDownDirection * m_PressDistance));
        }

        void OnValidate()
        {
            SetButtonHeight(0.0f);
        }
    }
}
