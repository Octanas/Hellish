using UnityEngine;

/// <summary>
/// Handles detection of ledges for a character to latch on to.
/// The character needs to have a single trigger collider,
/// in the position where his hands will be when grabbing the ledge.
/// This script should be added to the object containing that collider.
/// </summary>
public class LedgeChecker : MonoBehaviour
{
    private Transform playerTransform;
    /// <summary>
    /// Player movement script.
    /// </summary>
    [Tooltip("Player movement script.")]
    public PlayerMovement playerMovement;

    void Start()
    {
        playerTransform = playerMovement.transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Ledge")
        {
            playerMovement.HangOnLedge(other.transform.position, other.transform.rotation);
        }
    }
}