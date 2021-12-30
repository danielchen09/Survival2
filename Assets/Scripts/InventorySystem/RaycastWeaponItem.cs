using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastWeaponItem : WeaponItem {
    protected RaycastWeapon raycastWeaponObject;

    public GameObject raycastSpawn;

    protected new void Awake() {
        base.Awake();
        raycastWeaponObject = (RaycastWeapon)weaponObject;
    }

    public override void Attack(Character attacker) {
        base.Attack(attacker);

        if (Physics.Raycast(attacker.RaycastOrigin(), attacker.FaceDirection(), out RaycastHit hitInfo, raycastWeaponObject.range)) {
            Instantiate(raycastSpawn, hitInfo.point, attacker.transform.rotation);
            Collider[] hits = Physics.OverlapSphere(hitInfo.point, raycastWeaponObject.hitRadius);
            DealDamage(hits, attacker.gameObject);
        }
    }
}
