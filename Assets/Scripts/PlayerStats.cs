using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : CharacterStats
{
    public float intervalTime = 10f;
    private Animator _animator;
    
    void Start()
    {
        _animator = GetComponent<Animator>();
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

    protected override void Die()
    {
        //TODO: deactivate controls when player is dead
        GetComponent<PlayerAttack>().enabled = false;
        GetComponent<PlayerMovement>().enabled = false;

        _animator.SetTrigger("Death");
        //TODO: game over screen
        this.enabled = false;
    }
}

