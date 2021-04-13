using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    private Animator animator;
    private Transform target;
    private NavMeshAgent agent;

    public float lookRadius = 5f;
    public float animationDampTime = 0.1f;

    // NavMesh Agent and Animator: https://docs.unity3d.com/540/Documentation/Manual/nav-MixingComponents.html
    // - Agent follows animation
    // Navigation Control: https://docs.unity3d.com/540/Documentation/Manual/nav-CouplingAnimationAndNavigation.html

    void Start(){
        // Use singleton instead of inserting manually
        target = PlayerManager.instance.player.transform;

        agent = GetComponent<NavMeshAgent>();
        // Don't update the agent's position, the animation will do that
        agent.updatePosition = false;

        animator = GetComponent<Animator>();
    }


    void Update(){
        // Activate NavMeshAgent target
        float distance = Vector3.Distance(target.position, transform.position);
        if(distance <= lookRadius)
            agent.SetDestination(target.position);
        
        // Update animation parameters
        // Agent current spped/ agent maximum speed, that will return a value between 0 and 1
        float speed = agent.velocity.magnitude/ agent.speed;
        animator.SetFloat("Movement", speed, animationDampTime, Time.deltaTime);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);//radius
        if(agent)Gizmos.DrawWireSphere(agent.destination, 1);//target
    }

    void OnAnimatorMove (){
        // Update position to agent position
        transform.position = agent.nextPosition;
    }
}
