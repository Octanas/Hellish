using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;
using Object = System.Object;

public class PlayerAttack : MonoBehaviour
{
    [Header("External References")]
    [SerializeField]
    private SwordDamageCollider swordDamageCollider;

    [Header("Sword & Bolsa Meshes:")]
    public List<SkinnedMeshRenderer> ListBolsaMeshes;
    public MeshRenderer sword;
    public bool hasSword = false;

    private PlayerControls _controls;
    private Animator _animator;
    private bool _isAttacking;

    [Header("Animations:")]
    public AnimationClip[] defaultAnimationClips;
    public AnimationClip[] swordAnimationClips;
    private AnimationClip[] _currAnimationClips;
    private AnimatorOverrideController _overrideController;

    [Header("Move while attacking:")]
    public float movingTime = 0.1f;
    public float maxMovingVelocity = 10f;
    public float movementForce = 2.5f;
    private bool _move = false;
    private Vector3 _targetPosition;
    private Vector3 movingVelocity;

    [Header("Rotate in target direction:")]
    public float radius = 10;
    public float turningTime = 0.1f;
    private Transform _target;
    private float turningVelocity;
    private bool _rotate = false;

    private void Awake()
    {
        _controls = new PlayerControls();
        _controls.Gameplay.Attack.started += context => _isAttacking = true;
        _controls.Gameplay.Attack.canceled += context => _isAttacking = false;

        _controls.Gameplay.Equip.started += _ => ChangeWeapon();
    }

    private void Start()
    {
        _animator = GetComponent<Animator>();

        // Initializes meshes state depending on hasSword
        UpdateMeshes();

        // Easy way to swap out any of the clips in our animator controller for some other clip
        // change punch->sword_slash, and kick->sword_combo
        _overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
        _animator.runtimeAnimatorController = _overrideController;
        // Initialize clips depending on hasSword
        UpdateClips();
    }

    private void ChangeWeapon()
    {
        hasSword = !hasSword;
        // Update meshes state
        UpdateMeshes();
        // Update "weapon" animations clips
        UpdateClips();
        // Update "avatar mask - right hand layer"
        _animator.SetLayerWeight(1, hasSword ? 1 : 0);

        FMODUnity.RuntimeManager.PlayOneShotAttached(hasSword ? "event:/Player/Sword/Sword Take out 2" : "event:/Player/Sword/Take Sword Out", gameObject);
    }

    private void UpdateMeshes()
    {
        sword.enabled = hasSword;// Equips or unequips sword
        ListBolsaMeshes[0].enabled = hasSword;// Empty bolsa
        ListBolsaMeshes[1].enabled = !hasSword;// Bolsa with Sword
    }
    private void UpdateClips()
    {
        _currAnimationClips = hasSword ? swordAnimationClips : defaultAnimationClips;
        _overrideController["punch"] = _currAnimationClips[0];
        _overrideController["kick"] = _currAnimationClips[1];

    }

    private void FixedUpdate()
    {
        // Adds +1 position to movement
        if (_move)
        {
            Vector3 position = Vector3.SmoothDamp(transform.position, _targetPosition, ref movingVelocity, movingTime, maxMovingVelocity);
            transform.position = position;
        }

        // If target exists, then face it 
        if (_target && _rotate)
        {

            // Target direction
            Vector3 direction = (_target.position - transform.position).normalized;
            Quaternion rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

            // Angle to add to rotation y
            float targetAngle = rotation.eulerAngles.y;


            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turningVelocity, turningTime);
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
        }


    }

    private void Update()
    {
        _animator.SetBool("Attack", _isAttacking);

        // Start rotation around target 
        if (_isAttacking)
            _rotate = true;

        if (_rotate)
            FindEnemies();

    }

    private void FindEnemies()
    {
        // Find all enemies in the radius 
        Collider[] enemiesColliderRadius = Physics.OverlapSphere(transform.position, radius, LayerMask.GetMask("Enemy"));

        // Reset target as null
        if (enemiesColliderRadius.Length == 0)
            _target = null;

        // Check for the nearest enemy
        float distance = radius;
        foreach (var enemyCollider in enemiesColliderRadius)
        {
            Transform enemy = enemyCollider.transform;
            float enemyDistance = (transform.position - enemy.position).magnitude;
            if (!_target || distance > enemyDistance)
            {
                _target = enemy;
                distance = enemyDistance;
            }
        }
    }

    // Stop rotation when movement begins
    public void StopRotation()
    {
        _rotate = false;
    }

    /// <summary>
    ///  Event Function in attack animations
    /// - initiates movement
    /// </summary>
    public void ApplyMovement()
    {
        UpdateTargetPosition();
        _move = true;
    }

    private void UpdateTargetPosition()
    {
        // Get movement speed from animator
        float speed = _animator.GetFloat("Movement");

        Vector3 aux = transform.forward;
        aux.x *= speed * movementForce;
        aux.z *= speed * movementForce;

        _targetPosition = transform.position + aux;

    }

    /// <summary>
    ///  Event Function in attack animations
    ///  - stops movement
    /// </summary>
    public void StopMovement()
    {
        _move = false;
    }

    private void PlayAttackSound()
    {
        if (hasSword)
            FMODUnity.RuntimeManager.PlayOneShotAttached("event:/Player/Sword/Sword Swing", gameObject);
        else
            FMODUnity.RuntimeManager.PlayOneShotAttached("event:/Player/Attacks/highkick_miss", gameObject);
    }

    private void ActivateHeavySound()
    {
        swordDamageCollider.heavySound = true;
    }

    private void DeactivateHeavySound()
    {
        swordDamageCollider.heavySound = false;
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
