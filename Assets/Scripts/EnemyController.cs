using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterStats))]
public class EnemyController : MonoBehaviour
{
    private Animator _animator;
    private Transform _target;
    protected NavMeshAgent _agent;
    private CharacterCombat _myCombat;
    private CharacterStats _targetStats;

    [Header("Enemy proprieties:")] public float chaseTargetRadius = 50;
    [SerializeField] protected float maxMovingVelocity = 5f;
    [SerializeField] private float stoppingDistanceRadius = 1.2f;
    [SerializeField] private float maxDetectionAngle = 90f;
    [SerializeField] private float warnDistance = 10f;
    [SerializeField] private float warningCooldown = 10f;
    private float lastWarning = -10f;
    private bool foundPlayer = false;
    private bool blocked=false;
    public Transform handFireball;
    public GameObject fireball;
    [Header("Animation:")] public float animationDampTime = 0.1f;
    [Header("Rotation:")] public float quaternionInterpolationRatio = 5f;

    // NavMesh Agent and Animator: https://docs.unity3d.com/540/Documentation/Manual/nav-MixingComponents.html
    // Navigation Control: https://docs.unity3d.com/540/Documentation/Manual/nav-CouplingAnimationAndNavigation.html

    void Start()
    {
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
        if (targetDistance <= chaseTargetRadius)
        {
            if (foundPlayer)
            {
                SeeingPlayer(targetDistance);
            }
            else if (targetAngle < maxDetectionAngle) // Check if enemy can see the target
            {
                var position = transform.position;
                Vector3 raycastOrigin = new Vector3(position.x, position.y + 1.5f, position.z);
                //TODO change default to environment and obstacles layer when exists
                Physics.Raycast(raycastOrigin, targetDirection, out var hit, targetDistance + 1,
                    LayerMask.GetMask("Default", "Player"));
                if (hit.transform &&
                    hit.transform.gameObject.layer ==
                    LayerMask.NameToLayer("Player")) // if can see player without obstacles in the way
                {
                    if (!foundPlayer)
                    {
                        foundPlayer = true;
                    }

                    SeeingPlayer(targetDistance);
                }


                /*if (hit.transform)
                    Debug.Log("Hit" + LayerMask.LayerToName(hit.transform.gameObject.layer));
                else Debug.Log(("didnt hit"));*/
                Debug.DrawRay(raycastOrigin,
                    targetDirection,
                    Color.black);
            }
        }
        else if (foundPlayer)
        {
            foundPlayer = false;
            _agent.SetDestination(transform.position);
        }

        UpdateAnimatorParameters();
    }

    private void SeeingPlayer(float targetDistance)
    {
        _agent.SetDestination(_target.position);
        // Enemy reached the minimum radius
        if (targetDistance <= _agent.stoppingDistance)
        {
            FaceTheTarget();
            AttackTarget();
        }
        else
        {
            _animator.SetBool("Attack", false);
        }
    }

    private void FaceTheTarget()
    {
        if (blocked) return;
        Vector3 direction = (_target.position - transform.position).normalized;
        Quaternion newRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation =
            Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * quaternionInterpolationRatio);
    }

    protected virtual void AttackTarget()
    {
        // Check if target is still alive
        if (_targetStats.barHealth.fillAmount > 0)
        {
            WarnEnemies();
            _animator.SetBool("Attack", true);
        }
        else _animator.SetBool("Attack", false);
    }

    public void WarnEnemies() //called when hit or when it attacks
    {
        if (Time.time - lastWarning < warningCooldown) return;
        Collider[] colliders = Physics.OverlapSphere(transform.position, warnDistance, LayerMask.GetMask("Enemy"));
        foreach (var collide in colliders)
        {
            collide.GetComponent<EnemyController>().FindTarget();
        }

        lastWarning = Time.time;
    }

    private void FindTarget()
    {
        foundPlayer = true;
    }

    private void UpdateAnimatorParameters()
    {
        // Agent current speed/ agent maximum speed, will return a value between 0 and 1
        var speed = _agent.speed == 0 ? 0 : _agent.velocity.magnitude / _agent.speed;
        _animator.SetFloat("Movement", speed, animationDampTime, Time.deltaTime);
    }

    void CreateBall()
    {
        handFireball.gameObject.SetActive(true);
    }

    void ShootBall()
    {
        handFireball.gameObject.SetActive(false);
        GameObject bullet = Instantiate(fireball, handFireball.position, handFireball.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.AddForce((_target.position - handFireball.position).normalized * 10, ForceMode.Impulse);
    }

    void BlockMovement()
    {
        _agent.speed = 0;
        blocked = true;
    }

    void RegainMovement()
    {
        _agent.speed = maxMovingVelocity;
        blocked = false;
    }

    private void OnDrawGizmosSelected()
    {
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

    private void OnAnimatorMove()
    {
        /* Agent follows animation instead of Animation follows agent -> collision bugs
        //Update position to agent position
        transform.position = _agent.nextPosition;*/
    }
}