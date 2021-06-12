using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : CharacterStats
{
    private Animator _animator;

    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    protected override void HitReaction(Vector3 knockback)
    {
        GetComponent<EnemyController>().WarnEnemies();
    }

    protected override void Die()
    {
        GetComponent<EnemyController>().enabled = false;
        _animator.SetTrigger("Death");
        StartCoroutine(Disappear());
    }

    private IEnumerator Disappear()
    {
        yield return new WaitForSeconds(10);
        Destroy(gameObject);
    }
}