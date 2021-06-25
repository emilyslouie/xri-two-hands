using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Quiver : XRBaseInteractable
{
  [SerializeField]
  Transform m_ArrowPrefab;

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

  private void CreateAndSelectArrow(SelectEnterEventArgs arg0)
  {
    Arrow arrow = CreateArrow(arg0.interactor.transform);
    interactionManager.ForceSelect(arg0.interactor, arrow.InteractAble);
  }

  private Arrow CreateArrow(Transform interactor)
  {
    GameObject arrowObject = Instantiate(m_ArrowPrefab, interactor.position, interactor.rotation).gameObject;
    return arrowObject.GetComponent<Arrow>();
  }
}
