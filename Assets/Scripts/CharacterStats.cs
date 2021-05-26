using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterStats : MonoBehaviour
{
    public int maxHealth = 1000;
    public int damagePower = 200;

    // UI test
    public Image barHealth;

    protected float CurrentHealth;
    protected float TimeWithoutTakingDamage = 0f;

    private bool isPlayer;

    void Awake()
    {
        CurrentHealth = maxHealth;
        isPlayer = barHealth;
        if (isPlayer) barHealth.fillAmount = 1;
    }

    private void Update()
    {
        // Update time without taking damage
        TimeWithoutTakingDamage += Time.deltaTime;

        // Update Bar health [0,1]
        if (isPlayer)
        {
            if (CurrentHealth > 0) Recover(); // Character health recovery system
            barHealth.fillAmount = (float) Math.Max(CurrentHealth, 0) / maxHealth;
        }
    }


    public void TakeDamage(int damage)
    {
        TimeWithoutTakingDamage = 0;
        CurrentHealth -= damage;
        if (isPlayer) barHealth.fillAmount = (float) Math.Max(CurrentHealth, 0) / maxHealth;
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Die();
        }

        if (isPlayer)
        {
            //TODO add animation for when hit?
        }
    }

    protected virtual void Recover()
    {
    }

    protected virtual void Die()
    {
    }
}