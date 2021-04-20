using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerAttack : MonoBehaviour
{
    private PlayerControls _controls;
    private WeaponManager _weaponManager;
    private Animator _animator;
    private bool _isAttacking;
    
    // Enable the damage collider of the current "weapon" (player hand, player foot or weapon) and turn off after
    // animation is already done with animation events in the WeaponManager
    // TODO: choose the right current "weapon", _currentWeapon should be something like WeaponItem
    private DamageCollider _currentWeapon;
    
    private void Awake()
    {
        _controls = new PlayerControls();
        _controls.Gameplay.Attack.performed += context => _isAttacking = true;
        _controls.Gameplay.Attack.canceled += context => _isAttacking = false;
    }
    
    private void Start()
    {
        _weaponManager = GetComponent<WeaponManager>();
        _animator = GetComponent<Animator>();
        
        // TODO: load the right weapon instead of the DamageCollision on the lefHand
        _currentWeapon = GetComponentInChildren<DamageCollider>();
        _weaponManager.LoadWeapon(_currentWeapon);
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
