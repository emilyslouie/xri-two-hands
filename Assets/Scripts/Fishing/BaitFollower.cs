using System;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Fish))]
public class BaitFollower : MonoBehaviour
{
    private SphereCollider m_VisionCollider;
    public float ReactionChance = 1;
    public float DecisionCooldown = 5;
    
    public bool m_FollowingBait;
    public Bait m_Bait;
    public float m_nextDecision = 0;
    private Fish m_Fish;
    void Start()
    {
        m_VisionCollider = GetComponent<SphereCollider>();
        m_Fish = GetComponent<Fish>();
    }

    private void Update()
    {
        if (m_Fish.m_Caught) return;
        if (!m_Fish.Spawner) return;
        
        if (m_Bait && !m_FollowingBait && m_nextDecision > DecisionCooldown) {
            m_FollowingBait = ShouldFollow();
            m_nextDecision = 0;
        }
        
        if (m_FollowingBait)
        {
            FollowBait();
        }
        else
        {
            m_nextDecision += Time.deltaTime;
        }
    }

    private bool ShouldFollow()
    {
        return Random.Range(0, 1) <= ReactionChance;
    }

    void FollowBait()
    {
        if (!m_Bait || m_Bait.HasFish || !m_Bait.InWater) return;

        if (!m_FollowingBait && m_nextDecision < DecisionCooldown) return;
        
        m_Fish.MoveToTarget(m_Bait.transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != "Bait") return;
        m_Bait = other.GetComponent<Bait>();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Bait") m_Bait = null;
    }
}