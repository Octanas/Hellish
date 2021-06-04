using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    // This WeaponManger is right on top of our model where the animator is
    // In Unity, if any methods(scripts) sit on the model you can use any public methods
    // from those scripts in the Animation Events

    public List<DamageCollider> listDamageColliders;
    
    public void openDamageCollider(int index)
    {
        if(listDamageColliders.Capacity != 0)
            listDamageColliders[index].enableDamageCollider();    
    }
    
    public void closeDamageCollider(int index)
    {
        if(listDamageColliders.Capacity != 0)
            listDamageColliders[index].disableDamageCollider();
    }

}
