using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;
using Object = System.Object;

public class PlayerAttack : MonoBehaviour
{
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

    private void Awake()
    {
        _controls = new PlayerControls();
        _controls.Gameplay.Attack.performed += context => _isAttacking = true;
        _controls.Gameplay.Attack.canceled += context => _isAttacking = false;

        _controls.Gameplay.Equip.performed += _ => ChangeWeapon();
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
        
    }
    private void Update()
    {
        _animator.SetBool("Attack", _isAttacking);

        // If move is still possible and player keeps trying to move
        // - update target position
        // if (_move)
        //     UpdateTargetPosition();
        
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
        float spped = _animator.GetFloat("Movement");
        
        Vector3 aux = transform.forward;
        aux.x *= spped*movementForce;
        aux.z *= spped*movementForce;
        
        Debug.Log(aux);
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
    
    private void OnEnable()
    {
        _controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        _controls.Gameplay.Disable();
    }
}
