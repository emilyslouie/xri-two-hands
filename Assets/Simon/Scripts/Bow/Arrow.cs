using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
public class Arrow : MonoBehaviour
{
  [SerializeField]
  private Collider m_TipCollider;
  [SerializeField]
  private Collider m_ShaftCollider;
  [SerializeField]
  private Collider m_FletchingCollider;
  [SerializeField]
  private LayerMask m_IgnoreCollisionMask = 0;

  private const float LOWER_Y_DESPAWN = -10;
  private Rigidbody m_Body = null;

  private XRGrabInteractable m_GrabInteractable;
  private Vector3 m_TravelDirection;

  private bool m_bFired = false;
  private void Awake()
  {
    m_Body = GetComponent<Rigidbody>();
    m_GrabInteractable = GetComponent<XRGrabInteractable>();
    if (m_GrabInteractable != null)
    {
      m_GrabInteractable.enabled = false;
      m_GrabInteractable.selectEntered.AddListener(ArrowPickedUp);
      m_GrabInteractable.selectExited.AddListener(ArrowDropped);
    }
  }

  private void ArrowDropped(SelectExitEventArgs arg0)
  {
    //Arrow is let go, enable gravity, disable kinematic and enable the script to allow for rotation update and falling check
    //unparent so no weird position/rotation stuff happens
    this.transform.SetParent(null);

    m_Body.useGravity = true;
    m_Body.isKinematic = false;
    m_Body.collisionDetectionMode = CollisionDetectionMode.Continuous;
    m_bFired = false;
    enabled = true;
  }

  private void ArrowPickedUp(SelectEnterEventArgs arg0)
  {
    //When picked up, set parent to be the interactor
    this.transform.SetParent(arg0.interactor.transform);
    //enable the fletching collider/trigger
    //This is needed so we can check if we're being attached to the bowstring
    m_FletchingCollider.enabled = true;
  }

  private void Update()
  {
    if (transform.position.y < LOWER_Y_DESPAWN)
      Destroy(this.gameObject);
  }

  private void FixedUpdate()
  {
    //if there is some velocity acting on this arrow after being fired
    //point it in the direction of this velocity
    if (m_bFired && m_Body.velocity != Vector3.zero)
    {
      m_TravelDirection = m_Body.velocity;
      transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(m_TravelDirection.normalized), Time.deltaTime * 2);
    }
  }

  private void OnCollisionEnter(Collision collision)
  {
    //check if arrowpoint hit
    int contactCount = collision.contactCount;
    bool isTipHit = false;
    for (int i = 0; i < contactCount; i++)
    {
      if (collision.GetContact(i).thisCollider == m_TipCollider)
      {
        isTipHit = true;
        break;
      }
    }

    //if the tip hits something and we're not ignoring this object to be able to stick into
    if (isTipHit &&(m_IgnoreCollisionMask & (1 << collision.gameObject.layer)) == 0)
    {
      //update our parent
      transform.SetParent(collision.transform);

      //Back to discrete collision and become kinematic
      m_Body.collisionDetectionMode = CollisionDetectionMode.Discrete;
      m_Body.isKinematic = true;
      m_Body.useGravity = false;
      m_Body.velocity = Vector3.zero; //no more velocity for you!
      enabled = false;

      if (m_GrabInteractable != null)
      {
        m_GrabInteractable.enabled = true;
      }
    }

    m_bFired = false;
  }

  /// <summary>
  /// Fire arrow with a certain power behind it
  /// </summary>
  /// <param name="power"></param>
  public void Fire(float power)
  {
    //make sure script is enabled
    enabled = true;

    m_Body.useGravity = true;
    m_Body.isKinematic = false;
    //ensure smooth collision detection
    m_Body.collisionDetectionMode = CollisionDetectionMode.Continuous;

    //add an impulse force
    m_Body.AddForce(transform.forward * power, ForceMode.Impulse);
    m_bFired = true;
  }
}
