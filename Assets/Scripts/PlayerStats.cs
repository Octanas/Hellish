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
    public int maxMana = 1000;
    private float CurrentMana;
    private bool leap;
    private PopUpInstructions _popUp;
    public Slider sliderHealth;
    public Slider sliderMana;
    private Rigidbody _rigidbody;

    public float intervalTimeMana = 3f;

    // Fell Out
    public int minY = -10;
    private bool _fellOut = false;
    public CinemachineVirtualCamera fellOutCamera;
    public Transform playerCamera;

    protected override void Awake()
    {
        base.Awake();
        CurrentMana = maxMana;
        FillManaBar();
    }
    protected override void Start()
    {
        base.Start();

        fireBreath = false;
        fireWall = false;
        leap = false;
        _rigidbody = GetComponent<Rigidbody>();

        _popUp = GetComponent <PopUpInstructions>();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (CurrentHealth > 0)
        {
            UpdateBarMana();
        }
    }

    protected override void FillBar()
    {
        sliderHealth.value = maxHealth;
    }
    protected void FillManaBar()
    {
        sliderMana.value = maxHealth;
    }

    protected override void Recover()
    {
        base.Recover();

        if (CurrentMana < maxMana)
            CurrentMana += 0.5f;
    }
    protected void UpdateBarMana()
    {
        sliderMana.value = Math.Max(CurrentMana, 0);
    }
    protected override void UpdateBarHealth()
    {
        // Update Bar health [0,1]
        sliderHealth.value = Math.Max(CurrentHealth, 0);
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
        sliderHealth.maxValue = maxHealth;
    }
    public override void UpgradeManaBar()
    {
        maxMana += 500;
        sliderMana.maxValue = maxMana;
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

    public bool CheckBreath(float mana) {
        return fireBreath && CurrentMana >= mana;
    }
    public bool CheckWall(float mana) {
        return fireWall && CurrentMana >= mana;
    }
    public bool CheckLeap(float mana) {
        return leap && CurrentMana >= mana;
    }
    public void useMana(float mana) {
        CurrentMana -= mana;
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

