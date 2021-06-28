using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit; 

public class BallLauncher : MonoBehaviour
{
    XRGrabInteractable grabbyPart;

    private void Start()
    {
        grabbyPart = GetComponent<XRGrabInteractable>(); 
    }

    public void DisableGrab()
    {
        grabbyPart.enabled = false;
    }

    public void EnableGrab()
    {
        grabbyPart.enabled = true; 
    }
}
