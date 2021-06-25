using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit;

namespace Dube
{
    public class BreakableStick : XRGrabInteractable, IMultiInteractable
    {
        [SerializeField] private Transform m_alignAxis = null;
        [SerializeField] private Transform m_secondaryAttachTransform = null;
        
        private class Hand
        {
            public float scalarPosition;
            public XRBaseInteractor interactor;
            public Transform attach;
            public InputAction grip;
        }

        private readonly Hand m_primaryHand = new Hand();
        private readonly Hand m_secondaryHand = new Hand();
        private Vector3 alignVector = Vector3.forward;
       
        protected override void Awake()
        {
            base.Awake();

            if (m_alignAxis != null)
                alignVector = m_alignAxis.forward;
           
            // We must have an attach point, so make one if it's not set or is the object itself
            if (attachTransform == null || attachTransform == transform)
            {
                var newAttach = new GameObject("PrimaryAttach").transform;
                newAttach.parent = transform;
                attachTransform = newAttach;
            }

            if (m_secondaryAttachTransform == null || m_secondaryAttachTransform == transform)
            {
                var newAttach = new GameObject("SecondaryAttach").transform;
                newAttach.parent = transform;
                m_secondaryAttachTransform = newAttach;
            }

            m_primaryHand.attach = attachTransform;
            m_secondaryHand.attach = m_secondaryAttachTransform;
        }

        protected override void OnSelectEntering(SelectEnterEventArgs args)
        {
            if (m_primaryHand.interactor == null)
                base.OnSelectEntering(args);

            if (m_secondaryHand.interactor != null)
                return;

            var hand = m_primaryHand.interactor == null ? m_primaryHand : m_secondaryHand; 
            hand.interactor = args.interactor;
            hand.attach.gameObject.SetActive(true);
            hand.scalarPosition = GetScalarHandPosition(args.interactor);

            var gripAction = hand.interactor.GetComponent<Dube.GripAction>();
            hand.grip = gripAction == null ? null : gripAction.gripAction.action;
            
            // Update the primary hand
            if(m_secondaryHand.interactor == null)
                m_primaryHand.attach.localPosition = Vector3.up * m_primaryHand.scalarPosition;
        }
        
        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            if(m_secondaryHand.interactor == null)
                base.OnSelectEntered(args);

            UpdateHands();
        }
        
        protected override void OnSelectExiting(SelectExitEventArgs args)
        {
            if(m_secondaryHand.interactor == null)
                base.OnSelectExiting(args);
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            // Last hand being removed?
            if (m_secondaryHand.interactor == null)
            {
                m_primaryHand.attach.gameObject.SetActive(false);
                m_primaryHand.interactor = null;
                base.OnSelectExited(args);
                return;
            }

            // Disable the secondary hand
            var secondaryInteractor = m_secondaryHand.interactor; 
            m_secondaryHand.interactor = null;
            m_secondaryHand.attach.gameObject.SetActive(false);

            // Primary hand being removed so copy the secondary hand data
            if (args.interactor != secondaryInteractor)
            {
                m_primaryHand.interactor = null;
                interactionManager.ForceSelect(secondaryInteractor, this);
            }
        }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            //var localAttachRotation = attachTransform.localRotation;
            //var localAttachPosition = attachTransform.localPosition; 
            UpdateHands();
            base.ProcessInteractable(updatePhase);
            //attachTransform.localRotation = localAttachRotation;
            //attachTransform.localPosition = localAttachPosition;
        }

        private float GetScalarHandPosition (XRBaseInteractor interactor) => 
            Vector3.Dot((interactor.attachTransform.position - transform.position), transform.up);
        
        private void UpdateHands()
        {
            if (m_secondaryHand.interactor == null)
                return;
            
            var align = m_secondaryHand.scalarPosition > m_primaryHand.scalarPosition ? 
                m_secondaryHand.interactor.attachTransform.position - m_primaryHand.interactor.attachTransform.position :
                m_primaryHand.interactor.attachTransform.position - m_secondaryHand.interactor.attachTransform.position;
            
            m_primaryHand.attach.localRotation = Quaternion.Inverse(Quaternion.FromToRotation(alignVector, Quaternion.Inverse(m_primaryHand.interactor.attachTransform.rotation) * align));

            var oldCenter = (m_primaryHand.scalarPosition + m_secondaryHand.scalarPosition) * 0.5f;
            var primaryScalar = GetScalarHandPosition(m_primaryHand.interactor);
            var secondaryScalar = GetScalarHandPosition(m_secondaryHand.interactor);

            var primaryGrip = m_primaryHand.grip == null ? 1.0f : m_primaryHand.grip.ReadValue<float>();
            var secondaryGrip = m_secondaryHand.grip == null ? 1.0f : m_secondaryHand.grip.ReadValue<float>();
            primaryGrip = 1.0f - Mathf.Clamp(primaryGrip, 0.0f, 1.0f);
            secondaryGrip = 1.0f - Mathf.Clamp(secondaryGrip, 0.0f, 1.0f);
            
            //oldCenter = oldCenter + primaryGrip * (primaryScalar - m_primaryHand.scalarPosition) + secondaryGrip * (secondaryScalar - m_secondaryHand.scalarPosition);
            
            var newDistance = Mathf.Abs(secondaryScalar - primaryScalar);
            var dir = primaryScalar < secondaryScalar ? -1.0f : 1.0f;

            if (oldCenter + newDistance * 0.5f > 1.0f)
                oldCenter = 1.0f - newDistance * 0.5f;
            else if (oldCenter - newDistance * 0.5f < -1.0f)
                oldCenter = -1.0f + newDistance * 0.5f;
            
            var newPrimaryScalar = oldCenter + newDistance * 0.5f * dir;
            var newSecondaryScalar = oldCenter + newDistance * -0.5f * dir;
            
            m_primaryHand.scalarPosition = newPrimaryScalar;
            m_secondaryHand.scalarPosition = newSecondaryScalar;
            m_primaryHand.attach.localPosition = Vector3.up * newPrimaryScalar;
            m_secondaryHand.attach.localPosition = Vector3.up * newSecondaryScalar;

            //m_primaryHand.attach.transform.localRotation = Quaternion.identity;
            
        }
    }
}