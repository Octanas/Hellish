using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : CharacterStats
{
    public float intervalTime = 10f;
    
    protected override void Recover()
     {
         // Player health recovery system
         // If player:
         // - time without being attacked > interval time 
         // - TODO: not in attack mode?
         // - TODO: radius?
         if (CurrentHealth < maxHealth && TimeWithoutTakingDamage > intervalTime)
             CurrentHealth += 0.1f;
     }

    protected override void Die()
    {
        print("u dead");
        //TODO: apply death animation and game over screen
    }
}

