using UnityEngine;

/// <summary>
/// Handle fire breath particles.
/// </summary>
public class FireBreathParticles : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    private Collider _collider;

    /// <summary>
    /// Duration of the collider (to give damage).
    /// </summary>
    public float colliderDuration = 1.82f;

    /// <summary>
    /// Speed at which the particles move.
    /// </summary>
    public float particleSpeed = 5f;

    /// <summary>
    /// Damage that the fire particles does to enemies.
    /// </summary>
    public int damage = 50;

    private void Start()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        _collider = GetComponent<Collider>();
    }

    private void Update()
    {
        // Disable collider after some specified time,
        // so no more damage is dealt
        if (_particleSystem.time >= colliderDuration)
            _collider.enabled = false;
    }

    private void FixedUpdate()
    {
        transform.position += transform.forward * particleSpeed * Time.fixedDeltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Deal damage to enemy
        if (other.CompareTag("Enemy"))
        {
            CharacterStats enemyStats = other.GetComponentInParent<CharacterStats>();

            if (enemyStats)
                enemyStats.TakeDamage(damage);
        }
    }

    private void OnDrawGizmos()
    {
        // Draw gizmo sphere that appears while collider is active
        Gizmos.color = Color.red;

        if(_collider && _collider.enabled)
            Gizmos.DrawSphere(transform.position, 0.2f);
    }
}
