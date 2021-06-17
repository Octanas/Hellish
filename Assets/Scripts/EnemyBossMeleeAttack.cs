using UnityEngine;

public class EnemyBossMeleeAttack : MonoBehaviour
{
    private int attackState = Animator.StringToHash("Base Layer.Attack");

    [SerializeField] private Animator _animator;
    private Rigidbody _rigidbody;
    private Collider weaponCollider;

    public int damage = 20;

    private int damageMultiplier = 1;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        weaponCollider = GetComponent<Collider>();

        weaponCollider.enabled = false;
    }

    public void EnableHit(int damageMultiplier)
    {
        this.damageMultiplier = damageMultiplier;
        weaponCollider.enabled = true;
    }

    public void DisableHit()
    {
        weaponCollider.enabled = false;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (_animator.GetCurrentAnimatorStateInfo(0).fullPathHash == attackState && other.gameObject.CompareTag("Player"))
        {
            Vector3 hitPosition = other.GetContact(0).point;
            Vector3 bolderPosition = transform.position;

            Vector3 knockback = hitPosition - bolderPosition;
            Vector3 normalizedKnockback = knockback.normalized;
            normalizedKnockback.y = 0.2f;
            
            CharacterStats playerStats = other.gameObject.GetComponent<CharacterStats>();

            playerStats?.TakeDamage(damage * damageMultiplier, normalizedKnockback * 3);
        }
    }
}