using UnityEngine;

public class PlayerAttackState : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerAttack playerAttackScript = animator.gameObject.GetComponent<PlayerAttack>();

        if (playerAttackScript != null)
        {
            playerAttackScript.StopMovement();
            playerAttackScript.DeactivateHeavySound();

            foreach (DamageCollider collider in playerAttackScript.damageColliders)
            {
                if (collider)
                    collider.disableDamageCollider();
            }
        }

    }
}
