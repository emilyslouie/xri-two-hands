using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour
{

    [SerializeField]
    List<GameObject> m_CloneAndActivateOnBreak = new List<GameObject>();
    
    [SerializeField]
    List<GameObject> m_DeactivateOnBreak = new List<GameObject>();
    
    [SerializeField]
    [Tooltip("The hover audio source.")]
    AudioSource m_AudioHit;

    [SerializeField]
    [Tooltip("The click audio source.")]
    AudioSource m_AudioBreak;

        
    [SerializeField]
    int m_MaxDamage = 20;
    
    int m_Damage = 0;
    const float k_ShineTime = 0.5f;
    bool m_HitCooldown = false;
    float m_ShineTimer = 0.0f;
    float m_LastHitSpeed;


    void OnCollisionEnter(Collision other)
    {
        float hitSpeed;
        if (other.collider.isTrigger == false && other.rigidbody != null)
        {
            hitSpeed = (GetComponent<Rigidbody>().velocity + -other.rigidbody.velocity).magnitude;
        }
        else
        {
            hitSpeed = GetComponent<Rigidbody>().velocity.magnitude;
        }

        if (hitSpeed > 1f)
        {
            if (!m_HitCooldown)
            {
                m_AudioHit.Play();
                
                m_Damage += (int)hitSpeed;
                m_LastHitSpeed = hitSpeed;
                //Debug.Log("Watermelon damage from " + other.collider.gameObject.name + "\n RigidBody: " + other.rigidbody.gameObject.name + "\n Damage: " + (int)hitSpeed);

                if (m_Damage > m_MaxDamage)
                {
                    m_AudioBreak.Play();
                    foreach (var o in m_DeactivateOnBreak)
                    {
                        o.SetActive(false);
                    }

                    foreach (var o in m_CloneAndActivateOnBreak)
                    {
                        var instance = Instantiate(o, o.transform.position, o.transform.rotation);
                        instance.SetActive(true);
                        Destroy(instance, 30f);
                    }
                    
                    StartCoroutine(Respawn());
                }
                
                
                m_HitCooldown = true;
            }
        }
    }

    IEnumerator Respawn(float delay = 8f)
    {
        yield return new WaitForSeconds(delay);
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;
        var rb = GetComponent<Rigidbody>();
        m_Damage = 0;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        foreach (var o in m_DeactivateOnBreak)
        {
            o.SetActive(true);
        }
    }

    void Awake()
    {
        m_Damage = 0;
    }

    void Update()
    {

        if (transform.position.y < -100f) // Respawn when it falls out of world
        {
            StartCoroutine(Respawn(0f));
        }
        // Do timer count up/count down
        if (m_HitCooldown)
        {
            m_ShineTimer += Time.deltaTime;

            var shinePercent = Mathf.Clamp01(m_ShineTimer / k_ShineTime);
            var shineValue = Mathf.PingPong(shinePercent, 0.5f) * 2.0f;

            foreach (var o in m_DeactivateOnBreak)
            {
                o.transform.localScale = Vector3.one * (1f - (m_LastHitSpeed / 50f * shineValue));
            }
            
            if (shinePercent >= 1.0f)
            {
                m_HitCooldown = false;
                m_ShineTimer = 0.0f;
                foreach (var o in m_DeactivateOnBreak)
                {
                    o.transform.localScale = Vector3.one;
                }

            }
        }
    }
}
