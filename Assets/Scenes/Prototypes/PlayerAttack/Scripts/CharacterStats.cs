using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public int maxHealth = 100;
    public int damagePower = 20;
    [SerializeField]
    public int currentHealth { get; private set; }
    
    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void takeDamage(int damage)
    {
        Debug.Log(transform.name + " takes " + damage + " damage.");
        currentHealth -= damage;
        if (currentHealth <= 0)
            Die();
    }
    
    // TODO: override the next methods for each character (enemy, player)
    public virtual void Die() {}
    
}