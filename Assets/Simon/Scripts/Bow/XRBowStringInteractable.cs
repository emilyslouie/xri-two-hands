using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRBowStringInteractable : XRBaseInteractable
{
  [Header("Bow String Specfic")]
  [SerializeField]
  private Transform m_HandAttachTransform = default;
  [SerializeField]
  private Collider m_BowStringCollider = default;

  [SerializeField, Range(0, 360)]
  private float m_MisFireAngleThreshold = 3;
  public float MisFireAngleThreshold { get => m_MisFireAngleThreshold; }

  [SerializeField, Range(0, 360)]
  private float m_PerfectAngleThreshold = 1;

  [SerializeField]
  private float m_MaxDrawback = 0.5f;
  public float MaxDrawback { get => m_MaxDrawback; }

  [SerializeField, Range(0, 0.99f)]
  private float m_LaunchDrawbackThreshold = 0.1f;
  public float LaunchDrawbackThreshold { get => m_LaunchDrawbackThreshold; }

  [SerializeField]
  private float m_MaxPower = 10;
  public float MaxPower { get => m_MaxPower; }

  [Header("String Visuals")]
  [SerializeField]
  private Transform m_StringVisualTop = default;
  [SerializeField]
  private Transform m_StringVisualBottom = default;
  [SerializeField]
  private Transform m_StringArrowAttachPoint = default;

  [Header("Audio")]
  [SerializeField]
  private AudioClip m_ArrowFiredClip = null;
  [SerializeField]
  private AudioSource m_StringSource = null;

  /// <summary>
  /// The attachment point to use on this Interactable (will use this object's position if none set).
  /// </summary>
  public Transform handAttachTransform
  {
    get => m_HandAttachTransform;
  }
  public Transform arrowAttachTransform
  {
    get => m_StringArrowAttachPoint;
  }

  public float CurrentDrawback { get; private set; }
  public float CurrentPower { get; private set; }
  public float LaunchDeviationAngle { get; private set; }

  private XRCompoundGrabBowController m_BowController = default;
  private DynamicStringAttach m_DynamicStringAttach = default;
  private Arrow m_AttachedArrow = default;
  private bool m_AttachedToBowString = false;

  private Vector3 m_FireDirection; //Perfect fire direction if the point on the bowstring would return linearly to the middle
  private Vector3 m_AltFireDirection; //More realist fire direction where the point on the bowstring returns to the closest point on the "idle" bowstring

  public void SetupBowString(XRCompoundGrabBowController bowController)
  {
    m_DynamicStringAttach = m_HandAttachTransform.GetComponent<DynamicStringAttach>();
    m_BowController = bowController;
    StringReset();
  }

  public bool CheckForStringCollision(Collision collision, out ContactPoint stringContact)
  {
    int contactPoints = collision.contactCount;
    for (int i = 0; i < contactPoints; i++)
    {
      ContactPoint contact = collision.GetContact(i);
      if (contact.thisCollider == m_BowStringCollider && contact.otherCollider.tag == "Projectile")
      {
        stringContact = contact;
        return true;
      }
    }
    stringContact = default;
    return false;
  }

  public void AttachArrow(Arrow arrow, bool disableInteractable = false)
  {
    if (arrow == null)
      return;

    m_AttachedArrow = arrow;
    m_AttachedArrow.InteractAble.enabled = !disableInteractable;
    m_AttachedArrow.SetKinematic();
    m_AttachedArrow.ToggleFletchingCollider(false);
    m_AttachedArrow.transform.SetParent(m_StringArrowAttachPoint);
    m_AttachedArrow.transform.localPosition = Vector3.zero;
    m_AttachedArrow.transform.forward = (m_BowController.ArrowSocket.position - m_StringArrowAttachPoint.position).normalized;
  }

  private void StringReset()
  {
    CurrentDrawback = 0;
    //Setup the bowstring pieces to look correct on wakeup
    UpdateBowstringVisual(m_BowStringCollider.transform.position);
  }

  protected override void OnEnable()
  {
    base.OnEnable();

    //Setup some functions on what to do when bowstring is grabbed
    selectEntered.AddListener(SetupBowStringAttach);
    selectExited.AddListener(LetGoOffBowString);

    //Setup some functions on what to do when bowstring is hovered above
    hoverEntered.AddListener(BowStringHoverEnter);
    hoverExited.AddListener(BowStringHoverExit);

    StringReset();
  }

  protected override void OnDisable()
  {
    base.OnDisable();

    selectEntered.RemoveListener(SetupBowStringAttach);
    selectExited.RemoveListener(LetGoOffBowString);

    hoverEntered.RemoveListener(BowStringHoverEnter);
    hoverExited.RemoveListener(BowStringHoverExit);

    StringReset();
  }

  private void SetupBowStringAttach(SelectEnterEventArgs arg0)
  {
    if (m_HandAttachTransform != null)
    {
      StringReset();
      m_AttachedToBowString = true;
      m_BowController.BowStringUnderTension();
    }
  }

  private void LetGoOffBowString(SelectExitEventArgs arg0)
  {
    if (m_HandAttachTransform != null)
    {
      if (m_AttachedArrow != null)
        CheckForLaunch();

      StringReset();

      m_AttachedToBowString = false;
      m_BowController.BowStringReleased();
    }
  }

  private void BowStringHoverExit(HoverExitEventArgs arg0)
  {
    if (m_AttachedArrow != null)
    {
      m_AttachedArrow.InteractAble.enabled = true;
    }
  }

  private void BowStringHoverEnter(HoverEnterEventArgs arg0)
  {
    if (m_AttachedArrow != null)
    {
      m_AttachedArrow.InteractAble.enabled = false;
    }
  }

  private void CheckForLaunch()
  {
    if (CurrentDrawback >= LaunchDrawbackThreshold)
    {
      //check if this is a misfire, if so add torgue to the arrow
      bool addTorque = LaunchDeviationAngle > m_MisFireAngleThreshold;
      //check if the launch deviation is withing the "perfect" launch zone, if so, set firedirection as straight as possible
      if (LaunchDeviationAngle <= m_PerfectAngleThreshold)
      {
        m_AltFireDirection = m_BowController.ArrowSocket.forward;
      }

      //if a misfire, dramatically weaking the power
      float power = m_MaxPower * CurrentPower * (addTorque ? 0.3f : 1);

      //if not a misfire, then set the arrow position instantly to where the socket it
      if (addTorque == false)
      {
        m_AttachedArrow.transform.position = m_BowController.ArrowSocket.position;
        m_AttachedArrow.transform.forward = m_AltFireDirection;
      }

      m_AttachedArrow.Fire(power, m_AltFireDirection.normalized, addTorque);
      m_AttachedArrow = null;

      m_StringSource.clip = m_ArrowFiredClip;
      m_StringSource.Play();
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
        var handControllerTransform = selectingInteractor.attachTransform;
        m_DynamicStringAttach.Recenter(handControllerTransform.position, handControllerTransform.rotation);
        Vector3 restrictedHandWorldPos = GetHandPositionBasedOnDrawback(handControllerTransform.position);
        UpdateBowstringVisual(restrictedHandWorldPos);
      }
    }
  }

  private Vector3 GetHandPositionBasedOnDrawback(Vector3 handWorldPos)
  {
    Vector3 drawbackCenter = m_BowStringCollider.transform.position;
    Vector3 drawbackDirection = (handWorldPos - drawbackCenter);
    Vector3 socketedAimDirection = (handWorldPos - m_BowController.ArrowSocket.position);

    float drawBack = (handWorldPos - drawbackCenter).magnitude;
    if (drawBack > MaxDrawback)
    {
      drawBack = MaxDrawback;
      handWorldPos = drawbackCenter + (drawbackDirection.normalized * MaxDrawback);
    }

    //float aimDeviation = Vector3.Angle(m_BowController.ArrowSocket.forward, socketedAimDirection);

    CurrentDrawback = drawBack / m_MaxDrawback;

    //ideal drawback is directly behind the center point, this represents max ideal drawback
    //the further removed from the center, the less the string is actual drawn back
    float actualDrawback = (m_DynamicStringAttach.transform.position - handWorldPos).magnitude;
    CurrentPower = actualDrawback / m_MaxDrawback;
    return handWorldPos;
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="handWorldPos">Position of interactor in world space</param>
  private void UpdateBowstringVisual(Vector3 handWorldPos)
  {
    DebugMenu.Instance.SetInfoText($"Hand WorldPos {handWorldPos}", 0);

    Vector3 anchorTop = m_StringVisualTop.position;
    Vector3 anchorBottom = m_StringVisualBottom.position;

    Vector3 topDir = (handWorldPos - anchorTop);
    float topLength = topDir.magnitude;
    DebugMenu.Instance.SetInfoText($"Top Pos {anchorTop} - Length {topLength}", 1);

    //Debug.DrawRay(anchorTop, topDir, Color.red);

    Vector3 bottomDir = (handWorldPos - anchorBottom);
    float bottomLength = bottomDir.magnitude;
    DebugMenu.Instance.SetInfoText($"Bot Pos {anchorBottom} - Length {bottomLength}", 2);

    //Debug.DrawRay(anchorBottom, bottomDir, Color.red);

    Vector3 topScale = m_StringVisualTop.localScale;
    topScale.y = topLength;
    m_StringVisualTop.localScale = topScale;
    //Vector3 forward = Quaternion.AngleAxis(90, Vector3.right) * topDir;
    //m_DrawVisualTop.rotation = Quaternion.LookRotation(forward.normalized, -topDir.normalized);

    //Mesh aligned on the Y axis
    Vector3 upVector = -topDir.normalized;
    //This part is tricky...I need to use the original forward vector but clean it up so
    //it is 90 degrees from the new Up Vector
    Vector3 forwardVector = ProjectVectorOnPlane(upVector, m_StringVisualTop.forward).normalized;
    m_StringVisualTop.rotation = Quaternion.LookRotation(forwardVector, upVector);


    Vector3 bottomScale = m_StringVisualBottom.localScale;
    bottomScale.y = bottomLength;
    m_StringVisualBottom.localScale = bottomScale;

    //Mesh aligned on the Y axis
    upVector = bottomDir.normalized;
    //This part is tricky...I need to use the original forward vector but clean it up so
    //it is 90 degrees from the new Up Vector
    forwardVector = ProjectVectorOnPlane(upVector, m_StringVisualBottom.forward).normalized;
    m_StringVisualBottom.rotation = Quaternion.LookRotation(forwardVector, upVector);

    m_StringArrowAttachPoint.position = handWorldPos;

    m_FireDirection = m_BowStringCollider.transform.position - m_StringArrowAttachPoint.position;
    m_AltFireDirection = m_DynamicStringAttach.transform.position - m_StringArrowAttachPoint.position;

    //Debug.DrawRay(handWorldPos, m_FireDirection, Color.blue);
    //Debug.DrawRay(handWorldPos, m_AltFireDirection, Color.cyan);
    LaunchDeviationAngle = 0;

    if (m_AttachedArrow != null)
    {
      m_AttachedArrow.transform.forward = (m_BowController.ArrowSocket.position - handWorldPos).normalized;
      LaunchDeviationAngle = Vector3.Angle(m_AttachedArrow.transform.forward, m_BowController.ArrowSocket.forward);
    }
  }

  public static Vector3 ProjectVectorOnPlane(Vector3 planeNormal, Vector3 vector)
  {
    return vector - (Vector3.Dot(vector, planeNormal) * planeNormal);
  }
}
