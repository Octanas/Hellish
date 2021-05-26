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

    protected override void Die()
    {
        GetComponent<EnemyController>().enabled = false;
        _animator.SetTrigger("Death");
        StartCoroutine(Disappear());
    }

    private IEnumerator Disappear()
    {
        yield return new WaitForSeconds(4);
        Destroy(gameObject);
    }
}