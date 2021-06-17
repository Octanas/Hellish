using UnityEngine;

public class DamageCollider : MonoBehaviour
{
    [FMODUnity.EventRef] [SerializeField] private string soundEvent;
    private Collider damageCollider;

    private void Awake()
    {
        damageCollider = GetComponent<Collider>();
        damageCollider.gameObject.SetActive(true);
        damageCollider.isTrigger = true;
        damageCollider.enabled = false;//the collider itself can be enable and disable but the gameobject remains active
    }

    public void enableDamageCollider()
    {
        damageCollider.enabled = true;
    }

    public void disableDamageCollider()
    {
        damageCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            CharacterCombat myCombat = GetComponentInParent<CharacterCombat>();
            CharacterStats enemyStats = collision.GetComponentInParent<CharacterStats>();
            if (myCombat != null && enemyStats != null)
            {
                if (myCombat.attack(enemyStats))
                {
                    PlayHitSound();
                }
            }
        }
    }

    protected virtual void PlayHitSound()
    {
        if (string.IsNullOrEmpty(soundEvent))
            return;

        FMODUnity.RuntimeManager.PlayOneShotAttached(soundEvent, gameObject);
    }
}
