using UnityEngine;
using UnityEngine.UI;

public class EnemyBossController : EnemyController
{
    // STATE MANAGEMENT

    /// <summary>
    /// Enemy Boss states (in accordance to the animations in the animator).
    /// Each constant contains the hash value that represents each state.
    /// These values should be compared with <see cref="AnimatorStateInfo.fullPathHash"/>.
    /// </summary>
    public static class State
    {
        public static readonly int IdleTaunt = Animator.StringToHash("Base Layer.EnemyBossIdleTaunt");
    }

    /// <summary>
    /// Animator parameters (in accordance to the parameters set in the animator).
    /// Each constant contains the string value of each parameter.
    /// It does not contain information about the type of each parameter.
    /// </summary>
    public static class AnimatorParameters
    {
        public static readonly string Taunt = "Taunt";
        public static readonly string Stomp = "Stomp";
        public static readonly string MagicAttack = "MagicAttack";
    }

    private FMOD.Studio.EventInstance taunt;

    [Header("Taunt")] [SerializeField] [Range(0f, 1f)]
    private float tauntChances = 0.25f;

    [SerializeField] private float tauntInterval = 10f;
    private float tauntTimePassed = 0f;

    [Header("Stomp")] [SerializeField] [Range(0f, 1f)]
    private float stompChances = 0.25f;

    [SerializeField] private float stompInterval = 10f;
    [SerializeField] private float stompRadius = 5f;
    private float stompTimePassed = 0f;

    private Slider sliderHealth;

    protected override void Start()
    {
        base.Start();

        tauntTimePassed = tauntInterval;
        stompTimePassed = stompInterval;

        taunt = FMODUnity.RuntimeManager.CreateInstance("event:/Enemy/Giant/giant_noise");
        taunt.setParameterByName("Volumee", 0.1f);
        taunt.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));

        sliderHealth = GetComponent<EnemyStats>().sliderHealth;
    }

    protected override void Update()
    {
        base.Update();

        tauntTimePassed += Time.deltaTime;
        stompTimePassed += Time.deltaTime;

        if (tauntTimePassed >= tauntInterval && Random.value <= tauntChances)
        {
            _animator.SetTrigger(AnimatorParameters.Taunt);
            tauntTimePassed = 0;
        }
    }

    protected override void SeeingPlayer(float targetDistance)
    {
        base.SeeingPlayer(targetDistance);
        sliderHealth.gameObject.SetActive(true);

        if (targetDistance <= chaseTargetRadius && _agent.pathStatus != UnityEngine.AI.NavMeshPathStatus.PathComplete)
        {
            _animator.SetTrigger(AnimatorParameters.MagicAttack);
        }
        else if (stompTimePassed >= stompInterval && targetDistance <= stompRadius && Random.value <= stompChances)
        {
            _animator.SetTrigger(AnimatorParameters.Stomp);
            stompTimePassed = 0;
        }
    }

    protected override void AttackTarget()
    {
        _agent.speed = 0;
        base.AttackTarget();
    }

    protected override void ShootBall()
    {
        handFireball.gameObject.SetActive(false);
        GameObject bullet = Instantiate(fireball, handFireball.position, handFireball.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        Vector3 distance = PlayerManager.Instance.playerCenter.position - handFireball.position;
        float heightDifference = distance.y;
        distance.y = 0;

        float horizontalTravellingSpeed = 10;
        float gravityForce = Physics.gravity.y;

        float neededTime = distance.magnitude / horizontalTravellingSpeed;

        float verticalTravellingSpeed =
            (heightDifference - 0.5f * gravityForce * Mathf.Pow(neededTime, 2)) / neededTime;

        rb.velocity = distance.normalized * horizontalTravellingSpeed + Vector3.up * verticalTravellingSpeed;
    }

    private void Stomp()
    {
        Collider[] colliders =
            Physics.OverlapSphere(transform.position, stompRadius, LayerMask.GetMask("Enemy", "Player"));

        foreach (var collide in colliders)
        {
            collide.GetComponent<CharacterStats>()?.TakeDamage(0);
        }
    }

    private void PlayAttackSound()
    {
        FMODUnity.RuntimeManager.PlayOneShotAttached("event:/Enemy/Giant/swing_giant", gameObject);
    }

    private void PlayStepSound()
    {
        FMODUnity.RuntimeManager.PlayOneShotAttached("event:/Enemy/Giant/giant_walk", gameObject);
    }

    private void PlayTauntSound()
    {
        taunt.start();
    }

    private void StopTauntSound()
    {
        taunt.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public void InhibitMovement()
    {
        _agent.speed = 0;
    }

    public void RegainMovement()
    {
        _agent.speed = maxMovingVelocity;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stompRadius);
    }

    private void OnDestroy()
    {
        taunt.release();
    }
}