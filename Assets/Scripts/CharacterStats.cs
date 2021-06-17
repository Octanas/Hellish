using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class CharacterStats : MonoBehaviour
{
    protected Animator _animator;

    public int maxHealth = 1000;
    public int damagePower = 200;
    protected float CurrentHealth;

    protected float TimeWithoutTakingDamage = 0f;

    // Time without taking damage necessary to enable recover
    public float intervalTime = 10f;

    [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;

    [SerializeField] private Material hitMaterial;

    private Material defaultMaterial;

    [SerializeField]
    [Tooltip("States that cannot be interrupted by hits.")]
    private String[] unstoppableStates;

    protected virtual void Awake()
    {
        CurrentHealth = maxHealth;
        FillBar();
    }

    protected virtual void Start()
    {
        _animator = GetComponent<Animator>();

        if (_skinnedMeshRenderer)
            defaultMaterial = _skinnedMeshRenderer.material;
    }

    protected virtual void FixedUpdate()
    {
        // Update time without taking damage
        TimeWithoutTakingDamage += Time.fixedDeltaTime;

        // Character health recovery system
        if (CurrentHealth > 0)
        {
            Recover();
            UpdateBarHealth();
        }
    }

    public float GetHealth()
    {
        return CurrentHealth;    
    }
    
    public void TakeDamage(int damage)
    {
        TakeDamage(damage, Vector3.zero);
    }

    public void TakeDamage(int damage, Vector3 knockback)
    {
        TimeWithoutTakingDamage = 0;
        CurrentHealth -= damage;
        UpdateBarHealth();

        Debug.Log(transform.name + " takes " + damage + " damage.");
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Die();
        }
        else
        {
            int currentState = _animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
            int nextState = _animator.GetNextAnimatorStateInfo(0).fullPathHash;

            // Check current state, if it is an unstoppable state
            // do not trigger hit reaction
            foreach (String state in unstoppableStates)
            {
                if (currentState == Animator.StringToHash(state) || nextState == Animator.StringToHash(state))
                {
                    // Show hit indication on character
                    if (_skinnedMeshRenderer)
                    {
                        _skinnedMeshRenderer.material = hitMaterial;
                        StartCoroutine(ChangeToDefaultMaterial());
                    }

                    return;
                }
            }

            HitReaction(knockback);
        }
    }

    private IEnumerator ChangeToDefaultMaterial()
    {
        yield return new WaitForSecondsRealtime(0.1f);

        if (_skinnedMeshRenderer)
            _skinnedMeshRenderer.material = defaultMaterial;
    }

    private void PlayPreDeathSound()
    {
        FMODUnity.RuntimeManager.PlayOneShotAttached("event:/Player/Death/drop dead heavy", gameObject);
    }

    private void PlayDeathSound()
    {
        FMODUnity.RuntimeManager.PlayOneShotAttached("event:/Player/Death/drop dead fast", gameObject);
    }

    protected virtual void UpdateBarHealth()
    {
    }

    protected virtual void FillBar()
    {
    }

    protected virtual void Recover()
    {
        if (CurrentHealth < maxHealth && TimeWithoutTakingDamage > intervalTime)
            CurrentHealth += 1.5f;
    }

    protected virtual void Die()
    {
    }

    protected virtual void HitReaction(Vector3 knockback)
    {
    }

    public virtual void UpgradeHealthBar()
    {
    }
    public virtual void UpgradeManaBar()
    {
    }
}