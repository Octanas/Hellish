using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeChecker : MonoBehaviour
{
    private Transform playerTransform;
    public PlayerMovementClimb playerMovement;

    void Start()
    {
        playerTransform = playerMovement.transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Ledge")
        {
            playerMovement.HangOnLedge(other.transform.position, other.transform.rotation);

            // TODO: remove player friction with walls
        }
    }
}