using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System;

public class EnemyStats : CharacterStats
{
    public Slider sliderHealth;
    private Transform _target;
    private float _chaseTargetRadius;


    protected override void Start()
    {
        base.Start();
        // Use singleton instead of inserting manually
        _target = PlayerManager.Instance.player.transform;
        _chaseTargetRadius = GetComponent<EnemyController>().chaseTargetRadius;
    }

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

        if (sliderHealth)
            Destroy(sliderHealth.gameObject);

        StartCoroutine(Disappear());
    }

    private IEnumerator Disappear()
    {
        yield return new WaitForSeconds(10);

        Destroy(gameObject);
    }

    protected override void FillBar()
    {
        if (sliderHealth)
        {
            sliderHealth.maxValue = maxHealth;
            sliderHealth.value = maxHealth;
        }
    }

    protected override void UpdateBarHealth()
    {
        // Update Bar health [0,1]
        if (sliderHealth)
            sliderHealth.value = Math.Max(CurrentHealth, 0);
    }

    protected override void Recover()
    {
        Vector3 targetDirection = _target.position - transform.position;
        float targetDistance = targetDirection.magnitude;

        // Check if target is not on radius
        if (targetDistance > _chaseTargetRadius)
        {
            base.Recover();
        }
    }
}