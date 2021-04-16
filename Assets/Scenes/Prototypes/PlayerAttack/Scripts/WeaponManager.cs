using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    // This WeaponManger is right on top of our model where the animator is
    // In Unity, if any methods(scripts) sit on the model you can use any public methods
    // from those scripts in the Animation Events
    
    // Open the animations clip and add animation events when the collider should "damage" or not
    
    private DamageCollider weaponCollider;
    
    public void LoadWeapon(DamageCollider collider)
    {
        // TODO: this method should load the weapon mesh to the scene??
        weaponCollider = collider;
    }

    public void openDamageCollider()
    {
        weaponCollider.enableDamageCollider();    
    }
    
    public void closeDamageCollider()
    {
        weaponCollider.disableDamageCollider();
    }

}
