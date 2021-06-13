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
    public int maxMana = 1000;
    public int damagePower = 200;

    // UI - Health Bar 
    public Image barHealth;

    protected float CurrentHealth;
    protected float TimeWithoutTakingDamage = 0f;

    [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;

    [SerializeField] private Material hitMaterial;

    private Material defaultMaterial;

    [SerializeField]
    [Tooltip("States that cannot be interrupted by hits.")]
    private String[] unstoppableStates;

    void Awake()
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

    private void Update()
    {
        // Update time without taking damage
        TimeWithoutTakingDamage += Time.deltaTime;

        // Character health recovery system
        if (CurrentHealth > 0)
        {
            Recover();
            UpdateBarHealth();
        }
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

            // Check current state, if it is an unstoppable state
            // do not trigger hit reaction
            foreach (String state in unstoppableStates)
            {
                if (currentState == Animator.StringToHash(state))
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

    protected virtual void UpdateBarHealth()
    {
    }

    protected virtual void FillBar()
    {
    }

    protected virtual void Recover()
    {
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