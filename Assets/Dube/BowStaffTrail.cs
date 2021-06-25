using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowStaffTrail : MonoBehaviour
{
  public AnimationCurve scaleCurve;
  public float scaleMult = 1.0f;
  public float dampenTime = 0.01f;

  private Vector3 lastFrame;
  private bool skip = true;
  private Vector3 currentVelocity;

  private void FixedUpdate()
  {
    if (skip)
    {
      lastFrame = transform.position;
      skip = false;
      transform.localScale = new Vector3(1, 1, 0);
      return;
    }

    currentVelocity = Vector3.SmoothDamp(currentVelocity, transform.position - lastFrame, ref currentVelocity, dampenTime);

    var forward = Vector3.Cross(transform.up, Vector3.Cross(transform.up, currentVelocity.normalized));
    transform.localScale = new Vector3(1, 1, scaleCurve.Evaluate(Mathf.Clamp(Vector3.Dot(currentVelocity, -forward) * scaleMult, 0.0f, 1.0f)));

    if (forward != Vector3.zero)
    {
      transform.rotation = Quaternion.LookRotation(forward, transform.up);
    }
    lastFrame = transform.position;
  }
}
