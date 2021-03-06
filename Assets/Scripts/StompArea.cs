using UnityEngine;

/// <summary>
/// Manages a stomp attack damage area.
/// </summary>
public class StompArea : MonoBehaviour
{
    /// <summary>
    /// Collider that will damage entities.
    /// </summary>
    private Collider damageCollider;

    public int damage = 40;

    /// <summary>
    /// Radius of the damage area.
    /// </summary>
    [Tooltip("Radius of the damage area.")]
    public float stompRange = 5;

    /// <summary>
    /// Time the stomp area stays completely expanded.
    /// </summary>
    [Tooltip("Time the stomp area stays completelly expanded.")]
    public float interval = 1.5f;
    /// <summary>
    /// Keeps track of the time the stomp area has been completely expanded.
    /// </summary>
    private float currentTimePassed = 0f;

    private float targetRange = 0f;
    private float growingVelocity;
    /// <summary>
    /// Seconds it takes for the area to expand.
    /// </summary>
    public float expandTime = 3;
    /// <summary>
    /// Seconds it takes for the area to shrink.
    /// </summary>
    public float shrinkTime = 1;

    private float speed = 0f;

    public StompAreaParticles stompAreaParticles;

    private void Awake()
    {
        damageCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        Expand();
    }

    private void Update()
    {
        Vector3 currentScale = transform.localScale;
        float currentRange = currentScale.x;

        // If current range is maximum, count time until interval to shrink
        if (Mathf.Abs(currentRange - stompRange) < 0.001)
        {
            currentTimePassed += Time.deltaTime;

            if (currentTimePassed >= interval)
            {
                // Disable damage collider and shrink area
                targetRange = 0;
                damageCollider.enabled = false;
                currentTimePassed = 0;
            }
        }

        Move();
    }

    /// <summary>
    /// Expand stomp area.
    /// </summary>
    public void Expand()
    {
        targetRange = stompRange;
        damageCollider.enabled = true;
    }

    /// <summary>
    /// Gradually change stomp area's radius.
    /// </summary>
    private void Move()
    {
        Vector3 currentScale = transform.localScale;
        float currentRange = currentScale.x;

        if (Mathf.Abs(currentRange - targetRange) >= 0.001)
        {
            // Initialize speed
            if (speed == 0)
            {
                speed = Mathf.Abs(currentRange - targetRange) / (currentRange < targetRange ? expandTime : shrinkTime);
            }

            float range = Mathf.MoveTowards(currentRange, targetRange, speed * Time.deltaTime);

            currentScale.x = range;
            currentScale.z = range;

            transform.localScale = currentScale;
        }
        else
        {
            // When it enters here, it means it reached the target value, so the speed can be nulled
            speed = 0;

            if (targetRange == 0)
                Destroy(gameObject);
            else
                stompAreaParticles.FadeOut();
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            CharacterStats enemyStats = collision.GetComponentInParent<CharacterStats>();
            if (enemyStats != null)
            {
                enemyStats.TakeDamage(damage);
                FMODUnity.RuntimeManager.PlayOneShotAttached("event:/Enemy/fire ball", gameObject);
            }
        }
    }
}
