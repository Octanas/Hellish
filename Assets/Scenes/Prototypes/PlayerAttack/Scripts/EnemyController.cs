using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterStats))]
public class EnemyController : MonoBehaviour
{
    private Animator _animator;
    private Transform _target;
    private NavMeshAgent _agent;
    private CharacterCombat _myCombat;
    private CharacterStats _targetStats;
    
    [Header("Enemy proprieties:")]
    public float chaseTargetRadius = 5f;
    public float maxMovingVelocity = 5f;
    public float stoppingDistanceRadius = 1.2f;
    public float maxDectectionAngle = 120f;
    [Header("Animation:")]
    public float animationDampTime = 0.1f;
    [Header("Rotation:")]
    public float quaternionInterpolationRatio = 5f;

    // NavMesh Agent and Animator: https://docs.unity3d.com/540/Documentation/Manual/nav-MixingComponents.html
    // Navigation Control: https://docs.unity3d.com/540/Documentation/Manual/nav-CouplingAnimationAndNavigation.html

    void Start(){
        // Use singleton instead of inserting manually
        _target = PlayerManager.Instance.player.transform;

        _agent = GetComponent<NavMeshAgent>();
        /* Agent follows animation instead of Animation follows agent -> collision bugs
        // Don't update the agent's position, the animation will do that
        _agent.updatePosition = false;*/
        _agent.speed = maxMovingVelocity;
        _agent.stoppingDistance = stoppingDistanceRadius;

        _animator = GetComponent<Animator>();
        _myCombat = GetComponent<CharacterCombat>();
        _targetStats = _target.GetComponent<CharacterStats>();
        
    }


    void Update()
    {
        Vector3 targetDirection = _target.position - transform.position;
        float targetDistance = targetDirection.magnitude;
        float targetAngle = Vector3.Angle(targetDirection, transform.forward);

        // Check if target is on radius
        if (targetDistance <= chaseTargetRadius) {
            // Check if enemy can see the target
            if (targetAngle < maxDectectionAngle ) {
                _agent.SetDestination(_target.position);

                // Enemy reached the minimum radius
                if (targetDistance <= _agent.stoppingDistance) {
                    FaceTheTarget();
                    AttackTarget();
                }
            }
        }
        UpdateAnimatorParameters();
    }

    private void FaceTheTarget()
    {
        Vector3 direction = (_target.position - transform.position).normalized;
        Quaternion newRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * quaternionInterpolationRatio);

    }
    
    private void AttackTarget()
    {
        // Check if target is still alive
        /*if(_target != null)
            _myCombat.attack(_targetStats);*/
    }
    
    private void UpdateAnimatorParameters()
    {
        // Agent current spped/ agent maximum speed, will return a value between 0 and 1
        var speed = _agent.velocity.magnitude/ _agent.speed;
        _animator.SetFloat("Movement", speed, animationDampTime, Time.deltaTime);
    }

    private void OnDrawGizmosSelected() {
        /*Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseTargetRadius);
        if(_target != null){
            Gizmos.DrawWireSphere(_agent.destination, 1);
            
            Vector3 targetDirection = _target.position - transform.position;
            float viewableAngle = Vector3.Angle(targetDirection, transform.forward);
            Gizmos.DrawRay( transform.position, targetDirection);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay( transform.position, transform.forward);
        }*/
    }

    void OnAnimatorMove (){
        /* Agent follows animation instead of Animation follows agent -> collision bugs
        //Update position to agent position
        transform.position = _agent.nextPosition;*/
    }
}
