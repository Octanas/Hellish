using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : CharacterStats
{
    private Animator _animator;
    
    // Time without taking damage necessary to enable recover
    public float intervalTime = 10f;

    // Fell Out
    public int minY = -10;
    private bool _fellOut = false;
    public Transform playerCamera;

    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    protected override void FillBar()
    {
        barHealth.fillAmount = 1;
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
        barHealth.fillAmount = Math.Max(CurrentHealth, 0) / maxHealth;
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

    protected override void HitReaction()
    {
        //TODO add animation for when hit?
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
            // TODO: Stop camera
           /* Vector3 position = playerCamera.position;
            position.y += 20;
            Quaternion rotation = playerCamera.rotation;
            playerCamera.SetPositionAndRotation(position, rotation);*/
            
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

