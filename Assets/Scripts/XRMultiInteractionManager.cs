using UnityEngine.XR.Interaction.Toolkit;

public class XRMultiInteractionManager : XRInteractionManager
{
    readonly SelectEnterEventArgs m_SelectEnterEventArgs = new SelectEnterEventArgs();

    public override void SelectEnter(XRBaseInteractor interactor, XRBaseInteractable interactable)
    {
        // If this interactable does not support multi-interaction, or is not selected, then just go down the traditional path
        // Similarly, any interactor that requires exclusive mode does not support multi grab
        if (!(interactable is IMultiInteractable) || !interactable.isSelected || interactor.requireSelectExclusive || (interactable.isSelected && interactable.selectingInteractor.requireSelectExclusive))
        {
            base.SelectEnter(interactor, interactable);
            return;
        }

        // Otherwise, we report a selection without unregistering the last selection - the multi grab interactable will sort out interactor issues here
        m_SelectEnterEventArgs.interactor = interactor;
        m_SelectEnterEventArgs.interactable = interactable;
        SelectEnter(interactor, interactable, m_SelectEnterEventArgs);
    }

    public void ForceDeselect(XRBaseInteractor interactor)
    {
        if (interactor.selectTarget)
            SelectExit(interactor, interactor.selectTarget);
    }
}
