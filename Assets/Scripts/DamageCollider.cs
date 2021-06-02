using UnityEngine;

public class DamageCollider : MonoBehaviour
{
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
        //Debug.Log("(" + Time.time +  ")" +damageCollider.name + " OPEN collider.");
        damageCollider.enabled = true;
    }

    public void disableDamageCollider()
    {
        //Debug.Log("(" + Time.time +  ")" +damageCollider.name + " CLOSE collider.");
        damageCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            CharacterCombat myCombat = collision.GetComponent<CharacterCombat>();
            CharacterStats enemyStats = collision.GetComponent<CharacterStats>();
            if (myCombat != null && enemyStats != null)
            {
                myCombat.attack(enemyStats);
            }
        }
    }
}
