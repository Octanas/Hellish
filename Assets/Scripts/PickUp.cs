using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PickUp : MonoBehaviour
{
    private PlayerControls _controls;
    private GameObject chest;

    private void Awake()
    {
        _controls = new PlayerControls();
        _controls.Gameplay.PickUp.started += TriggerPickUp;
    }
    void Start()
    {
        chest = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter (Collision collider) {
        if (collider.gameObject.CompareTag("Chest"))
            chest = collider.gameObject;
    }
    private void OnCollisionExit (Collision collider) {
        if (collider.gameObject.CompareTag("Chest"))
            chest = null;
    }
    private void TriggerPickUp (InputAction.CallbackContext context) {
        chest?.GetComponent<ChestManager>().pickUpItems();
    }

    private void OnEnable()
    {
        _controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        _controls.Gameplay.Disable();
    }
}
