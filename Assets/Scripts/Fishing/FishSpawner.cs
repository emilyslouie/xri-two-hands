using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

[RequireComponent(typeof(BoxCollider))]
public class FishSpawner : MonoBehaviour
{
    public int MaxFish = 5;
    public GameObject FishPrefab;
    public float SpawnDelay = 2.0f;

    private List<Fish> m_Fish = new List<Fish>();
    
    private Collider m_Collider;
    private float cooldown;

    void Start()
    {
        m_Collider = GetComponent<Collider>();
    }

    private void Update()
    {
        if (m_Fish.Count < MaxFish && cooldown > SpawnDelay)
        {
            SpawnFish();
            cooldown = 0;
        }

        cooldown += Time.deltaTime;
    }

    public Vector3 RandomPointInVolume()
    {
        var x = Random.Range(-m_Collider.bounds.extents.x, m_Collider.bounds.extents.x);
        var z = Random.Range(-m_Collider.bounds.extents.z, m_Collider.bounds.extents.z);
        var y = Random.Range(-m_Collider.bounds.extents.y, m_Collider.bounds.extents.y);
        
        return new Vector3(transform.position.x + x, transform.position.y + y, transform.position.z + z);
    }
    
    void SpawnFish()
    {
        var pos = RandomPointInVolume();
        
        var fish = Fish.Create(FishPrefab,pos, this);
        m_Fish.Add(fish);
    }

    public void FishCaught(Fish fish)
    {
        var found = m_Fish.Find(f => f == fish);
        
        if (found != null) m_Fish.Remove(found);
    }

    public void FishReceived(Fish fish)
    {
        if (m_Fish.Find(f => f == fish)) return;

        m_Fish.Add(fish);
    }
}
