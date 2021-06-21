using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.XR.Interaction.Toolkit;

namespace Unity.XRContent.Interaction
{
    /// <summary>
    /// Component that when paired with an interactable will drive an associated timeline with the activate button
    /// Must be used with an action-based controller
    /// </summary>
    internal class InteractionAnimator : MonoBehaviour
    {
        // Many activate actions are configured as buttons - these constants are to replace those common bindings with analog versions appropriate for animation
        static readonly string[] k_ButtonLabels = { "triggerPressed", "triggerButton", "gripPressed", "gripButton" };
        static readonly string[] k_SwapLabels = { "{Trigger}", "{Trigger}", "{Grip}", "{Grip}" };

        [SerializeField]
        [Tooltip("The timeline to drive with the activation button")]
        PlayableDirector m_ToAnimate;

        bool m_Animating = false;
        InputAction m_InputAction;
        int totalBindings = 0;

        void Start()
        {
            // We want to hook up to the Select events so we can read data about the interacting controller
            var interactable = GetComponent<XRBaseInteractable>();
            if (interactable == null)
            {
                Debug.LogWarning($"No interactable on {name} - no animation will be played.");
                enabled = false;
                return;
            }

            if (m_ToAnimate == null)
            {
                Debug.LogWarning($"No timeline configured on {name} - no animation will be played.");
                enabled = false;
                return;
            }

            interactable.selectEntered.AddListener(OnSelect);
            interactable.selectExited.AddListener(OnDeselect);

            // We make a new input action to read from - we have to adapt the controller's typically binary input action
            m_InputAction = new InputAction("smoothAction");
            m_InputAction.Disable();
        }

        void Update()
        {
            if (m_Animating)
            {
                var floatValue = m_InputAction.ReadValue<float>();
                m_ToAnimate.time = floatValue;
            }
        }

        void OnSelect(SelectEnterEventArgs args)
        {
            // Get the controller from the interactor, and then the activation control from there
            var controllerInteractor = args.interactor as XRBaseControllerInteractor;
            if (controllerInteractor == null)
            {
                Debug.LogWarning($"Selected by {args.interactor.name}, which is not an XRBaseControllerInteractor");
                return;
            }
            var controller = controllerInteractor.xrController as ActionBasedController;
            if (controller == null)
            {
                Debug.LogWarning($"Selected by {controllerInteractor.xrController.name}, which is not an ActionBasedController");
                return;
            }

            // We grab the controller action and borrow its bindings - this is usually set to button so we can't just use this action directly
            var controllerBindings = controller.activateAction.action.bindings;
            totalBindings = controllerBindings.Count;
            foreach (var currentBinding in controllerBindings)
            {
                // Similarly, convert any button bindings to ones with analog readings
                var bindingString = currentBinding.path;
                for (var swapIndex = 0; swapIndex < k_SwapLabels.Length; swapIndex++)
                {
                    bindingString = bindingString.Replace(k_ButtonLabels[swapIndex], k_SwapLabels[swapIndex]);
                }

                m_InputAction.AddBinding(bindingString, currentBinding.interactions, currentBinding.processors, currentBinding.groups);
            }

            // Ready to animate
            m_ToAnimate.Play();
            m_InputAction.Enable();
            m_Animating = true;

        }

        void OnDeselect(SelectExitEventArgs args)
        {
            // Stop input from doing anything as the controller is no longer interacting with this object visibily
            m_InputAction.Disable();
            m_Animating = false;
            m_ToAnimate.Stop();

            while (totalBindings > 0)
            {
                totalBindings--;
                m_InputAction.ChangeBinding(totalBindings).Erase();
            }
        }
    }
}
