using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRMultiPhysicsInteractable : XRBaseInteractable, IMultiInteractable
{
    static readonly List<Pose> k_InteractorPoses = new List<Pose>();

    List<XRBaseInteractor> m_SecondaryInteractors = new List<XRBaseInteractor>();

    protected bool m_SecondarySelection = false;
    [SerializeField]
    float m_Power = 10f;

    [SerializeField]
    Rigidbody m_RigidyBody;
    
    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        // No grab? Use base
        if (!isSelected)
        {
            base.OnSelectEntering(args);
            return;
        }

        // Existing grab? Just store the interactor
        m_SecondaryInteractors.Add(args.interactor);
        m_SecondarySelection = true;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        // Pre-existing selection - we ignore this
        if (!m_SecondarySelection)
        {
            base.OnSelectEntered(args);
        }
        else
        {

        }
        m_SecondarySelection = false;
    }

    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        // If a secondary selection is exiting, just remove from the list
        var index = m_SecondaryInteractors.IndexOf(args.interactor);
        if (index == -1)
        {
            base.OnSelectExiting(args);
            return;
        }

        RemoveSecondarySelection(index);
        m_SecondarySelection = true;
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        // secondary selection - ignore
        if (!m_SecondarySelection)
        {
            base.OnSelectExited(args);

            // Transfer the secondary interactor over to this object if it exists
            // If not, disable the secondary interactable so it doesn't go around stealing inputs early
            if (m_SecondaryInteractors.Count > 0)
            {
                var primaryInteractor = m_SecondaryInteractors[0];
                RemoveSecondarySelection(0);
                interactionManager.ForceSelect(primaryInteractor, this);
            }
        }
        else
        {
            m_SecondarySelection = false;
        }
    }

    /// <summary>
    /// Remove the secondary selection at the given index from the list 
    /// </summary>
    /// <param name="index">Selection index</param>
    private void RemoveSecondarySelection(int index)
    {
        m_SecondaryInteractors.RemoveAt(index);
    }

    // Process function that looks for secondary interactor and mediates the attach point
    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        if (!isSelected)
            return;

        var primaryTransform = selectingInteractor.attachTransform;

        k_InteractorPoses.Clear();

        // The first interactor pose is the identity, as everything is in primary-interactor space
        k_InteractorPoses.Add(new Pose(primaryTransform.position, primaryTransform.rotation));

        // For each secondary interactor
        // We transform the secondary attach point as if the primary interactor has complete control
        // We then tranform the secondary interactor positions into this local space
        for (var i = 0; i < m_SecondaryInteractors.Count; i++)
        {
            var secondaryTransform = m_SecondaryInteractors[i].attachTransform;
            k_InteractorPoses.Add(new Pose(secondaryTransform.position, secondaryTransform.rotation));
        }

        var rb = m_RigidyBody;
        for (int i1 = 0; i1 < k_InteractorPoses.Count; i1++)
        {
            var position = k_InteractorPoses[i1].position;
            var closestPosition = position;
            var attachDistanceSq = 10000000.0f;
            foreach (var collider1 in this.colliders)
            {
                var newPosition = collider1.ClosestPoint(position);
                var distanceSq = (newPosition - position).sqrMagnitude;
                if (distanceSq < attachDistanceSq)
                {
                    attachDistanceSq = distanceSq;
                    closestPosition = newPosition;
                }
            }

            position = closestPosition;

            rb.AddForceAtPosition(m_Power * (k_InteractorPoses[i1].position - position), position, ForceMode.Impulse);
            Debug.DrawLine(k_InteractorPoses[i1].position, position, Color.white, 0.2f);
        }
        
        // Put final pose into primary transform space
        k_InteractorPoses.Clear();
    }
}
