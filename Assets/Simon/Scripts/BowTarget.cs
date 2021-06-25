using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BowTarget : MonoBehaviour
{
  [SerializeField]
  private float[] m_PointRanges = default;
  [SerializeField]
  private Material m_HighlightMat = default;

  private int m_LastHitPointRangeIndex;

  private MeshRenderer[] m_RingRenderers = null;

  private Material m_OrigMaterial;
  private SimpleTimer m_HighlightTimer = null;
  private void Awake()
  {
    m_RingRenderers = GetComponentsInChildren<MeshRenderer>();
    m_RingRenderers.Reverse();
    m_HighlightTimer = new SimpleTimer(1);
    m_HighlightTimer.ForceDone();
  }

  private void Update()
  {
    if (m_HighlightTimer.Done == false && m_HighlightTimer.Update(Time.deltaTime))
    {
      RestoreLastHitRing();
    }
  }
  private void OnCollisionEnter(Collision collision)
  {
    if (collision.contactCount > 0)
    {
      ContactPoint point = collision.GetContact(0);
      Vector3 localContact = transform.InverseTransformPoint(point.point);
      //Debug.Log($"Local Contact {localContact}");
      float distanceFromCenter = localContact.magnitude;

      for (int i = m_PointRanges.Length-1; i >= 0 ; --i)
      {
        if (distanceFromCenter < m_PointRanges[i])
        {
          //if previous was still highlighting, restore
          if (m_HighlightTimer.Done == false)
          {
            RestoreLastHitRing();
          }

          m_LastHitPointRangeIndex = i;
          if (i < m_RingRenderers.Length)
          {
            m_OrigMaterial = m_RingRenderers[m_LastHitPointRangeIndex].material;
            m_RingRenderers[m_LastHitPointRangeIndex].material = m_HighlightMat;
            m_HighlightTimer.Reset();
          }
          break;
        }
      }
    }
  }

  private void RestoreLastHitRing()
  {
    m_HighlightTimer.ForceDone();
    if(m_LastHitPointRangeIndex >= 0 && m_LastHitPointRangeIndex < m_RingRenderers.Length)
      m_RingRenderers[m_LastHitPointRangeIndex].material = m_OrigMaterial;
    m_OrigMaterial = null;
    m_LastHitPointRangeIndex = -1;
  }

  //private void OnDrawGizmosSelected()
  //{
  //  for (int i = 0; i < m_PointRanges.Length; i++)
  //  {
  //    Gizmos.DrawWireCube(transform.position, new Vector3(m_PointRanges[i] * 2, 0.2f, m_PointRanges[i] * 2));
  //  }
  //}
}
