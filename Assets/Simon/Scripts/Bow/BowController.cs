using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class BowController : MonoBehaviour
{
  [SerializeField]
  Transform m_StaticVisual;
  [SerializeField]
  Transform m_DrawVisualTop;
  [SerializeField]
  Transform m_DrawVisualBottom;
  [SerializeField]
  Transform m_ArrowPivot;
  [SerializeField]
  XRSocketInteractor m_ArrowSocket;
  [SerializeField]
  Transform m_DrawVisualAttach;
  [SerializeField]
  private float m_MaxDrawback = 0.5f;
  [SerializeField]
  private float m_MaxPower = 10;
  [SerializeField]
  Transform m_ArrowPrefab;

  [Header("UI")]
  [SerializeField]
  GameObject m_InfoCanvas;
  [SerializeField]
  Image m_DrawbackPowerImg;
  [SerializeField]
  Text m_DrawbackPowerText;

  private float m_CurrentDrawback;
  private Transform m_InstancedArrow;

  private void Awake()
  {
    if (m_InfoCanvas != null)
    {
      m_InfoCanvas.SetActive(false);
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="handWorldPos">Position of interactor in world space</param>
  /// <param name="constrainedHandWorldPos">Position of attachpoint constrained to collider</param>
  public void UpdateBowstringVisual(Vector3 handWorldPos, Vector3 constrainedHandWorldPos)
  {
    Vector3 arrowDir = (m_ArrowPivot.position - handWorldPos);

    //account for the amount of space between the arrowpivot and the bowstring in "non drawback" position
    float drawBackOffset = (constrainedHandWorldPos - m_ArrowPivot.position).magnitude;

    float drawBack = arrowDir.magnitude - drawBackOffset;
    if (drawBack > m_MaxDrawback)
    {
      drawBack = m_MaxDrawback;
      handWorldPos = m_ArrowPivot.position + (-arrowDir.normalized * (drawBack + drawBackOffset));
    }

    if (drawBack != m_CurrentDrawback)
    {
      m_CurrentDrawback = drawBack;
      if (m_InfoCanvas.activeSelf && m_DrawbackPowerImg != null)
      {
        float perc = Mathf.Clamp01(m_CurrentDrawback / m_MaxDrawback);
        m_DrawbackPowerImg.fillAmount = perc;
        m_DrawbackPowerText.text = string.Format("{0}%", Mathf.CeilToInt(perc * 100));
      }
    }

    DebugMenu.Instance.SetInfoText($"Hand WorldPos {handWorldPos}", 0);

    Vector3 anchorTop = m_DrawVisualTop.position;
    Vector3 anchorBottom = m_DrawVisualBottom.position;

    Vector3 topDir = (handWorldPos - anchorTop);
    float topLength = topDir.magnitude;
    DebugMenu.Instance.SetInfoText($"Top Pos {anchorTop} - Length {topLength}", 1);

    //Debug.DrawRay(anchorTop, topDir, Color.red);

    Vector3 bottomDir = (handWorldPos - anchorBottom);
    float bottomLength = bottomDir.magnitude;
    DebugMenu.Instance.SetInfoText($"Bot Pos {anchorBottom} - Length {bottomLength}", 2);

    //Debug.DrawRay(anchorBottom, bottomDir, Color.red);

    Vector3 topScale = m_DrawVisualTop.localScale;
    topScale.y = topLength;
    m_DrawVisualTop.localScale = topScale;
    //Vector3 forward = Quaternion.AngleAxis(90, Vector3.right) * topDir;
    //m_DrawVisualTop.rotation = Quaternion.LookRotation(forward.normalized, -topDir.normalized);

    //Mesh aligned on the Y axis
    Vector3 upVector = -topDir.normalized;
    //This part is tricky...I need to use the original forward vector but clean it up so
    //it is 90 degrees from the new Up Vector
    Vector3 forwardVector = ProjectVectorOnPlane(upVector, m_DrawVisualTop.forward).normalized;
    m_DrawVisualTop.rotation = Quaternion.LookRotation(forwardVector, upVector);


    Vector3 bottomScale = m_DrawVisualBottom.localScale;
    bottomScale.y = bottomLength;
    m_DrawVisualBottom.localScale = bottomScale;

    //Mesh aligned on the Y axis
    upVector = bottomDir.normalized;
    //This part is tricky...I need to use the original forward vector but clean it up so
    //it is 90 degrees from the new Up Vector
    forwardVector = ProjectVectorOnPlane(upVector, m_DrawVisualBottom.forward).normalized;
    m_DrawVisualBottom.rotation = Quaternion.LookRotation(forwardVector, upVector);

    m_DrawVisualAttach.position = handWorldPos;
    if (m_InstancedArrow != null && arrowDir != Vector3.zero)
    {
      m_InstancedArrow.rotation = Quaternion.LookRotation(arrowDir.normalized, m_InstancedArrow.up);
    }
  }
  public static Vector3 ProjectVectorOnPlane(Vector3 planeNormal, Vector3 vector)
  {
    return vector - (Vector3.Dot(vector, planeNormal) * planeNormal);
  }

  public void BowStringGripped()
  {
    if (m_InfoCanvas != null)
    {
      m_InfoCanvas.SetActive(true);
      m_DrawbackPowerImg.fillAmount = 0;
      m_DrawbackPowerText.text = "0%";
    }

    m_StaticVisual.gameObject.SetActive(false);
    m_DrawVisualTop.gameObject.SetActive(true);
    m_DrawVisualBottom.gameObject.SetActive(true);
    CreateNockedArrow();
  }

  public void BowStringReleased()
  {
    if (m_InfoCanvas != null)
      m_InfoCanvas.SetActive(false);


    m_StaticVisual.gameObject.SetActive(true);
    m_DrawVisualTop.gameObject.SetActive(false);
    m_DrawVisualBottom.gameObject.SetActive(false);
    LaunchNockedArrow();
  }

  public void CreateNockedArrow()
  {
    if (m_ArrowPrefab != null && m_InstancedArrow == null)
    {
      m_InstancedArrow = Instantiate(m_ArrowPrefab, m_DrawVisualAttach);
      m_InstancedArrow.transform.localPosition = Vector3.zero;
    }
  }

  public void DestroyNockedArrow()
  {
    if (m_InstancedArrow != null)
    {
      Destroy(m_InstancedArrow.gameObject);
      m_InstancedArrow = null;
    }
  }

  public void LaunchNockedArrow()
  {
    if (m_InstancedArrow != null)
    {
      m_InstancedArrow.SetParent(null);
      Arrow arrow = m_InstancedArrow.GetComponent<Arrow>();
      float power = m_MaxPower * (m_CurrentDrawback / m_MaxDrawback);

      if (m_CurrentDrawback <= 0)
        power = 0.1f;

      arrow.Fire(power);
      m_InstancedArrow = null;
    }
  }

}
