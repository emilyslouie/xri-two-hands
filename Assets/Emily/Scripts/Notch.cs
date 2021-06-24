using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace TwoHandedBow
{
    [RequireComponent(typeof(PullMeasurer))]
    public class Notch : XRSocketInteractor
    {
        [Range(0, 1)] public float releaseThreshold = 0.25f;

        public PullMeasurer PullMeasurer { get; private set; } = null;
        public bool IsReady { get; private set; } = false;

        private XRMultiInteractionManager CustomManager => interactionManager as XRMultiInteractionManager;

        protected override void Awake()
        {
            base.Awake();
            PullMeasurer = GetComponent<PullMeasurer>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            PullMeasurer.selectExited.AddListener(ReleaseArrow);

            PullMeasurer.Pulled.AddListener(MoveAttach);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            PullMeasurer.selectExited.RemoveListener(ReleaseArrow);
            PullMeasurer.Pulled.RemoveListener(MoveAttach);
        }

        public void ReleaseArrow(SelectExitEventArgs args)
        {
            if (selectTarget is Arrow && PullMeasurer.PullAmount > releaseThreshold)
            {
                CustomManager.ForceDeselect(this);
            }
        }

        public void MoveAttach(Vector3 pullPosition, float pullAmount)
        {
            attachTransform.position = pullPosition;

        }

        public void SetReady(BaseInteractionEventArgs args)
        {
            IsReady = args.interactable.isSelected;
        }

        public override bool CanSelect(XRBaseInteractable interactable)
        {
            return base.CanSelect(interactable) && CanHover(interactable) && IsArrow(interactable);
        }

        private bool IsArrow(XRBaseInteractable interactable)
        {
            return interactable is Arrow;
        }

        public override XRBaseInteractable.MovementType? selectedInteractableMovementTypeOverride
        {
            get { return XRBaseInteractable.MovementType.Instantaneous; }
        }

        public override bool requireSelectExclusive => false;
    }
}
