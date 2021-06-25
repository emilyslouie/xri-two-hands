using System;
using UnityEngine;

public class Bait : MonoBehaviour
{
    internal Fish Fish;

    internal bool HasFish => Fish != null;
    internal bool InWater;
    

    public void Clear()
    {
        Fish = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "FishSpawner") InWater = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "FishSpawner") InWater = false;
    }
}