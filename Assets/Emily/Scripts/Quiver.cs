using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace TwoHandedBow
{
    public class Quiver : XRBaseInteractable
    {
        public GameObject arrowPrefab = null;

        protected override void OnEnable()
        {
            base.OnEnable();
            selectEntered.AddListener(CreateAndSelectArrow);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            selectEntered.RemoveListener(CreateAndSelectArrow);
        }

        private void CreateAndSelectArrow(SelectEnterEventArgs args)
        {
            Arrow arrow = CreateArrow(args.interactor.transform);
            interactionManager.ForceSelect(args.interactor, arrow);
        }

        private Arrow CreateArrow(Transform orientation)
        {
            GameObject arrowObject = Instantiate(arrowPrefab, orientation.position, orientation.rotation);
            return arrowObject.GetComponent<Arrow>();
        }
    }
}
