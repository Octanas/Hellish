using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeCheckerJump : MonoBehaviour
{
    private Transform playerTransform;
    public PlayerMovementJump playerMovement;

    void Start()
    {
        playerTransform = playerMovement.transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playerMovement.GetState() == PlayerMovementJump.State.Moving && other.name == "Ledge")
        {
            Quaternion oldRotation = playerTransform.rotation;
            playerTransform.rotation = other.transform.rotation;

            Vector3 diffPosition = other.transform.position - transform.GetChild(0).position;
            diffPosition.x = 0;

            playerTransform.rotation = oldRotation;

            // Move character a total of diffPosition
            playerMovement.ApplyTranslation(diffPosition);
            playerMovement.SetAngle(other.transform.eulerAngles.y);

            playerMovement.ChangeState(PlayerMovementJump.State.Moving);

            // TODO: remove player friction with walls
        }
    }
}