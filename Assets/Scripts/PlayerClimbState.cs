using UnityEngine;

public class PlayerClimbState : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        FMODUnity.RuntimeManager.PlayOneShotAttached("event:/Player/climb", animator.gameObject);
    }
}
