using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeaponItem : WeaponItem {
    public override void Attack(Character attacker) {
        base.Attack(attacker);

        float radius = weaponObject.range / 2;
        Collider[] hits = Physics.OverlapSphere(attacker.RaycastOrigin() + attacker.FaceDirection() * radius, radius);
        DealDamage(hits, attacker.gameObject);
    }
}
