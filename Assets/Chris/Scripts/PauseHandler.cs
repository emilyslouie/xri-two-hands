using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PauseHandler : MonoBehaviour
{
    [SerializeField]
    InputActionReference m_PauseAction;

    public InputActionReference pauseAction
    {
        get => m_PauseAction;
        set => m_PauseAction = value;
    }

    protected void Awake()
    {
        if (m_PauseAction == null)
        {
            Debug.LogError("Missing reference to Pause action.", this);
            enabled = false;
        }
    }

    protected void Update()
    {
        var pressed = IsPressed(m_PauseAction);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPaused = pressed;
#endif
    }

    protected virtual bool IsPressed(InputAction action)
    {
        if (action == null)
            return false;

#if INPUT_SYSTEM_1_1_OR_NEWER
                return action.phase == InputActionPhase.Performed;
#else
        if (action.activeControl is ButtonControl buttonControl)
            return buttonControl.isPressed;

        if (action.activeControl is AxisControl)
            return action.ReadValue<float>() >= 0.5f;

        return action.triggered || action.phase == InputActionPhase.Performed;
#endif
    }
}
