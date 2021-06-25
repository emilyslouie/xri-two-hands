using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.Switch;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
public class Fish : MonoBehaviour
{
    public float MoveSpeed = 1;
    public float RotationSpeed = 1;
    internal FishSpawner Spawner;
    private FixedJoint m_Joint;
    private GameObject m_LastBait;

    internal bool m_Caught;

    private Vector3 m_NextWaypoint;
    private Rigidbody m_Rigidbody;

    private void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Rigidbody.useGravity = false;
        SetWaypoint();
    }

    public void PickedUp(SelectEnterEventArgs eventArgs)
    {
        Debug.Log("Picking up fish");
        Release();
    }

    private void Update()
    {
        if (m_Caught || m_LastBait) return;
        
        RandomSwim();
        if (ArrivedAtWaypoint()) SetWaypoint();
    }

    bool ArrivedAtWaypoint()
    {
        var dist = (m_NextWaypoint - transform.position).magnitude;
        return dist < 0.1;
    }

    void RandomSwim()
    {
        MoveToTarget(m_NextWaypoint);
    }

    public void MoveToTarget(Vector3 target)
    {
        var targetDir = (target - transform.position).normalized;
        var rotationSpeed = RotationSpeed * Time.deltaTime;
        var newDir = Quaternion.LookRotation(transform.up, targetDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, newDir, rotationSpeed);

        transform.Translate(Vector3.up * (MoveSpeed * Time.deltaTime));
    }

    void SetWaypoint()
    {
        m_NextWaypoint = Spawner.RandomPointInVolume();
    }

    public void OnCollisionEnter(Collision other)
    {
        switch (other.gameObject.tag)
        {
            case "Bait":
                if (other.gameObject == m_LastBait) return;
                Bite(other.gameObject);
                break;
            case "FishSpawner":
                if (m_Caught) return;
                Spawner.FishReceived(this);
                m_Rigidbody.useGravity = false;
                break;
            default:
                return;    
        }
    }

    void Bite(GameObject other)
    {
        m_LastBait = other;
        m_LastBait.GetComponent<Bait>().Fish = this;
        Spawner.FishCaught(this);
        m_Joint = gameObject.AddComponent<FixedJoint>();
        m_Joint.connectedBody = other.GetComponent<Rigidbody>();
        m_Caught = true;
        m_Rigidbody.useGravity = true;
    }

    public void Release()
    {
        Destroy(m_Joint);
        m_Caught = false;
        m_LastBait.GetComponent<Bait>().Clear();
        m_LastBait = null;
    }

    public static Fish Create(GameObject prefab, Vector3 pos, FishSpawner spawner)
    {
        var fish = Instantiate(prefab, pos, prefab.transform.rotation);

        var component = fish.GetComponent<Fish>();
        component.Spawner = spawner;

        return component;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(m_NextWaypoint, 0.05f);
    }
}