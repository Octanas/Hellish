using System;
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
        public static readonly int JumpingDown = Animator.StringToHash("Base Layer.JumpingDown");
        public static readonly int Falling = Animator.StringToHash("Base Layer.Falling");
        public static readonly int Landing = Animator.StringToHash("Base Layer.Landing");
        public static readonly int StartingLeap = Animator.StringToHash("Base Layer.StartingLeap");
        public static readonly int Leaping = Animator.StringToHash("Base Layer.Leaping");
        public static readonly int EndingLeap = Animator.StringToHash("Base Layer.EndingLeap");
        public static readonly int LeapDown = Animator.StringToHash("Base Layer.LeapDown");
        public static readonly int LeapStand = Animator.StringToHash("Base Layer.LeapStand");
        public static readonly int StartingBreatheFire = Animator.StringToHash("Base Layer.StartingBreatheFire");
        public static readonly int BreatheFire = Animator.StringToHash("Base Layer.BreatheFire");
        public static readonly int EndingBreatheFire = Animator.StringToHash("Base Layer.EndingBreatheFire");
        public static readonly int DodgeRoll = Animator.StringToHash("Base Layer.DodgeRoll");
        public static readonly int Punch_Slash = Animator.StringToHash("Base Layer.Punch_Slash");
        public static readonly int Kick_Combo = Animator.StringToHash("Base Layer.Kick_Combo");
        public static readonly int SwingZipline = Animator.StringToHash("Base Layer.SwingZipline");
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
        public static readonly string JumpDown = "JumpDown";
        public static readonly string Leap = "Leap";
        public static readonly string Fall = "Fall";
        public static readonly string Land = "Land";
        public static readonly string Dodge = "Dodge";
        public static readonly string FireWall = "FireWall";
        public static readonly string BreatheFire = "BreatheFire";
        public static readonly string StopBreatheFire = "StopBreatheFire";
        public static readonly string HangZipline = "HangZipline";
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
    public Transform hangingPointZipLine;
    /// <summary>
    /// Reference point to be used as origin for floor detection raycast.
    /// </summary>
    [Tooltip("Reference point to be used as origin for floor detection raycast.")]
    [UnityEngine.Serialization.FormerlySerializedAs("floorDetectionSource")]
    public Transform floorDetectionOrigin;
    public Transform jumpingDownDetectionPoint;
    /// <summary>
    /// Damage area when stomping from a leap.
    /// </summary>
    [Tooltip("Damage area when stomping from a leap.")]
    public GameObject stompArea;

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
    private float oldMaxMovingVelocity;

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
    public float timeToLandFromJump = 0.25f;
    public float timeToLandFromLeap = 0.25f;
    public float groundDetectionDistance = 0.5f;
    public float groundDetectionRadius = 0.25f;
    private bool isGrounded = false;

    // JUMPING
    [Header("Jumping")]
    private RaycastHit downgrade;
    private RaycastHit ground;

    // PlayerStats, check if player fell out of the scene
    private PlayerStats myStats;

    // PlayerAttack, stop rotation around enemy
    private PlayerAttack playerAttack;

    // Zipline
    private Vector3 zipLineDirection;
    private float timeStartZip;
    private bool inZip = false;
    public float manaCost = 700;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Gameplay.Move.performed += CaptureMovementDirection;
        controls.Gameplay.Move.canceled += CaptureMovementDirection;

        controls.Gameplay.Jump.performed += Jump;
        controls.Gameplay.Dodge.performed += Dodge;
        controls.Gameplay.Leap.performed += Leap;
        controls.Gameplay.Climb.performed += TriggerClimb;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();
        myStats = GetComponent<PlayerStats>();
        playerAttack = GetComponent<PlayerAttack>();

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
        // If inferior to 0.1, put value to 0 to avoid unnecessary computing
        if (movementInputSpeed <= 0.001 && movementInputAcceleration < 0)
        {
            movementInputSpeed = 0;
            movementInputAcceleration = 0;
        }
        else
        {
            movementInputSpeed = Mathf.SmoothDamp(movementInputSpeed, movementInput.magnitude,
                        ref movementInputAcceleration, movementInputAccelerationTime);
        }

        // Update movement speed on animator to adjust animation
        animator.SetFloat(AnimatorParameters.Movement, movementInputSpeed);

        // Do not change rotation and movement when transitioning to Hanging state or Swing in zipline,
        // to avoid problems when hanging on ledge
        if (nextState.fullPathHash != State.Hanging && nextState.fullPathHash != State.SwingZipline && !inZip && movementInput.magnitude >= 0.1f)
        {
            if (state.fullPathHash == State.Moving || state.fullPathHash == State.Punch_Slash
                || state.fullPathHash == State.Kick_Combo || state.fullPathHash == State.DodgeRoll
                || state.fullPathHash == State.BreatheFire)
            {
                // Set rotation value
                targetAngle = Mathf.Atan2(movementInput.x, movementInput.y) * Mathf.Rad2Deg + playerCamera.eulerAngles.y;
            }
            else if ((state.fullPathHash == State.JumpingUp || state.fullPathHash == State.JumpingDown || state.fullPathHash == State.Falling || state.fullPathHash == State.Leaping))
            {
                // Set rotation value
                targetAngle = Mathf.Atan2(movementInput.x, movementInput.y) * Mathf.Rad2Deg + playerCamera.eulerAngles.y;

                // While velocity is lower than maxAirHorizontalSpeed, player can speed up horizontal movement while falling
                if (Mathf.Sqrt(Mathf.Pow(playerRigidbody.velocity.x, 2) + Mathf.Pow(playerRigidbody.velocity.z, 2)) < maxAirSpeedHorizontal)
                    playerRigidbody.AddForce(transform.forward * airControlForce * movementInputSpeed, ForceMode.Force);

                // Get horizontal movement vector
                Vector3 horizontalMovement = playerRigidbody.velocity.normalized;
                horizontalMovement.y = 0;

                // Add air drag (contrary to horizontal air movement)
                playerRigidbody.AddForce(horizontalMovement * -airDragHorizontal, ForceMode.Force);
            }

            //Stop attack rotation around enemy when player starts moving
            playerAttack.StopRotation();
        }

        if (state.fullPathHash == State.Climbing
            || state.fullPathHash == State.JumpingUp
            || state.fullPathHash == State.Falling
            || state.fullPathHash == State.Landing
            || state.fullPathHash == State.StartingLeap
            || state.fullPathHash == State.Leaping
            || state.fullPathHash == State.EndingLeap
            || state.fullPathHash == State.LeapDown
            || state.fullPathHash == State.DodgeRoll)
        {
            // Adjust collider height during climbing animation, according to the animation curves
            // The obtained value from the curves goes from 0 to 1, 1 being full height, 0 being no height
            float newColliderHeight;

            // Do not reset collider height at the end of the JumpingUp, Falling, StartingLeap or Leaping state
            if (inStateTransition && state.fullPathHash != State.JumpingUp && state.fullPathHash != State.Falling
                && state.fullPathHash != State.StartingLeap && state.fullPathHash != State.Leaping)
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

            // If the current state is DodgeRoll, the top of the collider will move,
            // the bottom will stay in the same position
            float diff = (state.fullPathHash == State.DodgeRoll ? -1 : 1) * (playerCollider.height - newColliderHeight);
            playerCollider.height = newColliderHeight;

            // The collider is moved, so the top of the collider
            // is always in the same spot relative to the character
            Vector3 collCenter = playerCollider.center;
            collCenter.y += diff / 2;
            playerCollider.center = collCenter;
        }

        Vector3 auxVelocity = playerRigidbody.velocity;
        auxVelocity.y = 0;

        /**
         * -- Normal Movement (Walking/Running) -- 
         * A raycast will be fired straight down to detect the ground.
         *
         * The raycast will have a max length of groundDetectionDistance.
         *
         * -- Falling --
         * A raycast will be fired in the direction of velocity to detect the ground, if falling.
         *
         * The raycast will have a max length of the distance to the spot where the player will land in timeToLandFromJump seconds
         * timeToLandFromJump represents the number of seconds necessary for the landing animation to play.
         *
         * The vertical component of the raycast length is calculated taking gravity acceleration into account,
         * so that the landing animation is as synced with the actual landing as possible.
         *
         * -- Leaping --
         * The same as Falling, but instead of using timeToLandFromJump it uses timeToLandFromLeap.
         */
        Vector3 raycastDirection = state.fullPathHash == State.Falling || state.fullPathHash == State.Leaping ? playerRigidbody.velocity.normalized : -transform.up;
        float raycastLength = groundDetectionDistance;

        if (state.fullPathHash == State.Falling)
        {
            raycastLength = Mathf.Sqrt(
                Mathf.Pow(Mathf.Pow(timeToLandFromJump, 2) * -Physics.gravity.y + timeToLandFromJump * -playerRigidbody.velocity.y, 2)
                + (timeToLandFromJump * auxVelocity.magnitude));
        }
        else if (state.fullPathHash == State.Leaping)
        {
            raycastLength = Mathf.Sqrt(
                Mathf.Pow(Mathf.Pow(timeToLandFromLeap, 2) * -Physics.gravity.y + timeToLandFromLeap * -playerRigidbody.velocity.y, 2)
                + (timeToLandFromLeap * auxVelocity.magnitude));
        }

        RaycastHit hitInfo;

        isGrounded = Physics.Raycast(floorDetectionOrigin.position, raycastDirection, out hitInfo, raycastLength, LayerMask.GetMask("Default", "Wood", "Stone"));

        Debug.DrawRay(floorDetectionOrigin.position, raycastDirection * raycastLength, isGrounded ? Color.green : Color.red);

        if (isGrounded)
            ground = hitInfo;

        for (int i = 0; i < 2; i++)
        {
            if (isGrounded)
                break;

            for (int j = 0; j < 2; j++)
            {
                if (isGrounded)
                    break;

                Vector3 raycastOrigin = floorDetectionOrigin.position;

                if (i == 0)
                    raycastOrigin.x -= groundDetectionRadius;
                else
                    raycastOrigin.x += groundDetectionRadius;

                if (j == 0)
                    raycastOrigin.z -= groundDetectionRadius;
                else
                    raycastOrigin.z += groundDetectionRadius;

                isGrounded = Physics.Raycast(raycastOrigin, raycastDirection, out hitInfo, raycastLength, LayerMask.GetMask("Default", "Wood", "Stone"));

                Debug.DrawRay(raycastOrigin, raycastDirection * raycastLength, isGrounded ? Color.green : Color.red);

                if (isGrounded)
                    ground = hitInfo;
            }
        }

        /*
         * -- Deciding between from jumping up and jumping down --
         * downgrade is a raycasthit that will determine if the player jumps down or up;
         * the ray starts on the hips and points to a little bit ahead of the feet;
         * if the ray doesn't collide with anything, the player jumps down
         * 
         * for the debug purposes, the color of the ray changes depending on colliding or not
         * default is blue and green when collides
        */
        Color color = Color.blue;

        Vector3 raycastDown = transform.forward - transform.up;

        bool isDown = Physics.Raycast(jumpingDownDetectionPoint.position, raycastDown, out downgrade, 2, LayerMask.GetMask("Default", "Wood", "Stone"));

        if (downgrade.collider != null)
            color = Color.green;

        Debug.DrawRay(jumpingDownDetectionPoint.position, raycastDown * 2, color);

        // If player is Falling or Leaping and ground is detected, trigger landing
        if (state.fullPathHash == State.Falling || state.fullPathHash == State.Leaping)
        {
            // If ground's normal vector is more than 45 degrees in relation with up vector,
            // do not trigger landing.
            //
            // If ground is to close to player, do not trigger landing,
            // because there is not enough time to play the animation.
            // This is considered a bug, as it does not look nice
            // (https://github.com/Octanas/Hellish/issues/21)
            if (isGrounded && ground.distance >= raycastLength * 0.5
                && Vector3.Angle(Vector3.up, ground.normal) <= 45)
            {
                PrepareForLanding();
            }

            // check if player fell out of the scene
            myStats.CheckFellOut(transform.position.y);

        }
        // If player is hanging on the zip
        else if (state.fullPathHash == State.SwingZipline && inZip)
        {
            // Calculate current movement speed according to time
            timeStartZip += Time.deltaTime * 1;
            maxMovingVelocity += timeStartZip;

            // Only if the player reached the zipline move forward
            // Debug.Log(("distance: " + (transform.position - targetPosition).magnitude));
            if ((transform.position - targetPosition).magnitude < 0.1)//TODO: when rotation is done
            {
                ApplyTranslation(zipLineDirection);
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

        // If player is in BreatheFire state, turning is slower
        float currentTurningTime = turningTime * (state.fullPathHash == State.StartingBreatheFire
            || state.fullPathHash == State.BreatheFire
            || state.fullPathHash == State.EndingBreatheFire ?
            8 : 1);

        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turningVelocity, currentTurningTime);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);

        /*if (inZip)
        {
            Debug.DrawRay(transform.position, transform.forward * 100, Color.red, 100000000f);
        }*/
    }

    /// <summary>
    /// Hang character on ledge.
    /// </summary>
    /// <param name="position">Central position of the ledge.</param>
    /// <param name="orientation">Orientation of the ledge (to where the character will face).</param>
    public void HangOnLedge(Vector3 position, Quaternion orientation)
    {
        // Don't execute if the character is already hanging or climbing
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

        // Disable player colliders while hanging/climbing
        playerCollider.enabled = false;

        // Current player movement stops
        playerRigidbody.velocity = Vector3.zero;

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
        // Will only trigger landing if current state is Falling or Leaping
        // And is not transitioning into Hanging or Climbing
        if ((state.fullPathHash == State.Falling || state.fullPathHash == State.Leaping) &&
            nextState.fullPathHash != State.Hanging && nextState.fullPathHash != State.Climbing && nextState.fullPathHash != State.SwingZipline && !inZip)
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
        // And is not transitioning into Hanging or Climbing
        // And has not been triggered to transition into Hanging
        // (This function can be called right after the trigger,
        // which will set applyRootMotion to false and breka Hanging)
        // SUGGESTION: animator.applyRootMotion = false should probably go to an animation event
        // Problem with this suggestion: player loses velocity when between
        // the beginning of the animation and the event being fired
        if (state.fullPathHash == State.Moving && nextState.fullPathHash != State.Hanging && nextState.fullPathHash != State.Climbing && nextState.fullPathHash != State.SwingZipline && !inZip
            && !animator.GetBool(AnimatorParameters.Hang))
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
        Vector3 velocity = playerRigidbody.velocity;
        velocity.y = 0;

        // Will only trigger jump if current state is Moving and is not transitioning to one of these states
        if (state.fullPathHash == State.Moving && nextState.fullPathHash != State.DodgeRoll
            && nextState.fullPathHash != State.JumpingUp && nextState.fullPathHash != State.StartingLeap)
        {
            // if there is a downgrade ahead and the player is almost still, it triggers jump down instead of normal jumping
            if (downgrade.collider == null && velocity.magnitude < 0.1f)
            {
                animator.applyRootMotion = false;
                animator.SetTrigger(AnimatorParameters.JumpDown);
            }
            else
            {
                // Disable root motion so movement persists through jumping state
                animator.applyRootMotion = false;
                animator.SetTrigger(AnimatorParameters.Jump);
            }
        }
    }

    /// <summary>
    /// Consumes dodge input.
    /// </summary>
    /// <param name="context">Input callback context.</param>F
    private void Dodge(InputAction.CallbackContext context)
    {
        // Will only trigger jump if current state is Moving and is not transitioning to one of these states
        if (state.fullPathHash == State.Moving && nextState.fullPathHash != State.DodgeRoll
            && nextState.fullPathHash != State.JumpingUp && nextState.fullPathHash != State.StartingLeap
            || state.fullPathHash == State.Punch_Slash || state.fullPathHash == State.Kick_Combo)
        {
            animator.SetTrigger(AnimatorParameters.Dodge);
        }
    }

    /// <summary>
    /// Consumes leap input.
    /// </summary>
    /// <param name="context">Input callback context.</param>
    private void Leap(InputAction.CallbackContext context)
    {
        if (!myStats.CheckLeap(manaCost))
        {
            return;
        }
        // Will only trigger jump if current state is Moving and is not transitioning to one of these states
        if (state.fullPathHash == State.Moving && nextState.fullPathHash != State.DodgeRoll
            && nextState.fullPathHash != State.JumpingUp && nextState.fullPathHash != State.StartingLeap)
        {
            // Disable root motion so movement persists through leaping state
            animator.applyRootMotion = false;
            animator.SetTrigger(AnimatorParameters.Leap);
            myStats.useMana(manaCost);
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
        FMODUnity.RuntimeManager.PlayOneShotAttached("event:/Player/Jump/jum off grass", gameObject);

        // Apply jumping force
        if (state.fullPathHash == State.JumpingDown)
            // Jumping Down force - slightly up and forward
            playerRigidbody.AddForce(new Vector3(transform.forward.x * 3, transform.forward.y + 2, transform.forward.z * 3), ForceMode.Impulse);
        else if (state.fullPathHash == State.StartingLeap)
            // Leaping force - up and forward
            playerRigidbody.AddForce(new Vector3(transform.forward.x * 5, 12, transform.forward.z * 5), ForceMode.Impulse);
        else
            // Leaping Up force - up
            playerRigidbody.AddForce(new Vector3(0, 8, 0), ForceMode.Impulse);

    }

    /// <summary>
    /// Executes on leap animation event.
    /// </summary>
    private void PlayLeapSound()
    {
        FMODUnity.RuntimeManager.PlayOneShotAttached("event:/Player/Jump/fire_jump", gameObject);
    }

    /// <summary>
    /// Executes on walk and sprint animation step event.
    /// </summary>
    /// <param name="maxSpeed">Max speed at which the sound can be triggered</param>
    private void PlayStepSound(float maxSpeed)
    {
        if (movementInputSpeed <= 0.01 || movementInputSpeed > maxSpeed || !isGrounded)
            return;

        int groundLayer = ground.collider.gameObject.layer;

        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.4f, LayerMask.GetMask("Water"));

        if(colliders.Length > 0)
            FMODUnity.RuntimeManager.PlayOneShotAttached("event:/Player/Water/water_step", gameObject);
        else if (groundLayer == LayerMask.NameToLayer("Default"))
            FMODUnity.RuntimeManager.PlayOneShotAttached("event:/Player/Grass/Running_on_Grass", gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }

    /// <summary>
    /// Executes on dodge animation event.
    /// </summary>
    private void OnDodge()
    {
        // Reset triggers that maybe have been set during the dodge
        animator.ResetTrigger(AnimatorParameters.Dodge);
        animator.ResetTrigger(AnimatorParameters.Jump);
        animator.ResetTrigger(AnimatorParameters.Leap);
        animator.ResetTrigger(AnimatorParameters.BreatheFire);
        animator.ResetTrigger(AnimatorParameters.FireWall);
    }

    /// <summary>
    /// Executes on landing animation event.
    /// </summary>
    private void OnLand()
    {
        FMODUnity.RuntimeManager.PlayOneShotAttached("event:/Player/Grass/Landing_on_Grass", gameObject);

        // Re-enable root motion to give position control back to animations
        animator.applyRootMotion = true;

        // Disable any left land or fall triggers
        animator.ResetTrigger(AnimatorParameters.Land);
        animator.ResetTrigger(AnimatorParameters.Fall);

        // Reset triggers that maybe have been set during the jump
        animator.ResetTrigger(AnimatorParameters.Dodge);
        animator.ResetTrigger(AnimatorParameters.Jump);
        animator.ResetTrigger(AnimatorParameters.Leap);
        animator.ResetTrigger(AnimatorParameters.BreatheFire);
        animator.ResetTrigger(AnimatorParameters.FireWall);

        // Expand stomp area on Leap land
        if (state.fullPathHash == State.EndingLeap)
        {
            // DOUBT: may need a raycast here to get precise landing position

            // Instantiate Stomp Area in the point that the ground detection collider hit the ground
            Instantiate(stompArea, ground.point, Quaternion.identity);
        }
    }

    /// <summary>
    /// Executes on climbing animation event.
    /// </summary>
    private void OnClimb()
    {
        // Re-enable player collider after climbing
        playerCollider.enabled = true;
    }

    /// <summary>
    /// Hang character on zipline.
    /// </summary>
    /// <param name="position">Central position of the zipline.</param>
    /// <param name="orientation">Orientation of the zipline (to where the character will face).</param>
    /// <param name="name">Collider game object name</param>
    /// <param name="direction">Direction of the zipline.</param>
    public void HangOnZipline(Vector3 position, Quaternion orientation, Vector3 direction)
    {
        if (state.fullPathHash == State.Falling
            || nextState.fullPathHash == State.Falling
            || nextState.fullPathHash == State.JumpingUp
            || state.fullPathHash == State.JumpingUp
            /*|| nextState.fullPathHash == State.Landing 
            || state.fullPathHash == State.Landing*/)
        {
            // Don't execute if the character is already hanging in the zipline
            if (state.fullPathHash == State.SwingZipline || inZip)
                return;
            if (nextState.fullPathHash == State.SwingZipline)
                return;


            // Make sure root motion will be applied
            animator.applyRootMotion = true;

            animator.SetTrigger(AnimatorParameters.HangZipline);
            animator.SetBool("InZip", true);

            // Disable triggers
            animator.ResetTrigger(AnimatorParameters.Fall);
            animator.ResetTrigger(AnimatorParameters.Land);

            // Force update animator to start state transition immediately 
            // deltaTime is 0 so animations are not affected
            // CONFIRM THIS - https://forum.unity.com/threads/question-about-animator-force-update-with-deltatime-0.1094890/
            //
            // This fixes an issue with FixedUpdate running after trigger is set,
            // but before transition to Hanging state starts
            animator.Update(0f);

            // Disable player colliders
            playerCollider.enabled = false;

            // Current player movement stops
            playerRigidbody.velocity = Vector3.zero;

            // Character is temporarily rotated to target orientation
            // to calculate necessary translation relative to the final hand position
            Quaternion oldRotation = transform.rotation;
            transform.rotation = orientation;

            // Vector from hands to center of ledge
            Vector3 diffPosition = position - hangingPointZipLine.position;

            zipLineDirection = direction;
            oldMaxMovingVelocity = maxMovingVelocity;
            timeStartZip = 1;
            inZip = true;

            // Reset position
            transform.rotation = oldRotation;

            ApplyTranslation(diffPosition);
            //Debug.DrawRay(transform.position, new Vector3(direction.x, 0, direction.z)*100, Color.cyan, 100000000f);
            SetAngle(orientation.eulerAngles.y);
        }
    }

    /// <summary>
    /// Take character out of zipline.
    /// </summary>
    public void ExitZipline()
    {
        if (state.fullPathHash != State.SwingZipline)
            return;

        inZip = false;
        translate = false;

        // Reset max velocity
        maxMovingVelocity = oldMaxMovingVelocity;

        // Current player movement stops
        //playerRigidbody.velocity = Vector3.zero;

        // Enable player colliders
        playerCollider.enabled = true;

        // Disable root motion so movement persists through falling state
        animator.applyRootMotion = false;
        animator.SetBool("InZip", false);
        animator.SetTrigger(AnimatorParameters.Fall);

        // Force update animator to start state transition immediately 
        // deltaTime is 0 so animations are not affected
        // CONFIRM THIS - https://forum.unity.com/threads/question-about-animator-force-update-with-deltatime-0.1094890/
        //
        // This fixes an issue with FixedUpdate running after trigger is set,
        // but before transition to Hanging state starts
        animator.Update(0f);

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
