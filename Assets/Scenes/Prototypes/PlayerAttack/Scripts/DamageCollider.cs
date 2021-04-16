using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCollider : MonoBehaviour
{
    private Collider damageCollider;

    private void Awake()
    {
        damageCollider = GetComponent<Collider>();
        damageCollider.gameObject.SetActive(true);
        damageCollider.isTrigger = true; 
        damageCollider.enabled = false;//the collider itself can be enable and disable but the gameobject remains active
    }

    public void enableDamageCollider()
    {
        damageCollider.enabled = true;
    }
    
    public void disableDamageCollider()
    {
        damageCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Enemy")){
            CharacterCombat myCombat= collision.GetComponent<CharacterCombat>();
            CharacterStats enemyStats = collision.GetComponent<CharacterStats>();
            if (myCombat != null && enemyStats != null) {
                myCombat.attack(enemyStats);
            }
        }
    }
}
