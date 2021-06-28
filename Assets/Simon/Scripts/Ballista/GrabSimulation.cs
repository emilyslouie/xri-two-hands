using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GrabSimulation : MonoBehaviour
{
  [SerializeField]
  private Transform m_FirstGrabPoint, m_SecondGrabPoint = default;
  [SerializeField]
  private bool m_FirstGrabActive, m_SecondGrabActive = false;
  [SerializeField]
  private DynamicPhysicsAttach m_PhysicsAttachPoint = default;
  [SerializeField]
  private List<Collider> m_GrabAbleColliders = default;
  [SerializeField, Min(1f)]
  private float m_GrabStrength = 10;

  private Rigidbody m_Body = default;

  private Vector3 m_LastKnownGrabPoint;
  private int m_ActiveGrabPoints = 0;

  private void Awake()
  {
    m_Body = GetComponent<Rigidbody>();
  }

  private void Update()
  {
    CalculateGrabPoint();
  }

  private void FixedUpdate()
  {
    //If the attachpoint is active
    //Get the direction between where the grabpoint actually is and where we want it to be
    if (m_PhysicsAttachPoint.IsAttached)
    {
      Vector3 requestDirection = m_PhysicsAttachPoint.RequestedDirection;
      Debug.DrawRay(m_PhysicsAttachPoint.ClosestAvailablePosition, requestDirection, Color.green);

      //Use Acceleration so the mass of the Rigidbody doesn't matter
      //Normalize the direction so the only factor in the force is the grabstrength

      //TODO: Add something to get rid of "overshoot" due to forces being applied every frame without getting rid of the "snappyness"
      //BUG: Currently if the "desired grabpoint" gets close enough to the location where the PhysicsPoint is actually attached, jitter starts happening
      m_Body.AddForceAtPosition(requestDirection.normalized * m_GrabStrength, m_PhysicsAttachPoint.ClosestAvailablePosition, ForceMode.Acceleration);
    }
  }


  private void CalculateGrabPoint()
  {
    Vector3 grabPoint = Vector3.zero;
    int grabPoints = 0;

    //Check which grabpoints are active
    //These represent the controller positions having grabbed the item
    if (m_FirstGrabActive)
    {
      ++grabPoints;
      grabPoint += m_FirstGrabPoint.position;
    }

    if (m_SecondGrabActive)
    {
      ++grabPoints;
      grabPoint += m_SecondGrabPoint.position;
    }

    //if the amount of grabpoints changed then recalculate the attach point
    //For now the attachpoint is averaged equally
    if (m_ActiveGrabPoints != grabPoints && grabPoints > 0)
    {
      m_ActiveGrabPoints = grabPoints;
      grabPoint /= grabPoints;

      m_LastKnownGrabPoint = grabPoint;
      //Attach the physicsjoint to the closest position on the collider
      //Don't account for controller rotation right now
      m_PhysicsAttachPoint.Attach(m_LastKnownGrabPoint, Quaternion.identity, m_GrabAbleColliders);
    }
    else if (m_PhysicsAttachPoint.IsAttached && grabPoints == 0) //if attached, and no grabpoints, detach
    {
      m_ActiveGrabPoints = 0;
      m_PhysicsAttachPoint.Detach();
    }
    else if (m_PhysicsAttachPoint.IsAttached)//While attached, tell the physicsjoint where the grabpoint (avg controller positions) are
    {
      grabPoint /= grabPoints;
      m_LastKnownGrabPoint = grabPoint;
      m_PhysicsAttachPoint.UpdateRequestPosition(grabPoint);
    }
  }

  private void OnDrawGizmos()
  {
    if (m_PhysicsAttachPoint.IsAttached)
    {
      Gizmos.color = Color.red;
      Gizmos.DrawWireSphere(m_LastKnownGrabPoint, 0.1f);

      Gizmos.color = Color.yellow;
      Gizmos.DrawRay(m_Body.position, m_Body.velocity);
    }
  }
}
