using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : CharacterStats
{
    protected override void HitReaction(Vector3 knockback)
    {
        _animator.SetTrigger("Hit");

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