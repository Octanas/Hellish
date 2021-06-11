using UnityEngine;

public class EnemyBossController : EnemyController
{
    protected override void AttackTarget()
    {
        _agent.speed = 0;
        base.AttackTarget();
    }

    public void RegainMovement()
    {
        _agent.speed = maxMovingVelocity;
    }
}