using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementClimb : MonoBehaviour
{
    public static class State
    {
        public static readonly int Moving = Animator.StringToHash("Base Layer.Moving");
        public static readonly int Hanging = Animator.StringToHash("Base Layer.Hanging");
        public static readonly int Climbing = Animator.StringToHash("Base Layer.Climbing");
    }

    public static class AnimatorParameters
    {
        public static readonly string Movement = "Movement";
        public static readonly string Hang = "Hang";
        public static readonly string Climb = "Climb";
        public static readonly string ColliderHeight = "ColliderHeight";
    }

    private AnimatorStateInfo state;
    private AnimatorStateInfo nextState;

    private PlayerControls controls;
    private Animator animator;
    private Rigidbody playerRigidbody;
    private CapsuleCollider playerCollider;
    public Transform cameraTransform;
    public Transform hangingPoint;

    private float defaultColliderHeight;
    private float colliderDiff = -1;
    private float originalColliderHeight = -1;

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

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Gameplay.Move.performed += CaptureMovementDirection;
        controls.Gameplay.Move.canceled += CaptureMovementDirection;

        controls.Gameplay.Climb.performed += TriggerClimb;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();

        state = animator.GetCurrentAnimatorStateInfo(0);
        nextState = animator.GetNextAnimatorStateInfo(0);

        targetPosition = transform.position;
        targetAngle = transform.eulerAngles.y;
        defaultColliderHeight = playerCollider.height;
    }

    private void FixedUpdate()
    {
        state = animator.GetCurrentAnimatorStateInfo(0);
        nextState = animator.GetNextAnimatorStateInfo(0);

        // FIXME: if abruptly stopped, the movement value can stay not 0
        if (state.fullPathHash == State.Moving && nextState.fullPathHash != State.Hanging)
        {
            movementInputSpeed = Mathf.SmoothDamp(movementInputSpeed, movementInput.magnitude, ref movementInputAcceleration, movementInputAccelerationTime);

            animator.SetFloat(AnimatorParameters.Movement, movementInputSpeed);

            if (movementInput.magnitude >= 0.1f)
                targetAngle = Mathf.Atan2(movementInput.x, movementInput.y) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
        }
        else if (state.fullPathHash == State.Climbing)
        {
            float newColliderHeight;

            if (animator.IsInTransition(0))
            {
                if (colliderDiff == -1)
                {
                    colliderDiff = defaultColliderHeight - playerCollider.height;
                    originalColliderHeight = playerCollider.height;
                }

                float normalizedTransitionTime = animator.GetAnimatorTransitionInfo(0).normalizedTime;

                if (normalizedTransitionTime >= 0.95)
                {
                    newColliderHeight = defaultColliderHeight;
                    colliderDiff = -1;
                    originalColliderHeight = -1;
                }
                else
                    newColliderHeight = colliderDiff * normalizedTransitionTime + originalColliderHeight;
            }
            else
            {
                newColliderHeight = animator.GetFloat(AnimatorParameters.ColliderHeight) * defaultColliderHeight;
            }

            float diff = playerCollider.height - newColliderHeight;
            playerCollider.height = newColliderHeight;

            Vector3 collCenter = playerCollider.center;
            collCenter.y += diff / 2;
            playerCollider.center = collCenter;
        }

        Rotate();
        Move();
    }

    // TODO: remove ASAP
    public void ChangeState(int stateHash)
    {
        if (stateHash == State.Moving)
        {
            playerRigidbody.useGravity = true;

        }
        else if (stateHash == State.Hanging)
        {
            playerRigidbody.useGravity = false;
            playerRigidbody.velocity = Vector3.zero;
            animator.SetBool(AnimatorParameters.Hang, true);
        }
        else if (stateHash == State.Climbing)
        {
            animator.SetTrigger(AnimatorParameters.Climb);
            animator.SetBool(AnimatorParameters.Hang, false);
            playerRigidbody.useGravity = true;
        }
    }

    public AnimatorStateInfo GetState()
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

        if ((transform.position - targetPosition).magnitude < 0.001)
        {
            translate = false;
            return;
        }

        Vector3 position = Vector3.SmoothDamp(transform.position, targetPosition, ref movingVelocity, movingTime, maxMovingVelocity);
        transform.position = position;
    }

    private void Rotate()
    {
        if (Mathf.Abs(transform.eulerAngles.y - targetAngle) < 0.001)
            return;

        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turningVelocity, turningTime);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }

    public void HangOnLedge(Vector3 position, Quaternion orientation)
    {
        if (state.fullPathHash != State.Moving)
            return;

        if (nextState.fullPathHash == State.Hanging)
            return;

        animator.SetTrigger(AnimatorParameters.Hang);
        animator.Update(0f);

        Quaternion oldRotation = transform.rotation;
        transform.rotation = orientation;

        Vector3 diffPosition = position - hangingPoint.position;

        // FIXME: this will not always be X
        diffPosition.x = 0;

        transform.rotation = oldRotation;

        // Move character a total of diffPosition
        ApplyTranslation(diffPosition);
        SetAngle(orientation.eulerAngles.y);
    }

    #region Input event methods
    private void CaptureMovementDirection(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    private void TriggerClimb(InputAction.CallbackContext context)
    {
        // Will only trigger climb if current state is hanging
        if (state.fullPathHash == State.Hanging)
        {
            animator.SetTrigger(AnimatorParameters.Climb);
        }
    }
    #endregion

    private void OnEnable()
    {
        controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        controls.Gameplay.Disable();
    }
}
