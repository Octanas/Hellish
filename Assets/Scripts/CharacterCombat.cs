using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class CharacterCombat : MonoBehaviour
{
    private CharacterStats _myStats;
    private float _waitForNextAttack = 0f;

    [SerializeField] private float attackCooldown = 2f;

    private void Start()
    {
        _myStats = GetComponent<CharacterStats>();
    }

    private void Update()
    {
        _waitForNextAttack -= Time.deltaTime;
    }

    public bool attack(CharacterStats targetStats)
    {
        // One hit can't cause multiple damage to the same enemy
        if (_waitForNextAttack <= 0f)
        {
            // The attack damage will depend on who is attacking and who is been attacked, different enemies can have
            // different maxHealths
            _waitForNextAttack = attackCooldown;
            targetStats.TakeDamage(_myStats.damagePower);

            return true;
        }

        return false;
    }
}
