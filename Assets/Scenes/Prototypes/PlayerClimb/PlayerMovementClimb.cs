using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementClimb : MonoBehaviour
{
    // SUGGESTION: Should try to use the Animator has the sole state holder.
    public enum State
    {
        Moving,
        Hanging,
        Climbing
    }

    private PlayerControls controls;
    private Animator animator;
    private Rigidbody playerRigidbody;
    public Transform cameraTransform;

    private bool translate = false;
    private Vector3 targetPosition;
    private Vector3 movingVelocity;
    public float movingTime = 0.1f;
    public float maxMovingVelocity = 10f;

    private float targetAngle;
    private float turningVelocity;
    public float turningTime = 0.1f;

    private Vector2 movementInput = Vector2.zero;
    private float movementInputSpeed = 0f;
    private float movementInputAcceleration;
    public float movementInputAccelerationTime = 0.5f;

    private State state;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Gameplay.Move.performed += context => movementInput = context.ReadValue<Vector2>();
        controls.Gameplay.Move.canceled += context => movementInput = Vector2.zero;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();

        state = State.Moving;

        targetPosition = transform.position;
        targetAngle = transform.eulerAngles.y;
    }

    private void Update()
    {
        if (state == State.Moving)
        {
            movementInputSpeed = Mathf.SmoothDamp(movementInputSpeed, movementInput.magnitude, ref movementInputAcceleration, movementInputAccelerationTime);

            animator.SetFloat("Movement", movementInputSpeed);

            if (movementInput.magnitude >= 0.1f)
                targetAngle = Mathf.Atan2(movementInput.x, movementInput.y) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
        }

        Rotate();
        Move();
    }

    public void ChangeState(State state)
    {
        switch (state)
        {
            case State.Moving:
                playerRigidbody.useGravity = true;
                break;
            case State.Hanging:
                playerRigidbody.useGravity = false;
                playerRigidbody.velocity = Vector3.zero;
                animator.SetBool("Hanging", true);
                break;
        }

        this.state = state;
    }

    public State GetState()
    {
        return state;
    }

    public void SetPosition(Vector3 position)
    {
        targetPosition = position;
        translate = true;
    }

    public void ApplyTranslation(Vector3 translation)
    {
        targetPosition = transform.position + translation;
        translate = true;
    }

    public void SetAngle(float angle)
    {
        targetAngle = angle;
    }

    public void ApplyRotation(float rotation)
    {
        targetAngle = transform.eulerAngles.y + rotation;
    }

    private void Move()
    {

        if (!translate)
            return;

        if ((transform.position - targetPosition).magnitude < 0.0001)
        {
            translate = false;
            return;
        }

        Vector3 position = Vector3.SmoothDamp(transform.position, targetPosition, ref movingVelocity, movingTime, maxMovingVelocity);
        transform.position = position;
    }

    private void Rotate()
    {
        if (Mathf.Abs(transform.eulerAngles.y - targetAngle) < 0.0001)
            return;

        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turningVelocity, turningTime);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);
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
