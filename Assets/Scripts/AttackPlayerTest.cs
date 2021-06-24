using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackPlayerTest : MonoBehaviour
{
    private CharacterCombat _myCombat;

    void Start()
    {
        _myCombat = GetComponentInParent<CharacterCombat>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            CharacterStats playerStats = other.gameObject.GetComponent<CharacterStats>();
            _myCombat.attack(playerStats);
        }
    }
}
