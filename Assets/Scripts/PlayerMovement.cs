using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages player movement.
/// </summary>
public class PlayerMovement : MonoBehaviour
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
        public static readonly int JumpingUp = Animator.StringToHash("Base Layer.JumpingUp");
        public static readonly int Falling = Animator.StringToHash("Base Layer.Falling");
        public static readonly int Landing = Animator.StringToHash("Base Layer.Landing");
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
        public static readonly string Jump = "Jump";
        public static readonly string Fall = "Fall";
        public static readonly string Land = "Land";
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
    [Header("External References")]
    public Transform playerCamera;
    /// <summary>
    /// Reference point to where the hands will be when hanging on a ledge.
    /// </summary>
    [Tooltip("Reference point to where the hands will be when hanging on a ledge.")]
    public Transform hangingPoint;
    /// <summary>
    /// Reference point to where the camera looks at (used for floor detection raycast too).
    /// </summary>
    [Tooltip("Reference point to where the camera looks at (used for floor detection raycast too).")]
    public Transform lookAt;

    // COLLIDER ADJUSTMENT
    private float defaultColliderHeight;
    private float colliderDiff = -1;
    private float originalColliderHeight = -1;

    // CHARACTER TRANSLATION
    private bool translate = false;
    private Vector3 targetPosition;
    private Vector3 movingVelocity;
    [Header("Character Translation")]
    public float movingTime = 0.1f;
    public float maxMovingVelocity = 10f;

    // CHARACTER ROTATION
    private float targetAngle;
    private float turningVelocity;
    [Header("Character Rotation")]
    public float turningTime = 0.1f;

    // MOVEMENT INPUT
    private Vector2 movementInput = Vector2.zero;
    private float movementInputSpeed = 0f;
    private float movementInputAcceleration;
    [Header("Movement Input")]
    public float movementInputAccelerationTime = 0.5f;

    // AIR CONTROL
    [Header("Air Control")]
    public float maxAirSpeedHorizontal = 7f;
    public float airDragHorizontal = 3f;
    public float airControlForce = 10f;

    // FALLING
    [Header("Falling")]
    public float timeToLand = 0.25f;
    public float groundDetectionDistance = 0.5f;
    private bool isGrounded = false;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Gameplay.Move.performed += CaptureMovementDirection;
        controls.Gameplay.Move.canceled += CaptureMovementDirection;

        controls.Gameplay.Jump.performed += Jump;
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
        movementInputSpeed = Mathf.SmoothDamp(movementInputSpeed, movementInput.magnitude,
            ref movementInputAcceleration, movementInputAccelerationTime);

        // Update movement speed on animator to adjust animation
        animator.SetFloat(AnimatorParameters.Movement, movementInputSpeed);

        // Do not change rotation and movement when transitioning to Hanging state,
        // to avoid problems when hanging on ledge
        if (nextState.fullPathHash != State.Hanging && movementInput.magnitude >= 0.1f)
        {
            if (state.fullPathHash == State.Moving)
            {
                // Set rotation value
                targetAngle = Mathf.Atan2(movementInput.x, movementInput.y) * Mathf.Rad2Deg + playerCamera.eulerAngles.y;
            }
            else if ((state.fullPathHash == State.JumpingUp || state.fullPathHash == State.Falling))
            {
                // Set rotation value
                targetAngle = Mathf.Atan2(movementInput.x, movementInput.y) * Mathf.Rad2Deg + playerCamera.eulerAngles.y;

                // While velocity is lower than maxAirHorizontalSpeed, player can speed up horizontal movement while falling
                if (Mathf.Sqrt(Mathf.Pow(playerRigidbody.velocity.x, 2) + Mathf.Pow(playerRigidbody.velocity.z, 2)) < maxAirSpeedHorizontal)
                    playerRigidbody.AddForce(transform.forward * airControlForce * movementInputSpeed, ForceMode.Force);

                // Get horizonta movement vector
                Vector3 horizontalMovement = playerRigidbody.velocity.normalized;
                horizontalMovement.y = 0;

                // Add air drag (contrary to horizontal air movement)
                playerRigidbody.AddForce(horizontalMovement * -airDragHorizontal, ForceMode.Force);
            }
        }

        if (state.fullPathHash == State.Climbing
            || state.fullPathHash == State.JumpingUp
            || state.fullPathHash == State.Falling
            || state.fullPathHash == State.Landing)
        {
            // Adjust collider height during climbing animation, according to the animation curves
            // The obtained value from the curves goes from 0 to 1, 1 being full height, 0 being no height
            float newColliderHeight;

            // Do not reset collider height at the end of the JumpingUp state
            if (animator.IsInTransition(0) && state.fullPathHash != State.JumpingUp)
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


        // Detect ground with straight down raycast
        // FIXME: the raycast should actually be fired in the direction of movement,
        // to detect the floor where the player will land and not the floor right beneath him
        // (https://github.com/Octanas/Hellish/issues/16)
        isGrounded = Physics.Raycast(lookAt.position, -transform.up,
            state.fullPathHash == State.Falling ? timeToLand * -playerRigidbody.velocity.y : groundDetectionDistance,
            LayerMask.GetMask("Default"));

        // If player is Falling and ground is detected, trigger landing
        if (state.fullPathHash == State.Falling)
        {
            if (isGrounded)
            {
                PrepareForLanding();
            }
        }
        // If ground is not detected and player isn't falling, trigger fall
        else if (!isGrounded)
        {
            Fall();
        }

        Rotate();
        Move();
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
        // Only executes if the character is already hanging or climbing
        if (state.fullPathHash == State.Hanging || state.fullPathHash == State.Climbing)
            return;

        if (nextState.fullPathHash == State.Hanging)
            return;

        // Make sure root motion will be applied
        animator.applyRootMotion = true;

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
    /// Trigger landing state.
    /// </summary>
    private void PrepareForLanding()
    {
        // Will only trigger landing if current state is Falling
        if (state.fullPathHash == State.Falling)
        {
            animator.SetTrigger(AnimatorParameters.Land);
        }
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
    /// Trigger falling state.
    /// </summary>
    private void Fall()
    {
        // Will only trigger fall if current state is Moving
        if (state.fullPathHash == State.Moving)
        {
            // Disable root motion so movement persists through falling state
            animator.applyRootMotion = false;
            animator.SetTrigger(AnimatorParameters.Fall);
        }
    }

    /// <summary>
    /// Consumes jump input.
    /// </summary>
    /// <param name="context">Input callback context.</param>
    private void Jump(InputAction.CallbackContext context)
    {
        // Will only trigger jump if current state is Moving
        if (state.fullPathHash == State.Moving)
        {
            // Disable root motion so movement persists through jumping state
            animator.applyRootMotion = false;
            animator.SetTrigger(AnimatorParameters.Jump);
        }
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
            
            // Disable any left fall triggers from when the character was hanging/climbing
            animator.ResetTrigger(AnimatorParameters.Fall);
        }
    }

    /// <summary>
    /// Executes on jumping animation event.
    /// </summary>
    private void OnJump()
    {
        // Apply jumping force
        playerRigidbody.AddForce(new Vector3(0, 8, 0), ForceMode.Impulse);
    }

    /// <summary>
    /// Executes on landing animation event.
    /// </summary>
    private void OnLand()
    {
        // Re-enable root motion to give position control back to animations
        animator.applyRootMotion = true;

        // Disable any left land or fall triggers
        animator.ResetTrigger(AnimatorParameters.Land);
        animator.ResetTrigger(AnimatorParameters.Fall);
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
