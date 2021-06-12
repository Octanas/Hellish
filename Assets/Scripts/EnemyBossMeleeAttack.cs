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
        Debug.Log("Hits enabled: " + damageMultiplier);

        this.damageMultiplier = damageMultiplier;
        weaponCollider.enabled = true;
    }

    public void DisableHit()
    {
        Debug.Log("Hits disabled");

        weaponCollider.enabled = false;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (_animator.GetCurrentAnimatorStateInfo(0).fullPathHash == attackState && other.gameObject.CompareTag("Player"))
        {
            // TODO: apply force to player

            CharacterStats playerStats = other.gameObject.GetComponent<CharacterStats>();

            playerStats.TakeDamage(damage * damageMultiplier);
        }
    }
}