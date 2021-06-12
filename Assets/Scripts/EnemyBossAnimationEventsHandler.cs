using UnityEngine;

public class EnemyBossAnimationEventsHandler : MonoBehaviour
{
    [SerializeField] private EnemyBossMeleeAttack enemyBossMeleeAttack;

    public void EnableHit(int damageMultiplier)
    {
        enemyBossMeleeAttack.EnableHit(damageMultiplier);
    }

    public void DisableHit()
    {
        enemyBossMeleeAttack.DisableHit();
    }
}