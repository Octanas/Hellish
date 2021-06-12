using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class CharacterStats : MonoBehaviour
{
    public int maxHealth = 1000;
    public int damagePower = 200;

    // UI - Health Bar 
    public Image barHealth;

    protected float CurrentHealth;
    protected float TimeWithoutTakingDamage = 0f;

    void Awake()
    {
        CurrentHealth = maxHealth;
        FillBar();
    }

    private void Update()
    {
        // Update time without taking damage
        TimeWithoutTakingDamage += Time.deltaTime;

        // Character health recovery system
        if (CurrentHealth > 0)
        {
            Recover();
            UpdateBarHealth();
        }
    }

    public void TakeDamage(int damage)
    {
        TakeDamage(damage, Vector3.zero);
    }

    public void TakeDamage(int damage, Vector3 knockback)
    {
        TimeWithoutTakingDamage = 0;
        CurrentHealth -= damage;
        UpdateBarHealth();

        Debug.Log(transform.name + " takes " + damage + " damage.");
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Die();
        }
        else HitReaction(knockback);
    }

    protected virtual void UpdateBarHealth()
    {
    }

    protected virtual void FillBar()
    {
    }

    protected virtual void Recover()
    {
    }

    protected virtual void Die()
    {
    }

    protected virtual void HitReaction(Vector3 knockback)
    {
    }
}