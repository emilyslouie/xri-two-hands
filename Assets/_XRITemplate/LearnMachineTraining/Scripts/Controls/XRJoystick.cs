using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace Unity.XRContent.Interaction
{
    /// <summary>
    /// An interactable joystick that can move side to side, and forward and back by a direct interactor
    /// </summary>
    public class XRJoystick : XRBaseInteractable
    {
        const float k_MaxDeadZonePercent = 0.9f;

        public enum JoystickType
        {
            BothCircle,
            BothSquare,
            FrontBack,
            LeftRight,
        }

        [Serializable]
        public class ValueChangeEvent : UnityEvent<float> { }

        [Tooltip("Controls how the joystick moves")]
        [SerializeField]
        JoystickType m_JoystickMotion = JoystickType.BothCircle;

        [SerializeField]
        [Tooltip("The object that is visually grabbed and manipulated")]
        Transform m_Handle = null;

        [SerializeField]
        [Tooltip("The value of the joystick")]
        Vector2 m_Value = Vector2.zero;

        [SerializeField]
        [Tooltip("If true, the joystick will return to center on release")]
        bool m_RecenterOnRelease = true;

        [SerializeField]
        [Tooltip("Maximum angle the joystick can move")]
        [Range(1.0f, 90.0f)]
        float m_MaxAngle = 60.0f;

        [SerializeField]
        [Tooltip("Minimum amount the joystick must move off the center to register changes")]
        [Range(1.0f, 90.0f)]
        float m_DeadZoneAngle = 10.0f;

        [SerializeField]
        [Tooltip("Events to trigger when the joystick's x value changes")]
        ValueChangeEvent m_OnValueChangeX = new ValueChangeEvent();

        [SerializeField]
        [Tooltip("Events to trigger when the joystick's y value changes")]
        ValueChangeEvent m_OnValueChangeY = new ValueChangeEvent();

        XRBaseInteractor m_Interactor = null;

        /// <summary>
        /// Controls how the joystick moves
        /// </summary>
        public JoystickType JoystickMotion { get { return m_JoystickMotion; } set { m_JoystickMotion = value; } }

        /// <summary>
        /// The object that is visually grabbed and manipulated
        /// </summary>
        public Transform Handle { get { return m_Handle; } set { m_Handle = value; } }

        /// <summary>
        /// The value of the joystick
        /// </summary>
        public Vector2 Value
        {
            get { return m_Value; }
            set
            {
                if (!m_RecenterOnRelease)
                {
                    SetValue(value);
                    SetHandleAngle(value * m_MaxAngle);
                }
            }
        }

        /// <summary>
        /// If true, the joystick will return to center on release
        /// </summary>
        public bool RecenterOnRelease { get { return m_RecenterOnRelease; } set { m_RecenterOnRelease = value; } }

        /// <summary>
        /// Maximum angle the joystick can move
        /// </summary>
        public float MaxAngle { get { return m_MaxAngle; } set { m_MaxAngle = value; } }

        /// <summary>
        /// Minimum amount the joystick must move off the center to register changes
        /// </summary>
        public float DeadZoneAngle { get { return m_DeadZoneAngle; } set { m_DeadZoneAngle = value; } }

        /// <summary>
        /// Events to trigger when the joystick's x value changes
        /// </summary>
        public ValueChangeEvent OnValueChangeX => m_OnValueChangeX;

        /// <summary>
        /// Events to trigger when the joystick's y value changes
        /// </summary>
        public ValueChangeEvent OnValueChangeY => m_OnValueChangeY;

        void Start()
        {
            if (m_RecenterOnRelease)
                SetHandleAngle(Vector2.zero);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            selectEntered.AddListener(StartGrab);
            selectExited.AddListener(EndGrab);
        }

        protected override void OnDisable()
        {
            selectEntered.RemoveListener(StartGrab);
            selectExited.RemoveListener(EndGrab);
            base.OnDisable();
        }

        private void StartGrab(SelectEnterEventArgs args)
        {
            m_Interactor = args.interactor;
        }

        private void EndGrab(SelectExitEventArgs arts)
        {
            UpdateValue();

            if (m_RecenterOnRelease)
            {
                SetHandleAngle(Vector2.zero);
                SetValue(Vector2.zero);
            }

            m_Interactor = null;
        }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);

            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            {
                if (isSelected)
                {
                    UpdateValue();
                }
            }
        }

        Vector3 GetLookDirection()
        {
            Vector3 direction = m_Interactor.transform.position - m_Handle.position;
            direction = transform.InverseTransformDirection(direction);
            switch (m_JoystickMotion)
            {
                case JoystickType.FrontBack:
                    direction.x = 0;
                    break;
                case JoystickType.LeftRight:
                    direction.z = 0;
                    break;
            }

            return direction.normalized;
        }

        void UpdateValue()
        {
            var lookDirection = GetLookDirection();

            // Get up/down angle and left/right angle
            var upDownAngle = Mathf.Atan2(lookDirection.z, lookDirection.y) * Mathf.Rad2Deg;
            var leftRightAngle = Mathf.Atan2(lookDirection.x, lookDirection.y) * Mathf.Rad2Deg;

            // Extract signs
            var signX = Mathf.Sign(leftRightAngle);
            var signY = Mathf.Sign(upDownAngle);

            upDownAngle = Mathf.Abs(upDownAngle);
            leftRightAngle = Mathf.Abs(leftRightAngle);

            // First clamp to square axes
            upDownAngle = Mathf.Min(upDownAngle, m_MaxAngle);
            leftRightAngle = Mathf.Min(leftRightAngle, m_MaxAngle);

            var stickValue = new Vector2(leftRightAngle / m_MaxAngle, upDownAngle / m_MaxAngle);

            // Clamp to circle range if desired
            if (m_JoystickMotion == JoystickType.BothCircle)
            {
                var stickMax = stickValue.normalized;

                stickValue.x = Mathf.Min(stickValue.x, stickMax.x);
                stickValue.y = Mathf.Min(stickValue.y, stickMax.y);

                leftRightAngle = stickValue.x * m_MaxAngle;
                upDownAngle = stickValue.y * m_MaxAngle;
            }

            // Apply deadzone
            var deadZone = m_DeadZoneAngle / m_MaxAngle;
            var aliveZone = (1.0f - deadZone);
            stickValue.x = (stickValue.x - deadZone) / aliveZone;
            stickValue.y = (stickValue.y - deadZone) / aliveZone;

            // Re-apply signs
            stickValue.x *= signX;
            stickValue.y *= signY;

            leftRightAngle *= signX;
            upDownAngle *= signY;

            SetHandleAngle(new Vector2(leftRightAngle, upDownAngle));
            SetValue(stickValue);
        }

        void SetValue(Vector2 value)
        {
            m_Value = value;
            m_OnValueChangeX.Invoke(m_Value.x);
            m_OnValueChangeY.Invoke(m_Value.y);
        }

        void SetHandleAngle(Vector2 angles)
        {
            if (m_Handle != null)
                m_Handle.localRotation = Quaternion.Euler(angles.y, 0.0f, -angles.x);
        }

        void OnDrawGizmosSelected()
        {
            var angleStartPoint = transform.position;

            if (m_Handle != null)
                angleStartPoint = m_Handle.position;

            const float k_AngleLength = 0.25f;

            if (m_JoystickMotion != JoystickType.LeftRight)
            {
                Gizmos.color = Color.green;
                var axisPoint1 = angleStartPoint + transform.TransformDirection(Quaternion.Euler(m_MaxAngle, 0.0f, 0.0f) * Vector3.up) * k_AngleLength;
                var axisPoint2 = angleStartPoint + transform.TransformDirection(Quaternion.Euler(-m_MaxAngle, 0.0f, 0.0f) * Vector3.up) * k_AngleLength;
                Gizmos.DrawLine(angleStartPoint, axisPoint1);
                Gizmos.DrawLine(angleStartPoint, axisPoint2);

                if (m_DeadZoneAngle > 0.0f)
                {
                    Gizmos.color = Color.red;
                    axisPoint1 = angleStartPoint + transform.TransformDirection(Quaternion.Euler(m_DeadZoneAngle, 0.0f, 0.0f) * Vector3.up) * k_AngleLength;
                    axisPoint2 = angleStartPoint + transform.TransformDirection(Quaternion.Euler(-m_DeadZoneAngle, 0.0f, 0.0f) * Vector3.up) * k_AngleLength;
                    Gizmos.DrawLine(angleStartPoint, axisPoint1);
                    Gizmos.DrawLine(angleStartPoint, axisPoint2);
                }
            }

            if (m_JoystickMotion != JoystickType.FrontBack)
            {
                Gizmos.color = Color.green;
                var axisPoint1 = angleStartPoint + transform.TransformDirection(Quaternion.Euler(0.0f, 0.0f, m_MaxAngle) * Vector3.up) * k_AngleLength;
                var axisPoint2 = angleStartPoint + transform.TransformDirection(Quaternion.Euler(0.0f, 0.0f, -m_MaxAngle) * Vector3.up) * k_AngleLength;
                Gizmos.DrawLine(angleStartPoint, axisPoint1);
                Gizmos.DrawLine(angleStartPoint, axisPoint2);

                if (m_DeadZoneAngle > 0.0f)
                {
                    Gizmos.color = Color.red;
                    axisPoint1 = angleStartPoint + transform.TransformDirection(Quaternion.Euler(0.0f, 0.0f, m_DeadZoneAngle) * Vector3.up) * k_AngleLength;
                    axisPoint2 = angleStartPoint + transform.TransformDirection(Quaternion.Euler(0.0f, 0.0f, -m_DeadZoneAngle) * Vector3.up) * k_AngleLength;
                    Gizmos.DrawLine(angleStartPoint, axisPoint1);
                    Gizmos.DrawLine(angleStartPoint, axisPoint2);
                }
            }
        }

        void OnValidate()
        {
            m_DeadZoneAngle = Mathf.Min(m_DeadZoneAngle, m_MaxAngle * k_MaxDeadZonePercent);
        }
    }
}