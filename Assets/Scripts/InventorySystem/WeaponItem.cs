using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponItem : Item {
    protected Weapon weaponObject;

    protected void Awake() {
        weaponObject = (Weapon)itemObject;
    }

    public override void OnLeftClick() {
        base.OnLeftClick();

        Attack(GameManager.instance.player.GetComponent<Character>());
    }

    public virtual void Attack(Character attacker) {
    }

    public void DealDamage(Character target, bool isCrit) {
        if (target) {
            target.TakeDamage(weaponObject.baseDamage * (isCrit ? 2 : 1), isCrit);
        }
    }

    public void DealDamage(Collider[] colliders, GameObject attacker) {
        float critChance = attacker.GetComponent<Character>().critChance;

        foreach (Collider collider in colliders) {
            if (collider.gameObject == attacker)
                continue;
            DealDamage(collider.gameObject.GetComponent<Character>(), Random.Range(0f, 1f) <= critChance);
        }
    }
}
