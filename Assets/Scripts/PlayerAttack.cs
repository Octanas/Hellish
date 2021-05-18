using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = System.Object;

public class PlayerAttack : MonoBehaviour
{
    public List<SkinnedMeshRenderer> ListBolsaMeshes;
    public MeshRenderer sword;
    public bool hasSword = false;
    
    // Enable the damage collider of the current "weapon" and turn off after the animation is done,
    // with animation events, functions are available in the WeaponManager script on the player
    // Maybe use later
    //private WeaponManager _weaponManager;
    //public DamageCollider _currentDamageCollider;
    
    private PlayerControls _controls;
    private Animator _animator;
    private bool _isAttacking;
    
    public AnimationClip[] defaultAnimationClips;
    public AnimationClip[] swordAnimationClips;
    private AnimationClip[] _currAnimationClips;
    private AnimatorOverrideController _overrideController;

    private void Awake()
    {
        _controls = new PlayerControls();
        _controls.Gameplay.Attack.performed += context => _isAttacking = true;
        _controls.Gameplay.Attack.canceled += context => _isAttacking = false;

        _controls.Gameplay.Equip.performed += _ => ChangeWeapon();
    }

    private void Start()
    {
        // Maybe use later
        //_weaponManager = GetComponent<WeaponManager>();
        //_currentDamageCollider = GetComponentInChildren<DamageCollider>();
        //_weaponManager.LoadCollider(_currentDamageCollider);
        
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
    
    private void Update()
    {
        _animator.SetBool("Attack", _isAttacking);
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
