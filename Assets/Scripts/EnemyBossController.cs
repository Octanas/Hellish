using UnityEngine;

public class EnemyBossController : EnemyController
{
    // TODO: increase minimum radius for attack trigger
    
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