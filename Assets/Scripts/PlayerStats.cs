using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : CharacterStats
{
    private bool fireBreath;
    private bool fireWall;
    private bool leap;
    private PopUpInstructions _popUp;
    public Slider slider;
    private Rigidbody _rigidbody;

    // Time without taking damage necessary to enable recover
    public float intervalTime = 10f;

    // Fell Out
    public int minY = -10;
    private bool _fellOut = false;
    public CinemachineVirtualCamera fellOutCamera;
    public Transform playerCamera;

    protected override void Start()
    {
        base.Start();

        fireBreath = false;
        fireWall = false;
        leap = false;
        _rigidbody = GetComponent<Rigidbody>();

        _popUp = GetComponent <PopUpInstructions>();
    }

    protected override void FillBar()
    {
        slider.value = maxHealth;
    }

    protected override void Recover()
    {
        // Player health recovery system
        // If player:
        // - time without being attacked > interval time 
        // - TODO: not in attack mode?
        // - TODO: radius?
        if (CurrentHealth < maxHealth && TimeWithoutTakingDamage > intervalTime)
            CurrentHealth += 0.5f;

    }
    protected override void UpdateBarHealth()
    {
        // Update Bar health [0,1]
        slider.value = Math.Max(CurrentHealth, 0);
    }

    protected override void Die()
    {
        // Dying due to falling out of the scene
        if (_fellOut)
            return;

        // Dying due to enemies damage
        _animator.SetTrigger("Death");
        GameOver();
    }

    protected override void HitReaction(Vector3 knockback)
    {
        if (knockback.magnitude != 0)
            _animator.applyRootMotion = false;

        _animator.SetTrigger("Hit");

        _rigidbody.AddForce(knockback, ForceMode.Impulse);
    }
    public override void UpgradeHealthBar()
    {
        maxHealth += 500;
    }
    public override void UpgradeManaBar()
    {
        maxMana += 500;
    }
    public void GainFireBreath() {
        fireBreath = true;
        _popUp.enableFireBreath();
    }
    public void GainFireWall() {
        fireWall = true;
        _popUp.enableFireWall();
    }
    public void GainLeap() {
        leap = true;
        _popUp.enableLeap();
    }

    public bool CheckBreath() {
        return fireBreath;
    }
    public bool CheckWall() {
        return fireWall;
    }
    public bool CheckLeap() {
        return leap;
    }
    public void CheckFellOut(float y)
    {
        if (y < minY)
        {
            _fellOut = true;

            // Take damage
            if (CurrentHealth < 50f) CurrentHealth = 0;
            else CurrentHealth -= 50f;
            UpdateBarHealth();
        }

        if (CurrentHealth <= 0)
        {
            // Exchange cameras
            // Set position and rotation of fell out camera according to last player position
            Vector3 position = playerCamera.position;
            Quaternion rotation = playerCamera.localRotation;

            // TODO: increase y?
            // position.y = minY;

            // Increase priority of fell out camera
            fellOutCamera.ForceCameraPosition(position, rotation);
            fellOutCamera.Priority = 11; //The other is 10

            GameOver();
        }
    }

    private void GameOver()
    {
        //TODO: deactivate controls when player is dead
        GetComponent<PlayerAttack>().enabled = false; // x -> equip weapon
        GetComponent<PlayerMovement>().enabled = false;
        this.enabled = false;

        //TODO: game over screen


        Debug.Log("GAME OVER");
    }
}

