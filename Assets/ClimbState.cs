using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbState : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<PlayerMovementClimb>().ChangeState(PlayerMovementClimb.State.Moving);
    }
}
