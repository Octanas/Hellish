using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handle player's Fire Breath ability.
/// </summary>
public class FireBreath : MonoBehaviour
{
    private PlayerControls _controls;
    private Animator animator;
    private PlayerAttack playerAttack;
    private PlayerStats playerStats;
    public float manaCost = 800;

    /// <summary>
    /// Fire particles prefab.
    /// </summary>
    public GameObject fireParticles;

    /// <summary>
    /// Point from which to instantiante fire particles.
    /// </summary>
    public Transform fireBreathOrigin;

    /// <summary>
    /// Fire rate particles per second.
    /// </summary>
    public float fireRate = 1;

    private float timePassed;

    /// <summary>
    /// If the ability can be used.
    /// </summary>
    private bool available;

    /// <summary>
    /// If the ability is currently being used.
    /// </summary>
    private bool breathingFire;

    /// <summary>
    /// Seconds the ability takes to finish.
    /// </summary>
    public float duration = 3f;

    /// <summary>
    /// Seconds between use of ability.
    /// </summary>
    public float colldown = 5f;

    private void Awake()
    {
        _controls = new PlayerControls();
        _controls.Gameplay.FireBreath.performed += FireBreathInput;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        playerStats = GetComponent<PlayerStats>();

        available = true;
        breathingFire = false;

        timePassed = fireRate;
    }

    private void Update()
    {
        // If currently breathing fire, spawn particle according to fire rate.
        if (breathingFire)
        {
            timePassed += Time.deltaTime;

            if (timePassed >= fireRate)
            {
                timePassed -= fireRate;

                Instantiate(fireParticles, fireBreathOrigin.position, fireBreathOrigin.rotation);
            }
        }
    }

    private void FireBreathInput(InputAction.CallbackContext context)
    {
        // If the ability is available, trigger it
        if (available && playerStats.CheckBreath(manaCost))
        {
            animator.SetTrigger(PlayerMovement.AnimatorParameters.BreatheFire);
            playerStats.useMana(manaCost);
        }
    }

    private void OnFireBreath()
    {
        // If ability is already active, do nothing
        if (breathingFire)
            return;

        // Reset trigger so ability is not triggered more than once
        animator.ResetTrigger(PlayerMovement.AnimatorParameters.BreatheFire);

        breathingFire = true;
        available = false;
        // Start timing coroutine
        StartCoroutine(CountTime());
    }

    private IEnumerator CountTime()
    {
        // After duration of ability has passed, disable it
        yield return new WaitForSeconds(duration);
        animator.SetTrigger(PlayerMovement.AnimatorParameters.StopBreatheFire);
        breathingFire = false;

        // Only after cooldown has passed will it be available to use again
        yield return new WaitForSeconds(colldown);
        available = true;
    }

    private void OnEnable()
    {
        _controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        _controls.Gameplay.Disable();
    }
}