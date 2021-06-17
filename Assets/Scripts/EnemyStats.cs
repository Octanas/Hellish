using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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
        gameObject.GetComponent<NavMeshAgent>().baseOffset = 0;
        var wings = GetComponent<Wings>();
        if (wings) wings.enabled = false;
        StartCoroutine(Disappear());
    }

    private IEnumerator Disappear()
    {
        yield return new WaitForSeconds(10);
        Destroy(gameObject);
    }
}