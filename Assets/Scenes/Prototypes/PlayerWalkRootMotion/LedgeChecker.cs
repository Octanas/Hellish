using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeChecker : MonoBehaviour
{
    private Transform player;
    private CharacterController playerController;

    // Start is called before the first frame update
    void Start()
    {
        player = gameObject.transform.parent;
        playerController = player.GetComponent<CharacterController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Ledge")
        {
            Vector3 diffPosition = other.transform.position - transform.position;

            // Move character a total of diffPosition and hold him there
        }
    }
}
