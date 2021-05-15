using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterStats : MonoBehaviour
{
    public int maxHealth = 100;
    public int damagePower = 20;

    // UI test
    public Image barHealth;
    
    protected float CurrentHealth;
    protected float TimeWithoutTakingDamage = 0f;
    
    void Awake()
    {
        CurrentHealth = maxHealth;
    }

    private void Update()
    {
        // Update time without taking damage
        TimeWithoutTakingDamage += Time.deltaTime;
        
        // Update Bar health [0,1]
        if (barHealth)
            barHealth.fillAmount = CurrentHealth/maxHealth ;
        
        // Character health recovery system
        Recover();
    }

    
    public void TakeDamage(int damage)
    {
        TimeWithoutTakingDamage = 0;
        Debug.Log(transform.name + " takes " + damage + " damage.");
        CurrentHealth -= damage;
        if (CurrentHealth <= 0)
            Die();
    }
    
    // TODO: override the next methods for each character (enemy, player)
    protected virtual void Recover() {}
    protected virtual void Die() {}
    
}