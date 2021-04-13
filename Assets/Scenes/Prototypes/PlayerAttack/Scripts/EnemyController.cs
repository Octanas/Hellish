using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    private Animator _animator;
    private Transform _target;
    private NavMeshAgent _agent;

    public float lookRadius = 5f;
    public float animationDampTime = 0.1f;

    // NavMesh Agent and Animator: https://docs.unity3d.com/540/Documentation/Manual/nav-MixingComponents.html
    // - Agent follows animation
    // Navigation Control: https://docs.unity3d.com/540/Documentation/Manual/nav-CouplingAnimationAndNavigation.html

    void Start(){
        // Use singleton instead of inserting manually
        _target = PlayerManager.Instance.player.transform;

        _agent = GetComponent<NavMeshAgent>();
        // Don't update the agent's position, the animation will do that
        _agent.updatePosition = false;//comment

        _animator = GetComponent<Animator>();
    }


    void Update(){
        // Activate NavMeshAgent target
        float distance = Vector3.Distance(_target.position, transform.position);
        if(distance <= lookRadius)
            _agent.SetDestination(_target.position);
        
        // Update animation parameters
        // Agent current spped/ agent maximum speed, that will return a value between 0 and 1
        float speed = _agent.velocity.magnitude/ _agent.speed;
        _animator.SetFloat("Movement", speed, animationDampTime, Time.deltaTime);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(transform.position, lookRadius);//radius
        if(_agent)Gizmos.DrawWireSphere(_agent.destination, 1);//target
    }

    void OnAnimatorMove (){
        //Update position to agent position
        transform.position = _agent.nextPosition;
    }
}
