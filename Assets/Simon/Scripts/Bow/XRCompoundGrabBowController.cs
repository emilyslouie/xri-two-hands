using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class XRCompoundGrabBowController : XRCompoundGrabInteractable
{
  [Header("Bow Specific")]
  [SerializeField]
  private Collider m_BowStringCollider;
  [SerializeField]
  private XRSocketInteractor m_ArrowSocket;
  public Transform ArrowSocket { get => m_ArrowSocket.transform; }

  [SerializeField]
  private LineRenderer m_TargettingLaser;
  //[SerializeField, Range(1,179)]
  //private float m_MaxUpDownAngle = 30;
  //public float MaxUpDownAimAngle { get => m_MaxUpDownAngle; }
  //[SerializeField, Range(1, 179)]
  //private float m_MaxLeftRightAngle = 50;
  //public float MaxLeftRightAimAngle { get => m_MaxLeftRightAngle; }

  [SerializeField]
  private Transform m_ArrowPrefab;

  [Header("UI")]
  [SerializeField]
  private GameObject m_InfoCanvas;
  [SerializeField]
  private Image m_DrawbackPowerImg;
  [SerializeField]
  private Text m_DrawbackPowerText;

  [SerializeField]
  private Image m_PowerImg;
  [SerializeField]
  private Text m_PowerText;
  [SerializeField]
  private Text m_DeviationText;

  private XRMultiInteractionManager m_MultiInteractionManager;
  private XRBowStringInteractable m_XRBowStringInteractable;

  private bool m_bUnderTension = false;
  private bool m_bHasBeenSocketed = false;
  private Vector3 m_CurrentLaunchDirection;
  private float m_CurrentDrawback;
  private float m_CurrentPower;
  private Arrow m_InstancedArrow;

  protected override void Awake()
  {
    base.Awake();

    if (m_InfoCanvas != null)
    {
      m_InfoCanvas.SetActive(false);
    }

    m_TargettingLaser.enabled = false;

    //if (m_ArrowSocket != null)
    //  m_ArrowSocket.hoverEntered.AddListener(OnArrowSocketEntered);
    m_MultiInteractionManager = interactionManager as XRMultiInteractionManager;
    Debug.Assert(m_MultiInteractionManager != null, "InteractionManager is not of type XRMultiInteractionManager!", interactionManager);

    m_XRBowStringInteractable = m_SecondaryInteractable as XRBowStringInteractable;
    Debug.Assert(m_XRBowStringInteractable != null, "Secondary Interactable is not of type XRBowStringInteractable!", m_SecondaryInteractable);
  }

  private void Start()
  {
    m_XRBowStringInteractable.SetupBowString(this);
  }

  private void Update()
  {
    if(m_bUnderTension)
    {
      //Show how much of the maximum drawback is reached at this positions
      if (m_InfoCanvas.activeSelf && m_DrawbackPowerImg != null)
      {
        float perc = Mathf.Clamp01(m_XRBowStringInteractable.CurrentDrawback);
        m_DrawbackPowerImg.fillAmount = perc;
        m_DrawbackPowerText.text = string.Format("{0}%", Mathf.CeilToInt(perc * 100));

        perc = Mathf.Clamp01(m_XRBowStringInteractable.CurrentPower);
        m_PowerImg.fillAmount = perc;
        m_PowerText.text = string.Format("{0}%", Mathf.CeilToInt(perc * 100));

        m_DeviationText.text = string.Format("{0:F0}°", m_XRBowStringInteractable.LaunchDeviationAngle);
        m_DeviationText.color = m_XRBowStringInteractable.LaunchDeviationAngle >= m_XRBowStringInteractable.MisFireAngleThreshold ? Color.red : Color.white;
      }
    }
    ////While there is an arrow on the bow
    ////Check if there is tension on the bowstring, if not then release the arrow
    //if (m_InstancedArrow != null)
    //{
    //  //Get closest point on the bowstring collider
    //  Vector3 closestPosOnCollider = m_BowStringCollider.ClosestPoint(m_InstancedArrow.transform.position);
    //  //Get the direction that the bowstring is currently pulled in
    //  Vector3 dirBetweenColliderAndArrow = (closestPosOnCollider - m_InstancedArrow.transform.position);
    //  //Use dot product to determine if the direction of the arrow is in the opposide direction of the pull
    //  //if so then the bottom of the arrow is creating tension, otherwise we're "pulling" the bowstring, in that case, detach arrow
    //  float dot = Vector3.Dot(dirBetweenColliderAndArrow, m_InstancedArrow.transform.forward);
    //  if (dot < 0)
    //  {
    //    DetachArrow(false);
    //  }
    //  else //move the bowstring based on the push of the arrow
    //  {
    //    //Time for some hacky magic
    //    //check how far the hand is remove from the arrow bottom
    //    UpdateBowstringVisual(m_InstancedArrow.transform.position, closestPosOnCollider, false);
    //  }
    //}
  }

  ///// <summary>
  ///// 
  ///// </summary>
  ///// <param name="handWorldPos">Position of interactor in world space</param>
  ///// <param name="constrainedHandWorldPos">Position of attachpoint constrained to collider</param>
  //public void UpdateBowstringVisual(Vector3 handWorldPos, Vector3 constrainedHandWorldPos, bool setArrowPosToHandPos)
  //{
  //  Vector3 arrowPivotPosition = m_ArrowSocket.attachTransform.position;

  //  //Ideal arrow direction, use this to calculate drawback
  //  Vector3 arrowDirToPivot = arrowPivotPosition - m_InstancedArrow.transform.position;
  //  float totalDrawbackFromPivot = arrowDirToPivot.magnitude;
  //  Debug.DrawLine(arrowPivotPosition, m_InstancedArrow.transform.position, Color.blue);

  //  //account for the amount of space between the arrowpivot and the bowstring in "non drawback" position
  //  float minimumDrawback = (constrainedHandWorldPos - arrowPivotPosition).magnitude;
  //  Debug.DrawLine(arrowPivotPosition, constrainedHandWorldPos, Color.red);

  //  float actualDrawback = totalDrawbackFromPivot - minimumDrawback;
  //  float maximumAvailableDrawback = m_MaxDrawback - minimumDrawback;
  //  if (actualDrawback > maximumAvailableDrawback)
  //  {
  //    //if the drawback exceeds what can be done, detach the arrow and reset bow
  //    if (m_bHasBeenSocketed == false)
  //    {
  //      DetachArrow(false);
  //      return;
  //    }
  //    else
  //    {
  //      actualDrawback = maximumAvailableDrawback;
  //      handWorldPos = arrowPivotPosition + (-arrowDirToPivot.normalized * m_MaxDrawback);
  //    }
  //  }

  //  if (actualDrawback != m_CurrentDrawback)
  //  {
  //    m_CurrentDrawback = actualDrawback;
     

  //    m_CurrentPower = m_MaxPower * (actualDrawback / m_MaxDrawback);
  //  }

  //  m_CurrentLaunchDirection = arrowDirToPivot;

  //  //If the arrow is in the socket, use the socket attach as pivot point
  //  if (m_bHasBeenSocketed)
  //  {
  //    if (m_InstancedArrow != null && arrowDirToPivot != Vector3.zero)
  //    {
  //      m_InstancedArrow.transform.rotation = Quaternion.LookRotation(arrowDirToPivot.normalized, m_InstancedArrow.transform.up);
  //    }
  //  }

  //  if (setArrowPosToHandPos)
  //    m_InstancedArrow.transform.position = handWorldPos;
  //}

  //public static Vector3 ProjectVectorOnPlane(Vector3 planeNormal, Vector3 vector)
  //{
  //  return vector - (Vector3.Dot(vector, planeNormal) * planeNormal);
  //}

  //private void OnArrowSocketEntered(HoverEnterEventArgs arg0)
  //{
  //  if (m_InstancedArrow != null)
  //  {
  //    m_bHasBeenSocketed = true;
  //    m_ArrowSocket.enabled = false;

  //    if (m_InstancedArrow.GrabDistanceToBottom < 0.1f)
  //    {
  //      //Desregister from arrow releasing
  //      m_InstancedArrow.InteractAble.selectExited.RemoveListener(OnArrowReleased);
  //      m_InstancedArrow.InteractAble.enabled = false;

  //      //Enable our secondary interactable
  //      m_XRBowStringInteractable.enabled = true;
  //      //Select it
  //      m_XRBowStringInteractable.interactionManager.ForceSelect(m_InstancedArrow.InteractAble.selectingInteractor, m_XRBowStringInteractable);
  //      //Disable the arrow interactable

  //    }
  //  }
  //}


  void OnCollisionEnter(Collision collision)
  {
    if(m_XRBowStringInteractable.CheckForStringCollision(collision, out ContactPoint stringContact))
    {
      stringContact.otherCollider.enabled = false;
      Arrow arrow = stringContact.otherCollider.GetComponentInParent<Arrow>();
      Debug.Assert(arrow != null, "Had projectile string collision but it wasn't an Arrow!", stringContact.otherCollider);
      if(arrow != null)
      {
        XRBaseInteractor currentArrowInteractor = arrow.InteractAble.selectingInteractor;

        if (arrow.GrabDistanceToBottom < 0.1f)
        {
          m_MultiInteractionManager.ForceDeselect(currentArrowInteractor);
          m_MultiInteractionManager.ForceSelect(currentArrowInteractor, m_XRBowStringInteractable);
        }
        else
          m_MultiInteractionManager.ForceDeselect(currentArrowInteractor);

        m_XRBowStringInteractable.AttachArrow(arrow);

        
      }
    }
  }

  //private void SetNockedArrow(Arrow arrowObject, Vector3 contactPoint)
  //{
  //  m_InstancedArrow = arrowObject;
  //  m_InstancedArrow.InteractAble.selectExited.AddListener(OnArrowReleased);

  //  BowStringUnderTension();
  //  UpdateBowstringVisual(contactPoint, contactPoint, false);
  //}

  //private void DetachArrow(bool fireArrow)
  //{
  //  BowStringReleased();

  //  m_InstancedArrow.InteractAble.selectExited.RemoveListener(OnArrowReleased);
  //  m_bHasBeenSocketed = false;
  //  m_ArrowSocket.enabled = true;

  //  if (fireArrow)
  //    LaunchNockedArrow();
  //  else
  //  {
  //    m_InstancedArrow.ToggleFletchingCollider(true);
  //    m_InstancedArrow = null;
  //  }
  //}

  public void BowStringUnderTension()
  {
    if (m_InfoCanvas != null)
    {
      m_InfoCanvas.SetActive(true);
      m_DrawbackPowerImg.fillAmount = 0;
      m_DrawbackPowerText.text = "0%";
    }

    m_TargettingLaser.enabled = true;
    m_bUnderTension = true;
  }

  public void BowStringReleased()
  {
    if (m_InfoCanvas != null)
      m_InfoCanvas.SetActive(false);

        m_TargettingLaser.enabled = false;
    m_bUnderTension = false;
  }

  //public void OnBowGrabbed(SelectEnterEventArgs enterArgs)
  //{
  //  m_ArrowSocket.enabled = true;
  //}

  //public void OnBowReleased(SelectExitEventArgs exitArgs)
  //{
  //  //DetachArrow(false);
  //  m_ArrowSocket.enabled = false;
  //}

  //public void OnArrowReleased(SelectExitEventArgs exitArgs)
  //{
  //  //DetachArrow(true);
  //}
}
