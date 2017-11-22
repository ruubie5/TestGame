using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponScript : MonoBehaviour {

    public string weaponName;
    public float weaponRange;
    public float damage;
    public float cooldown;
    public float knockBack;
    public int durability;

    public bool equipped = false;
    public bool canAttack = true;

    public Animator weaponAnimation;
    public RaycastHit2D hit;

    public void justAttacked() 
    {
        if (canAttack == true)
        {
            canAttack = false;
            Invoke("resetAttack", cooldown);
        }
    }

    private void resetAttack()
    {
        transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
        canAttack = true;
    }
}
