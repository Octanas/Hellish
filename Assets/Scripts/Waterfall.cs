using UnityEngine;

public class Waterfall : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            FMODUnity.RuntimeManager.PlayOneShot("event:/Player/Water/water_step", other.transform.position);
    }
}
