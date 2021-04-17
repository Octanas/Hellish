using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages player movement.
/// </summary>
public class PlayerMovementClimb : MonoBehaviour
{
    // STATE MANAGEMENT

    /// <summary>
    /// Player states (in accordance to the animations in the animator).
    /// Each constant contains the hash value that represents each state.
    /// These values should be compared with <see cref="AnimatorStateInfo.fullPathHash"/>.
    /// </summary>
    public static class State
    {
        public static readonly int Moving = Animator.StringToHash("Base Layer.Moving");
        public static readonly int Hanging = Animator.StringToHash("Base Layer.Hanging");
        public static readonly int Climbing = Animator.StringToHash("Base Layer.Climbing");
    }

    /// <summary>
    /// Animator parameters (in accordance to the parameters set in the animator).
    /// Each constant contains the string value of each parameter.
    /// It does not contain information about the type of each parameter.
    /// </summary>
    public static class AnimatorParameters
    {
        public static readonly string Movement = "Movement";
        public static readonly string Hang = "Hang";
        public static readonly string Climb = "Climb";
        public static readonly string ColliderHeight = "ColliderHeight";
    }

    /// <summary>
    /// Current state in the animator.
    /// </summary>
    private AnimatorStateInfo state;
    /// <summary>
    /// Next state in the animator (if in a transition).
    /// </summary>
    private AnimatorStateInfo nextState;
    private bool inStateTransition;

    // EXTERNAL REFERENCES
    private PlayerControls controls;
    private Animator animator;
    private Rigidbody playerRigidbody;
    private CapsuleCollider playerCollider;
    public Transform playerCamera;
    /// <summary>
    /// Reference point to where the hands will be when hanging on a ledge.
    /// </summary>
    [Tooltip("Reference point to where the hands will be when hanging on a ledge.")]
    public Transform hangingPoint;

    // COLLIDER ADJUSTMENT
    private float defaultColliderHeight;
    private float colliderDiff = -1;
    private float originalColliderHeight = -1;

    // CHARACTER TRANSLATION
    private bool translate = false;
    private Vector3 targetPosition;
    private Vector3 movingVelocity;
    public float movingTime = 0.1f;
    public float maxMovingVelocity = 10f;

    // CHARACTER ROTATION
    private float targetAngle;
    private float turningVelocity;
    public float turningTime = 0.1f;

    // MOVEMENT INPUT
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

        // Initialize variables
        state = animator.GetCurrentAnimatorStateInfo(0);
        nextState = animator.GetNextAnimatorStateInfo(0);
        inStateTransition = animator.IsInTransition(0);

        targetPosition = transform.position;
        targetAngle = transform.eulerAngles.y;
        defaultColliderHeight = playerCollider.height;
    }

    private void FixedUpdate()
    {
        // Update state variables
        state = animator.GetCurrentAnimatorStateInfo(0);
        nextState = animator.GetNextAnimatorStateInfo(0);
        inStateTransition = animator.IsInTransition(0);

        // Calculate current movement speed
        // It will gradually decrease/increase, so the animations and movement are smoother
        movementInputSpeed = Mathf.SmoothDamp(movementInputSpeed,
            state.fullPathHash == State.Moving && !inStateTransition || nextState.fullPathHash == State.Moving ?
                movementInput.magnitude :
                0,
            ref movementInputAcceleration, movementInputAccelerationTime);

        // Update movement speed on animator to adjust animation
        animator.SetFloat(AnimatorParameters.Movement, movementInputSpeed);

        // Set rotation value
        // Do not change rotation when transitioning to Hanging state,
        // to avoid wrong orientation when hanging on ledge
        if (state.fullPathHash == State.Moving && nextState.fullPathHash != State.Hanging && movementInput.magnitude >= 0.1f)
            targetAngle = Mathf.Atan2(movementInput.x, movementInput.y) * Mathf.Rad2Deg + playerCamera.eulerAngles.y;

        if (state.fullPathHash == State.Climbing)
        {
            // Adjust collider height during climbing animation, according to the animation curves
            // The obtained value from the curves goes from 0 to 1, 1 being full height, 0 being no height
            float newColliderHeight;

            if (animator.IsInTransition(0))
            {
                // During a transition, gradually adjust collider to full length
                // Relative to the transition's length

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

            // The collider is moved, so the top of the collider
            // is always in the same spot relative to the character
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

    /// <summary>
    /// Returns the current state of the character.
    /// </summary>
    /// <returns>Current state of the character.</returns>
    public AnimatorStateInfo GetState()
    {
        return state;
    }

    /// <summary>
    /// Set new position for the character (change will be gradual).
    /// </summary>
    /// <param name="position">New position.</param>
    public void SetPosition(Vector3 position)
    {
        targetPosition = position;
        translate = true;
    }

    /// <summary>
    /// Apply translation to character (change will be gradual).
    /// </summary>
    /// <param name="translation">Translation to be applied.</param>
    public void ApplyTranslation(Vector3 translation)
    {
        targetPosition = transform.position + translation;
        translate = true;
    }

    /// <summary>
    /// Set new orientation for the character (change will be gradual).
    /// </summary>
    /// <param name="angle">New orientation.</param>
    public void SetAngle(float angle)
    {
        targetAngle = angle;
    }

    /// <summary>
    /// Apply rotation to character (change will be gradual).
    /// </summary>
    /// <param name="rotation">Rotation to be applied.</param>
    public void ApplyRotation(float rotation)
    {
        targetAngle = transform.eulerAngles.y + rotation;
    }

    /// <summary>
    /// Move the character to the target position gradually.
    /// </summary>
    private void Move()
    {
        // Movement will only be applied if translate flag is set to true
        // This flag is only true after the targetPosition has been changed
        // and will be set to false after the targetPosition is reached
        // This way, root motion is not affected unintentionally.
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

    /// <summary>
    /// Rotate the character to the target orientation gradually.
    /// </summary>
    private void Rotate()
    {
        if (Mathf.Abs(transform.eulerAngles.y - targetAngle) < 0.001)
            return;

        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turningVelocity, turningTime);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }

    /// <summary>
    /// Hang character on ledge.
    /// </summary>
    /// <param name="position">Central position of the ledge.</param>
    /// <param name="orientation">Orientation of the ledge (to where the character will face).</param>
    public void HangOnLedge(Vector3 position, Quaternion orientation)
    {
        // Only valid if character is in Moving state
        // and not already transitioning to Hanging state
        if (state.fullPathHash != State.Moving)
            return;

        if (nextState.fullPathHash == State.Hanging)
            return;

        animator.SetTrigger(AnimatorParameters.Hang);

        // Force update animator to start state transition immediately 
        // deltaTime is 0 so animations are not affected
        // CONFIRM THIS - https://forum.unity.com/threads/question-about-animator-force-update-with-deltatime-0.1094890/
        //
        // This fixes an issue with FixedUpdate running after trigger is set,
        // but before transition to Hanging state starts
        animator.Update(0f);

        // Character is temporarily rotated to target orientation
        // to calculate necessary translation relative to the final hand position
        Quaternion oldRotation = transform.rotation;
        transform.rotation = orientation;

        // Vector from hands to center of ledge
        Vector3 diffPosition = position - hangingPoint.position;

        // Get character's forward and up vectors and project them to diffPosition's length
        // This will result in the necessary translation to put the character on the ledge,
        // without moving him sideways along it
        Vector3 translationVectorForward = transform.forward;
        translationVectorForward = Vector3.Project(diffPosition, translationVectorForward);

        Vector3 translationVectorUp = transform.up;
        translationVectorUp = Vector3.Project(diffPosition, translationVectorUp);

        Vector3 translationVector = translationVectorForward + translationVectorUp;

        // Reset position
        transform.rotation = oldRotation;

        ApplyTranslation(translationVector);
        SetAngle(orientation.eulerAngles.y);
    }

    /// <summary>
    /// Consumes movement input.
    /// </summary>
    /// <param name="context">Input callback context.</param>
    private void CaptureMovementDirection(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// Consumes climb trigger input.
    /// </summary>
    /// <param name="context">Input callback context.</param>
    private void TriggerClimb(InputAction.CallbackContext context)
    {
        // Will only trigger climb if current state is Hanging
        if (state.fullPathHash == State.Hanging)
        {
            animator.SetTrigger(AnimatorParameters.Climb);
        }
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
