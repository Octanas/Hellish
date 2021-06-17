using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBarController : MonoBehaviour
{
    public Transform healthTransform;
    public float initialHealth;
    private Vector3 targetHealth;
    public Transform manaTransform;
    public float initialMana;
    private Vector3 targetMana;

    private PlayerStats stats;
    void Start()
    {
        stats = GetComponent<PlayerStats>();
    }

    // Update is called once per frame
    void Update()
    {
        targetHealth = Vector3.Lerp(healthTransform.localScale, new Vector3(stats.maxHealth/initialHealth, healthTransform.localScale.y, healthTransform.localScale.z), Time.deltaTime * 3);

        healthTransform.localScale = targetHealth;

        targetMana = Vector3.Lerp(manaTransform.localScale, new Vector3(stats.maxMana/initialMana, manaTransform.localScale.y, manaTransform.localScale.z), Time.deltaTime * 3);

        manaTransform.localScale = targetMana;
    }
}
