using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawTrajectory : MonoBehaviour
{
    LineRenderer line; //this is the trajectory

    List<Vector3> linePoints = new List<Vector3>();
    Vector3 lastPosition; 

    private void Start()
    {
        line = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (lastPosition != this.transform.position)
        {
            lastPosition = this.transform.position;
            linePoints.Add(this.transform.position);
            line.positionCount = linePoints.Count;
            line.SetPositions(linePoints.ToArray()); 
        }

        if(line.positionCount == 0 && linePoints.Count != 0)
        {
            linePoints.Clear();
        }
    }
}
