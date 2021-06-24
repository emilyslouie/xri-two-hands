using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace TwoHandedBow
{
    public class PullMeasurer : XRBaseInteractable
    {
        // Hidden in Inspector, would need to be serializable, and added to custom editor
        public class PullEvent : UnityEvent<Vector3, float> { }
        public PullEvent Pulled = new PullEvent();

        public Transform start = null;
        public Transform end = null;

        private float pullAmount = 0.0f;
        public float PullAmount => pullAmount;

        private XRBaseInteractor pullingInteractor = null;

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);

            pullingInteractor = args.interactor;
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);

            pullingInteractor = null;

            SetPullValues(start.position, 0.0f);
        }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);

            if (isSelected)
            {
                if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
                    CheckForPull();
            }
        }

        private void CheckForPull()
        {
            Vector3 interactorPosition = pullingInteractor.transform.position;

            float newPullAmount = CalculatePull(interactorPosition);
            Vector3 newPullPosition = CalculatePosition(newPullAmount);

            SetPullValues(newPullPosition, newPullAmount);
        }

        private float CalculatePull(Vector3 pullPosition)
        {
            Vector3 pullDirection = pullPosition - start.position;
            Vector3 targetDirection = end.position - start.position;

            float maxLength = targetDirection.magnitude;
            targetDirection.Normalize();

            float pullValue = Vector3.Dot(pullDirection, targetDirection) / maxLength;
            pullValue = Mathf.Clamp(pullValue, 0.0f, 1.0f);

            return pullValue;
        }

        private Vector3 CalculatePosition(float amount)
        {
            return Vector3.Lerp(start.position, end.position, amount);
        }

        private void SetPullValues(Vector3 newPullPosition, float newPullAmount)
        {
            if (newPullAmount != pullAmount)
            {
                pullAmount = newPullAmount;
                Pulled?.Invoke(newPullPosition, newPullAmount);
            }
        }

        public override bool IsSelectableBy(XRBaseInteractor interactor)
        {
            return base.IsSelectableBy(interactor) && IsDirectInteractor(interactor);
        }

        private bool IsDirectInteractor(XRBaseInteractor interactor)
        {
            return interactor is XRDirectInteractor;
        }

        private void OnDrawGizmos()
        {
            if (start && end)
                Gizmos.DrawLine(start.position, end.position);
        }
    }
}
