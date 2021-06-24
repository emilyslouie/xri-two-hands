using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRCompoundGrabInteractable : XRGrabInteractable
{
  [SerializeField]
  [Tooltip("Interactable that becomes available when this interactable is grabbed.")]
  XRBaseInteractable m_SecondaryInteractable;

  [SerializeField]
  bool m_AllowTransferSecondaryInteractor = true;

  protected override void Awake()
  {
    base.Awake();

    // If no secondary interactable is available
    // Make a secondary grab interactable, with the same colliders as this one
    if (m_SecondaryInteractable == null)
    {
      Debug.LogWarning("No secondary interactable specified, will act as a regular grab interactable");
    }

    m_SecondaryInteractable.enabled = false;
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
    m_SecondaryInteractable.enabled = true;
  }

  void TeardownSecondarySelection(SelectExitEventArgs args)
  {
    // Transfer the secondary interactor over to this object if it exists
    // If not, disable the secondary interactable so it doesn't go around stealing inputs early
    if (m_AllowTransferSecondaryInteractor && m_SecondaryInteractable.selectingInteractor != null)
    {
      interactionManager.ForceSelect(m_SecondaryInteractable.selectingInteractor, this);
    }
    else
      m_SecondaryInteractable.enabled = false;
  }
}
