using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRLauncherGrabInteractable : XRGrabInteractable
{
    Transform slingshotParent; 
    [SerializeField] Transform handlePoint;
    [SerializeField] XRSocketInteractor ballSocket;

    Vector3 startingPosition; 

    private void Start()
    {
        startingPosition = this.transform.localPosition;
        slingshotParent = this.transform.parent;
    }

    protected override void Drop()
    {
        base.Drop();

        //trajectory.HideLine();

        if(ballSocket.selectTarget != null)
        {
            GameObject proj = ballSocket.selectTarget.gameObject; 
            //need to detach it from the socket
            ((XRMultiInteractionManager)interactionManager).ForceDeselect(ballSocket);
    
            float velocityModifier = Vector3.Distance(handlePoint.position, this.transform.position);
            proj.GetComponent<Rigidbody>().AddForce(proj.transform.forward * 1000 * velocityModifier, ForceMode.Force);
        }

        this.transform.SetParent(slingshotParent);
        this.transform.localPosition = startingPosition;
        this.transform.localRotation = Quaternion.Euler(Vector3.zero); 
    }
}
