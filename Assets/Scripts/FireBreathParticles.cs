using UnityEngine;

/// <summary>
/// Handle fire breath particles.
/// </summary>
public class FireBreathParticles : MonoBehaviour
{
    /// <summary>
    /// Speed at which the particles move.
    /// </summary>
    public float particleSpeed = 5f;

    private void FixedUpdate()
    {
        transform.position += transform.forward * particleSpeed * Time.fixedDeltaTime;
    }
}
