using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
public class Arrow : MonoBehaviour
{
  [SerializeField]
  private Transform m_CenterOfMass;
  [SerializeField]
  private Collider m_TipCollider;
  [SerializeField]
  private Collider m_ShaftCollider;
  [SerializeField]
  private Collider m_FletchingCollider;
  [SerializeField]
  private LayerMask m_IgnoreCollisionMask = 0;

  public float GrabDistanceToBottom { get; private set; }

  private const float LOWER_Y_DESPAWN = -10;
  private Rigidbody m_Body = null;

  private XRGrabInteractable m_GrabInteractable;

  public XRGrabInteractable InteractAble
  {
    get { return m_GrabInteractable; }
  }

  private Vector3 m_TravelDirection;

  private int m_OrigShaftLayer = 0;
  private int m_OrigFletchingLayer = 0;
  private bool m_bFired = false;

  private SimpleTimer m_FletchingColliderTimeout = null;

  private void Awake()
  {
    m_FletchingColliderTimeout = new SimpleTimer(0.5f);
    m_FletchingColliderTimeout.ForceDone();
    m_Body = GetComponent<Rigidbody>();
    m_Body.centerOfMass = m_CenterOfMass.localPosition;
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
    SetDynamic();

    m_bFired = false;

    m_FletchingCollider.enabled = false;
    m_FletchingCollider.gameObject.layer = m_OrigFletchingLayer;

    m_ShaftCollider.gameObject.layer = m_OrigShaftLayer;

    GrabDistanceToBottom = float.MaxValue;
  }

  private void ArrowPickedUp(SelectEnterEventArgs arg0)
  {
    SetKinematic();
    //When picked up, set parent to be the interactor
    this.transform.SetParent(arg0.interactor.transform);

    //enable the fletching collider/trigger
    //This is needed so we can check if we're being attached to the bowstring
    m_FletchingColliderTimeout.Reset();

    int defaultLayer = LayerMask.NameToLayer("Default");
    //set this collider to be default
    m_OrigFletchingLayer = m_FletchingCollider.gameObject.layer;
    m_FletchingCollider.gameObject.layer = defaultLayer;

    m_OrigShaftLayer = m_ShaftCollider.gameObject.layer;
    m_ShaftCollider.gameObject.layer = defaultLayer;


    GrabDistanceToBottom = (arg0.interactor.transform.position - transform.position).magnitude;
    //Debug.Log($"Arrow GrabDistance {GrabDistanceToBottom}");
  }

  private void Update()
  {
    if (transform.position.y < LOWER_Y_DESPAWN)
      Destroy(this.gameObject);

    if (m_FletchingColliderTimeout.Done == false && m_FletchingColliderTimeout.Update(Time.deltaTime))
    {
      m_FletchingCollider.enabled = true;
    }
  }

  private void FixedUpdate()
  {
    //if there is some velocity acting on this arrow after being fired
    //point it in the direction of this velocity
    if (m_bFired && m_Body.velocity != Vector3.zero)
    {
      m_TravelDirection = m_Body.velocity;
      transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(m_TravelDirection.normalized), Time.deltaTime * 10);
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
    if (isTipHit && (m_IgnoreCollisionMask & (1 << collision.gameObject.layer)) == 0)
    {
      //update our parent
      transform.SetParent(collision.transform);

      //Back to discrete collision and become kinematic
      SetKinematic();

      if (m_bFired)
      {
        ContactPoint contact = collision.GetContact(0);
        Vector3 contactPoint = contact.point;
        transform.position = contactPoint + (-transform.forward * 0.6f);
      }

      if (m_GrabInteractable != null)
      {
        m_GrabInteractable.enabled = true;
      }

      m_bFired = false;
    }
  }

  public void SetKinematic()
  {
    m_Body.collisionDetectionMode = CollisionDetectionMode.Discrete;
    m_Body.isKinematic = true;
    m_Body.useGravity = false;
    m_Body.velocity = Vector3.zero; //no more velocity for you!
  }

  public void SetDynamic()
  {
    //Arrow is let go, enable gravity, disable kinematic and enable the script to allow for rotation update and falling check
    //unparent so no weird position/rotation stuff happens
    this.transform.SetParent(null);

    m_Body.useGravity = true;
    m_Body.isKinematic = false;
    //ensure smooth collision detection
    m_Body.collisionDetectionMode = CollisionDetectionMode.Continuous;
  }

  /// <summary>
  /// Fire arrow with a certain power behind it
  /// </summary>
  /// <param name="power"></param>
  public void Fire(float power, Vector3 forceDirection, bool addTorque)
  {
    SetDynamic();

    m_FletchingCollider.enabled = false;

    if(addTorque == false)
    {
      //add an impulse force
      m_Body.AddForce(forceDirection * power, ForceMode.Impulse);
      m_bFired = true;
    }
    else
    {
      //add an impulse force
      m_Body.AddForceAtPosition(forceDirection * power, transform.position + (transform.forward * 0.1f), ForceMode.Impulse);
    }
  }

  public void ToggleFletchingCollider(bool toggle)
  {
    m_FletchingColliderTimeout.ForceDone();
    m_FletchingCollider.enabled = toggle;
  }
}
