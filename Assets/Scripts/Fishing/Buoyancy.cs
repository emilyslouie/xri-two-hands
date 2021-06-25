using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(BoxCollider))]
public class Buoyancy : MonoBehaviour
{
    public float UpSpeed = 1;
    private List<GameObject> objects = new List<GameObject>();
    private Collider m_Collider;

    private void Start()
    {
        m_Collider = GetComponent<Collider>();
    }

    private void Update()
    {
        foreach(var gameObject in objects)
        {
            FloatObject(gameObject);
        }
    }

    void FloatObject(GameObject gameObject)
    {
        var t = gameObject.transform;
        if (t.position.y >= m_Collider.bounds.extents.y) return;
        t.Translate(Vector3.up * (UpSpeed * Time.deltaTime));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<FloatingObject>()) objects.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        var match = objects.Find(o => o == other.gameObject);
        if (match) objects.Remove(match);
    }
}