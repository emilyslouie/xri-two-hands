using Unity.XRContent.Interaction;
using UnityEngine;

public class ExtensibleString : MonoBehaviour
{
    public ConfigurableJoint Tip;
    public XRKnob Knob;
    public float StringLength = 1.0f;

    private void Update()
    {
        var limit = new SoftJointLimit
        {
            limit = Knob.Value * StringLength
        };
        
        Tip.linearLimit = limit;
    }
}
