using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DynamicPhysicsAttach : MonoBehaviour
{
  public bool IsAttached { get; private set; }

  private Vector3 m_RequestedPosition;

  public Vector3 RequestedPosition { get => m_RequestedPosition; }
  public Vector3 ClosestAvailablePosition { get => transform.position; }

  public Vector3 RequestedDirection { get => m_RequestedPosition - transform.position; }
  private void Awake()
  {
    IsAttached = false;
  }

  internal void Attach(Vector3 requestedPosition, Quaternion rotation, List<Collider> colliders = null)
  {
    //check if the new  requested position is different than before, otherwise don't do needles checks
    if ((requestedPosition - m_RequestedPosition).sqrMagnitude > 0.000001f)
    {
      //store request
      m_RequestedPosition = requestedPosition;

      Vector3 availablePosition = requestedPosition;
      if (colliders != null && colliders.Count > 0)
      {
        availablePosition = ClosestAttachPointOnColliders(requestedPosition, colliders);
      }

      transform.position = availablePosition;
      transform.rotation = rotation;
      IsAttached = true;
    }
  }

  internal void UpdateRequestPosition(Vector3 requestedPosition)
  {
    if ((requestedPosition - m_RequestedPosition).sqrMagnitude > 0.000001f)
    {
      //store request
      m_RequestedPosition = requestedPosition;
    }
  }

  internal void RecheckBestAvailablePosition(List<Collider> colliders = null)
  {
    if (colliders != null && colliders.Count > 0)
    {
      transform.position = ClosestAttachPointOnColliders(m_RequestedPosition, colliders);
    }
  }

  internal void Detach()
  {
    IsAttached = false;
  }

  private static Vector3 ClosestAttachPointOnColliders(Vector3 position, List<Collider> colliders)
  {
    var attachDistanceSq = 10000000.0f;
    var closestPosition = position;
    foreach (var collider in colliders)
    {
      var newPosition = collider.ClosestPoint(position);
      var distanceSq = (newPosition - position).sqrMagnitude;
      if (distanceSq < attachDistanceSq)
      {
        attachDistanceSq = distanceSq;
        closestPosition = newPosition;
      }
    }
    return closestPosition;
  }

  private void OnDrawGizmos()
  {
    if(IsAttached)
    {
      Gizmos.color = Color.cyan;
      Gizmos.DrawWireSphere(m_RequestedPosition, 0.1f);

      Gizmos.color = Color.green;
      Gizmos.DrawWireSphere(transform.position, 0.1f);
    }
  }
}
