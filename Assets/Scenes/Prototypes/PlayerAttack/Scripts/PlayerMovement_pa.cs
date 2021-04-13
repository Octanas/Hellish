using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement_pa : MonoBehaviour
{
    PlayerControls controls;
    private Animator animator;
    public Transform cameraTransform;

    private float currentTurningVelocity;
    public float turningTime = 0.1f;

    private bool sprinting = false;
    private float currentSpeed = 0f;
    private Vector2 movementInput = Vector2.zero;

    private float currentAcceleration;
    public float accelerationTime = 0.5f;
    private bool attacking = false;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Gameplay.Sprint.performed += context => sprinting = true;
        controls.Gameplay.Sprint.canceled += context => sprinting = false;

        controls.Gameplay.Move.performed += context => movementInput = context.ReadValue<Vector2>();
        controls.Gameplay.Move.canceled += context => movementInput = Vector2.zero;
        
        //attack
        controls.Gameplay.Attack.performed += context => attacking = true;
        controls.Gameplay.Attack.canceled += context => attacking = false;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        currentSpeed = Mathf.SmoothDamp(currentSpeed, movementInput.magnitude, ref currentAcceleration, accelerationTime);

        animator.SetFloat("Movement", currentSpeed);

        if (movementInput.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(movementInput.x, movementInput.y) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref currentTurningVelocity, turningTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
        
        //attack
        animator.SetBool("Attack", attacking);
    }

    private void OnEnable()
    {
        controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        controls.Gameplay.Disable();
    }
}