using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class CharacterStats : MonoBehaviour
{
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

    void Awake()
    {
        CurrentHealth = maxHealth;
        FillBar();
    }

    private void Start()
    {
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

        if (_skinnedMeshRenderer)
        {
            if (!defaultMaterial)
                defaultMaterial = _skinnedMeshRenderer.material;

            _skinnedMeshRenderer.material = hitMaterial;
            StartCoroutine(ChangeToDefaultMaterial());
        }

        Debug.Log(transform.name + " takes " + damage + " damage.");
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Die();
        }
        else HitReaction(knockback);
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