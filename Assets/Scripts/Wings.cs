using System;
using UnityEngine;

public class Wings : MonoBehaviour
{
    //left  y 0 70 | x 180 160,
    //right y 0 -70 | x 0  20 
    public Transform leftWing, rightWing, baseLeftWing, baseRightWing;
    private const int min = 0, max = 90;
    private float direction = 1f;
    private int increment, totalTicks = 15;

    void Start()
    {
        increment = (max - min) / totalTicks;
    }

    // Update is called once per frame
    void Update()
    {
        var rotationLeft = leftWing.localEulerAngles;
        var rotationRight = rightWing.localEulerAngles;
        var rotationBaseLeft = baseLeftWing.localEulerAngles;
        var rotationBaseRight = baseRightWing.localEulerAngles;
        if (Math.Abs(rotationLeft.y - max) <= 1)
        {
            direction = -1;
        }
        else if (Math.Abs(rotationLeft.y - min) <= 1)
            direction = 1;

        leftWing.localEulerAngles = new Vector3(rotationLeft.x, rotationLeft.y + direction * increment, rotationLeft.z);
        rightWing.localEulerAngles = new Vector3(rotationRight.x, rotationRight.y - direction * increment, rotationRight.z);
        baseLeftWing.localEulerAngles = new Vector3((float) (rotationBaseLeft.x - direction * 20.0 / totalTicks),
            rotationBaseLeft.y, rotationBaseLeft.z);
        baseRightWing.localEulerAngles = new Vector3((float) (rotationBaseRight.x + direction * 20.0 / totalTicks),
            rotationBaseRight.y, rotationBaseRight.z);
    }
}