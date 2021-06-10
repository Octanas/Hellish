using System;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    [SerializeField] private int damagePower;
    [SerializeField] private float lifetime;

    private void Update()
    {
        lifetime -= Time.deltaTime;
        if (lifetime <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<CharacterStats>().TakeDamage(damagePower);
        }

        Destroy(gameObject);
    }
}