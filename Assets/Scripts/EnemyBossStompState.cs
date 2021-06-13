using UnityEngine;

public class EnemyBossStompState : StateMachineBehaviour
{
    private EnemyBossController enemyBossController;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemyBossController = animator.gameObject.GetComponent<EnemyBossController>();
        enemyBossController.InhibitMovement();
        animator.applyRootMotion = false;
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.applyRootMotion = true;
        animator.ResetTrigger("Stomp");
        enemyBossController.RegainMovement();
    }
}
