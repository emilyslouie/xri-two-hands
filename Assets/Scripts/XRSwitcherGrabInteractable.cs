using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;

public class XRSwitcherGrabInteractable : XRGrabInteractable
{
    [FormerlySerializedAs("m_SecondaryInteractable")]
    [SerializeField]
    [Tooltip("Interactable that becomes available when this interactable is grabbed.")]
    XRBaseInteractable m_SecondaryEnableInteractable;
    
    [SerializeField]
    [Tooltip("Interactable that becomes unavailable when this interactable is grabbed.")]
    XRBaseInteractable m_SecondaryDisableInteractable;

    protected override void Awake()
    {
        base.Awake();

        // If no secondary interactable is available
        // Make a secondary grab interactable, with the same colliders as this one
        if (m_SecondaryEnableInteractable == null)
        {
            Debug.LogWarning("No secondary interactable specified, will act as a regular grab interactable");
        }

        m_SecondaryEnableInteractable.enabled = false;
        // Inject a select function that enables the secondary grab
        selectEntered.AddListener(SetupSecondarySelection);

        // Inject a deselect function that disables the secondary grab and potentially steals the interactor
        selectExited.AddListener(TeardownSecondarySelection);
    }

    public override bool IsSelectableBy(XRBaseInteractor interactor)
    {
        if (isSelected && interactor != selectingInteractor)
            return false;

        return base.IsSelectableBy(interactor);
    }

    void SetupSecondarySelection(SelectEnterEventArgs args)
    {
        // Enable the secondary interactable so it can start receiving input
        m_SecondaryDisableInteractable.enabled = false; // Disable before enabling because otherwise removing will unmap the collider from the new interactable
        m_SecondaryEnableInteractable.enabled = true;
    }

    void TeardownSecondarySelection(SelectExitEventArgs args)
    {
        // Transfer the secondary interactor over to this object if it exists
        // If not, disable the secondary interactable so it doesn't go around stealing inputs early
        var secondarySelectingInteractor = m_SecondaryEnableInteractable.selectingInteractor;
        if (secondarySelectingInteractor != null)
        {
            m_SecondaryEnableInteractable.enabled = false;
            m_SecondaryDisableInteractable.enabled = true;
            interactionManager.ForceSelect(secondarySelectingInteractor, m_SecondaryDisableInteractable);

        }
        else
        {
            m_SecondaryEnableInteractable.enabled = false;
            m_SecondaryDisableInteractable.enabled = true;
        }
    }
}
