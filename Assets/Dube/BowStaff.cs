using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit;

namespace Dube
{
    public class BowStaff : XRMultiGrabInteractable
    {
        [SerializeField] private Transform m_alignAxis = null;
        [SerializeField] private GameObject m_trails = null;

        private Vector3 m_alignVector = Vector3.forward;
        private float m_lastCenter = float.MaxValue;
       
        protected override void Awake()
        {
            base.Awake();

            if (m_alignAxis != null)
                m_alignVector = m_alignAxis.forward;

            m_trails.SetActive(false);
        }
        
        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);

            m_trails.SetActive(true);
            
            // Force center to be recalculated
            m_lastCenter = float.MaxValue;
        }

        Quaternion saved = Quaternion.identity;
        
        protected override void OnSelectExiting(SelectExitEventArgs args)
        {
            // Primary interactor going away while there is still a secondary interactor?
            if (args.interactor == selectingInteractor && secondaryInteractors.Count > 0)
            {
                // Adjust the attachment point transform using a reverse order influences list to prepare for the primary hand swap  
                var influences = new List<Pose>()
                {
                    new Pose(secondaryInteractors[0].attachTransform.position, secondaryInteractors[0].attachTransform.rotation),
                    new Pose(selectingInteractor.attachTransform.position, selectingInteractor.attachTransform.rotation)
                };

                ProcessesMultiGrab(influences);
            }
            
            base.OnSelectExiting(args);
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);

            if (selectingInteractor == null)
            {
                m_trails.SetActive(false);
            }
        }

        public override Pose ProcessesMultiGrab(List<Pose> influences)
        {
            var pose0 = influences[0];
            var pose1 = influences[1];
            var scalarPosition0 = GetScalarPosition(pose0);
            var scalarPosition1 = GetScalarPosition(pose1);
            var dir = scalarPosition1 > scalarPosition0 ? 1.0f : -1.0f;

            // Reset last center if it is invalid
            if (m_lastCenter >= float.MaxValue)
                m_lastCenter = (scalarPosition0 + scalarPosition1) * 0.5f;
            
            // Clamp the hands to the staff
            var distance = (pose0.position - pose1.position).magnitude;
            var center = m_lastCenter;
            if (center + distance * 0.5f > 1.0f)
                center = 1.0f - distance * 0.5f;
            else if (center - distance * 0.5f < -1.0f)
                center = -1.0f + distance * 0.5f;

            // Align the staff to the hands
            var align = (pose1.position - pose0.position) * dir;
            attachTransform.localPosition = m_alignVector * (center + distance * 0.5f * -dir);
            attachTransform.localRotation = Quaternion.Inverse(Quaternion.FromToRotation(m_alignVector, Quaternion.Inverse(pose0.rotation) * align));
            
            // Since we already positioned the attach transform just return back pose0 so the attach transform does not change..
            return pose0;
        }

        private float GetScalarPosition (Pose pose) => 
            Vector3.Dot(pose.position - transform.position, transform.TransformDirection(m_alignVector));
    }
}