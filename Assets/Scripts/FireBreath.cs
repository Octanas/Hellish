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

        available = true;
        breathingFire = false;
    }

    private void FireBreathInput(InputAction.CallbackContext context)
    {
        // If the ability is available, trigger it
        if (available)
            animator.SetTrigger(PlayerMovement.AnimatorParameters.BreatheFire);
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
