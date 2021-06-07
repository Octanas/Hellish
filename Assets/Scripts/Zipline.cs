using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zipline : MonoBehaviour
{
    private PlayerMovement _playerMovement;

    void Start()
    {
        _playerMovement = GetComponentInParent<PlayerMovement>();
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer == LayerMask.NameToLayer("Zipline"))
        {
            // Position
            Vector3 aux1 = collider.transform.position;
            // Next Position
            Vector3 aux2 = collider.gameObject.transform.GetChild(0).position;
            // Direction
            Vector3 direction = ( aux2 - aux1).normalized;
            Debug.DrawLine(aux1, aux2, Color.yellow);
            // Target direction
            Quaternion rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            

            _playerMovement.HangOnZipline(aux1, rotation);
        }
    }
}
