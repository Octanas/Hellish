using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireWallEffect : MonoBehaviour
{
    public int damage = 500;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        // Deal damage to enemy
        if (other.CompareTag("Enemy"))
        {
            CharacterStats enemyStats = other.GetComponentInParent<CharacterStats>();

            if (enemyStats)
                enemyStats.TakeDamage(damage);
        }
    }
}
