using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRBowStringInteractable : XRBaseInteractable
{
  [SerializeField]
  Transform m_AttachTransform = default;
  [SerializeField]
  BowController m_BowController = default;
  /// <summary>
  /// The attachment point to use on this Interactable (will use this object's position if none set).
  /// </summary>
  public Transform attachTransform
  {
    get => m_AttachTransform;
    set => m_AttachTransform = value;
  }

  DynamicStringAttach m_DynamicStringAttach;
  bool m_AttachedToBowString = false;

  protected override void Awake()
  {
    base.Awake();

    m_DynamicStringAttach = m_AttachTransform.GetComponent<DynamicStringAttach>();
    // Inject a select function that enables the secondary grab
    selectEntered.AddListener(SetupBowStringAttach);

    // Inject a deselect function that disables the secondary grab and potentially steals the interactor
    selectExited.AddListener(LetGoOffBowString);

    hoverEntered.AddListener(BowStringHoverEnter);

    hoverExited.AddListener(BowStringHoverExit);
  }

  private void BowStringHoverExit(HoverExitEventArgs arg0)
  {
    //Debug.Log($"BowString Hover Exit, Interactor: {arg0.interactor.name} Interactable: {arg0.interactable.name}");
  }

  private void BowStringHoverEnter(HoverEnterEventArgs arg0)
  {
    //Debug.Log($"BowString Hover Enter, Interactor: {arg0.interactor.name} Interactable: {arg0.interactable.name}");
  }

  private void SetupBowStringAttach(SelectEnterEventArgs arg0)
  {
    if (m_AttachTransform != null)
    {
      m_AttachedToBowString = true;
      m_BowController.BowStringGripped();
    }
  }

  private void LetGoOffBowString(SelectExitEventArgs arg0)
  {
    if (m_AttachTransform != null)
    {
      m_AttachedToBowString = false;
      m_BowController.BowStringReleased();
    }
  }

  public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
  {
    base.ProcessInteractable(updatePhase);

    if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
    {
      //while grabbing the bowstring, update the attachpoint based on the controller
      if (m_AttachedToBowString && m_DynamicStringAttach != null)
      {
        var primaryTransform = selectingInteractor.attachTransform;
        m_DynamicStringAttach.Recenter(primaryTransform.position, primaryTransform.rotation);
        m_BowController.UpdateBowstringVisual(primaryTransform.position, m_DynamicStringAttach.transform.position);
      }
    }
  }

}
