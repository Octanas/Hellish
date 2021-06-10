using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackPlayerTest : MonoBehaviour
{
    private CharacterCombat _myCombat;

    void Start()
    {
        _myCombat = GetComponent<CharacterCombat>();
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CharacterStats playerStats = collision.gameObject.GetComponent<CharacterStats>();
            _myCombat.attack(playerStats);
        }
    }
}
