using UnityEngine;

public class PlayerStartFireBreathState : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        FMODUnity.RuntimeManager.PlayOneShotAttached("event:/Player/Attacks/fire_breath", animator.gameObject);
    }
}
