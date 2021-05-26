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

    protected int CurrentHealth;
    protected float TimeWithoutTakingDamage = 0f;

    private bool isEnemy;

    void Awake()
    {
        CurrentHealth = maxHealth;
        isEnemy = !barHealth;
    }

    private void Update()
    {
        // Update time without taking damage
        TimeWithoutTakingDamage += Time.deltaTime;

        // Update Bar health [0,1]
        if (!isEnemy)
        {
            if (CurrentHealth > 0) Recover(); // Character health recovery system
            barHealth.fillAmount = (float) CurrentHealth / maxHealth;
        }
    }


    public void TakeDamage(int damage)
    {
        TimeWithoutTakingDamage = 0;
        Debug.Log(transform.name + " takes " + damage + " damage.");
        CurrentHealth -= damage;
        if (CurrentHealth <= 0)
            Die();
        if (isEnemy)
        {
            //TODO add animation for when hit?
        }
    }

    protected virtual void Recover()
    {
    }

    protected virtual void Die(){}
}