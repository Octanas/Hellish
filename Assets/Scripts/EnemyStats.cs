using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System;

public class EnemyStats : CharacterStats
{ 
    public Slider sliderHealth;
    
    protected override void HitReaction(Vector3 knockback)
    {
        _animator.SetTrigger("Hit");

        GetComponent<EnemyController>().WarnEnemies();
    }

    protected override void Die()
    {
        GetComponent<EnemyController>().enabled = false;
        _animator.SetTrigger("Death");
        gameObject.GetComponent<NavMeshAgent>().baseOffset = 0;
        var wings = GetComponent<Wings>();
        if (wings) wings.enabled = false;
        StartCoroutine(Disappear());
    }

    private IEnumerator Disappear()
    {
        yield return new WaitForSeconds(10);
        Destroy(sliderHealth.gameObject);//sliderHealth.gameObject.SetActive(false);
        Destroy(gameObject);
    }
    
    protected override void FillBar()
    {
        if(sliderHealth)
            sliderHealth.value = maxHealth;
    }
    
    protected override void UpdateBarHealth()
    {
        // Update Bar health [0,1]
        if(sliderHealth)
            sliderHealth.value = Math.Max(CurrentHealth, 0);
    }

}