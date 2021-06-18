using UnityEngine;

public class PlayerHitReactionState : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.applyRootMotion = true;
        animator.ResetTrigger("Hit");
    }
}
