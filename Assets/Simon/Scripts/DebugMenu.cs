using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class DebugMenu : MonoBehaviour
{
  public static DebugMenu Instance
  {
    get { return s_Instance; }
  }
  private static DebugMenu s_Instance = null;

  [SerializeField]
  private ActionBasedController m_StickController = default;
  [SerializeField]
  private Transform m_CharacterController = default;
  [SerializeField]
  private Vector3 m_OpeningOffset = Vector3.zero;
  [SerializeField]
  private Transform m_ActionFeedback = default;
  [SerializeField]
  private List<Text> m_InfoText;

  private XRRayInteractor m_RayInteractor;
  private Vector3 m_SpawnLocation;

  private SimpleTimer m_FeedbackTimer = default;

  InputAction m_MenuAction;

    private void Awake()
  {
    if (s_Instance != null && s_Instance != this)
    {
      Destroy(this);
      return;
    }
    s_Instance = this;

    Debug.Assert(m_StickController != null, "No Controller assigned, Debug Menu wont work!", this);
    m_RayInteractor = m_StickController.GetComponent<XRRayInteractor>();

    m_FeedbackTimer = new SimpleTimer(1);
    m_FeedbackTimer.ForceDone();

    for (int i = 0; i < m_InfoText.Count; i++)
    {
      m_InfoText[i].gameObject.SetActive(false);
    }
    m_MenuAction = new InputAction("menuAction");
    m_MenuAction.Disable();
    m_MenuAction.AddBinding("<XRController>{LeftHand}/menu");
    m_MenuAction.AddBinding("<XRController>{RightHand}/menu");
    m_MenuAction.Enable();
    UpdateSpawnPoint();
    Hide();
  }

  private void ToggleVisibility(InputAction.CallbackContext obj)
  {
    if (gameObject.activeSelf == false)
    {
      Show();
    }
  }

  private void Update()
  {
    if(m_FeedbackTimer.Done == false)
    {
      if(m_FeedbackTimer.Update(Time.deltaTime))
      {
        m_ActionFeedback.gameObject.SetActive(false);
      }
    }
  }

  /// <summary>
  /// Call this on functions that do something without closing the menu
  /// </summary>
  private void DoActionFeedback()
  {
    m_ActionFeedback.gameObject.SetActive(true);
    m_FeedbackTimer.Reset();
  }

  /// <summary>
  /// Open debug menu and register to closing events
  /// Menu opens at the opening controller and has an configurable offset applied to it
  /// Enables the rayinteractor to allow for UI interactions
  /// </summary>
  public void Show()
  {
    m_MenuAction.performed -= ToggleVisibility;
    m_MenuAction.performed += OnUIPressActionWhileOpen;

    Vector3 worldPos = m_StickController.transform.position;
    Quaternion rotation = m_StickController.transform.rotation;
    Vector3 euler = rotation.eulerAngles;
    euler.z = 0;

    Vector3 offset = m_StickController.transform.forward * m_OpeningOffset.z + m_StickController.transform.up * m_OpeningOffset.y + m_StickController.transform.right * m_OpeningOffset.x;  
    this.transform.position = worldPos + offset;
    this.transform.rotation = Quaternion.Euler(euler);

    m_RayInteractor.enabled = true;

    gameObject.SetActive(true);
  }

  /// <summary>
  /// Check if anything was hit during the action press, if not close the menu
  /// </summary>
  /// <param name="obj"></param>
  private void OnUIPressActionWhileOpen(InputAction.CallbackContext obj)
  {
    //if (m_RayInteractor.TryGetCurrentUIRaycastResult(out UnityEngine.EventSystems.RaycastResult hitResult) == false)
      Hide();
  }

  /// <summary>
  /// Hide menu and register for needed events
  /// </summary>
  public void Hide()
  {
    m_RayInteractor.enabled = false;
    m_MenuAction.performed -= OnUIPressActionWhileOpen;
    m_MenuAction.performed += ToggleVisibility;
    gameObject.SetActive(false);
  }

  /// <summary>
  /// Respawn character to saved locations
  /// Hide Menu afterwards
  /// </summary>
  public void OnRespawn()
  {
    m_CharacterController.position = m_SpawnLocation;
    Hide();
  }

  /// <summary>
  /// Update character spawn location to current location
  /// </summary>
  public void UpdateSpawnPoint()
  {
    m_SpawnLocation = m_CharacterController.position;
    DoActionFeedback();
  }

  /// <summary>
  /// Quit the game
  /// </summary>
  public void QuitGame()
  {
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
  }

  public void SetInfoText(string text, int infoIndex)
  {
    if (infoIndex < 0 || infoIndex >= m_InfoText.Count)
      return;

    m_InfoText[infoIndex].gameObject.SetActive(true);
    m_InfoText[infoIndex].text = text;
  }

  public void DisableInfoText(int infoIndex)
  {
    if (infoIndex < 0 || infoIndex >= m_InfoText.Count)
      return;

    m_InfoText[infoIndex].gameObject.SetActive(false);
  }

}
