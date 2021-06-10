using UnityEngine;

public class EnemyBossAttackState : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<EnemyBossController>().RegainMovement();
    }
}
