using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class CharacterCombat : MonoBehaviour
{
    private CharacterStats _myStats;
    private float _waitForNextAttack = 0f;

    private void Start()
    {
        _myStats = GetComponent<CharacterStats>();
    }

    private void Update()
    {
        _waitForNextAttack -= Time.deltaTime;
    }

    public void attack(CharacterStats targetStats)
    {
        // TODO: This only works kinda for one enemy
        // TODO: add a EnemyStats that when attacked: (1) has a immune interval instead of the code below (2) spots the player even if he can't see him
        // One hit can't cause multiple damage to the same enemy
        if (_waitForNextAttack <= 0f)
        {
            // The attack damage will depend on who is attacking and who is been attacked, different enemies can have
            // different maxHealths
            targetStats.TakeDamage(_myStats.damagePower);
            _waitForNextAttack = 1f;
        }
    }
}
