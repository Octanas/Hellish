using UnityEngine;

public class SwordDamageCollider : DamageCollider
{
    [FMODUnity.EventRef] [SerializeField] private string heavyHitSoundEvent;

    [HideInInspector]
    public bool heavySound = false;

    protected override void PlayHitSound()
    {
        if (string.IsNullOrEmpty(heavyHitSoundEvent) || !heavySound)
            base.PlayHitSound();
        else
            FMODUnity.RuntimeManager.PlayOneShotAttached(heavyHitSoundEvent, gameObject);
    }
}
