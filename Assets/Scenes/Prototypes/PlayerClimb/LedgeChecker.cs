using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeChecker : MonoBehaviour
{
    private Transform playerTransform;
    public PlayerMovementClimb playerMovement;

    // Start is called before the first frame update
    void Start()
    {
        playerTransform = playerMovement.transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Ledge")
        {
            Quaternion oldRotation = playerTransform.rotation;
            playerTransform.rotation = other.transform.rotation;

            Vector3 diffPosition = other.transform.position - transform.GetChild(0).position;
            diffPosition.x = 0;

            playerTransform.rotation = oldRotation;

            // Move character a total of diffPosition
            playerMovement.ApplyTranslation(diffPosition);
            playerMovement.SetAngle(other.transform.eulerAngles.y);

            playerMovement.ChangeState(PlayerMovementClimb.State.Hanging);

            // TODO: remove player friction with walls
        }
    }
}