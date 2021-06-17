using System;
using UnityEngine;

public class Wings : MonoBehaviour
{
    //left  y 0 90 | x 180 160,
    //right y 0 -90 | x 0  20 
    public Transform leftWing, rightWing, baseLeftWing, baseRightWing;
    private const int min = 0, max = 90;
    private const int minBase = 0, maxBase = 20;
    private float direction = 1f;
    private float increment, incrementBase;
    private float totalSeconds = 0.3f;

    void Start()
    {
        increment = (max - min) / totalSeconds * Time.fixedDeltaTime;
        incrementBase = (maxBase - minBase) / totalSeconds * Time.fixedDeltaTime;
    }

    void FixedUpdate()
    {
        var rotationLeft = leftWing.localEulerAngles;
        var rotationRight = rightWing.localEulerAngles;
        var rotationBaseLeft = baseLeftWing.localEulerAngles;
        var rotationBaseRight = baseRightWing.localEulerAngles;

        if (Math.Abs(rotationLeft.y - max) <= 1)
        {
            FMODUnity.RuntimeManager.PlayOneShotAttached("event:/Player/Jump/wing_flap", gameObject);
            direction = -1;
        }
        else if (Math.Abs(rotationLeft.y - min) <= 1)
            direction = 1;

        float wingIncrement = direction * increment;
        float baseWingIncrement = direction * incrementBase;

        leftWing.localEulerAngles = new Vector3(rotationLeft.x, rotationLeft.y + wingIncrement, rotationLeft.z);
        rightWing.localEulerAngles = new Vector3(rotationRight.x, rotationRight.y - wingIncrement, rotationRight.z);
        baseLeftWing.localEulerAngles = new Vector3(rotationBaseLeft.x - baseWingIncrement, rotationBaseLeft.y, rotationBaseLeft.z);
        baseRightWing.localEulerAngles = new Vector3(rotationBaseRight.x + baseWingIncrement, rotationBaseRight.y, rotationBaseRight.z);
    }
}