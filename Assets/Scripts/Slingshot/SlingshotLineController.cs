using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SlingshotLineController : MonoBehaviour
{
    LineRenderer line;
    [SerializeField] Transform startPos;
    [SerializeField] Transform endPos; 
    // Start is called before the first frame update
    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.SetPosition(0, startPos.position);
        line.SetPosition(1, endPos.position); 
    }

    // Update is called once per frame
    void Update()
    {
        line.SetPosition(0, startPos.position);
        line.SetPosition(1, endPos.position);
    }
}
