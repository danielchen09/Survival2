using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : Character {
    public Healthbar healthBar;
    public Transform raycastOrigin;

    protected override void Start() {
        base.Start();
    }

    public override void TakeDamage(float damage, bool isCrit) {
        base.TakeDamage(damage, isCrit);

        healthBar.SetHealth(health / characterObject.baseHealth);
        GameManager.instance.uiController.SpawnDamagePopup(transform.position, damage, isCrit);
    }

    public override Vector3 FaceDirection() {
        return transform.forward;
    }

    public override Vector3 RaycastOrigin() {
        if (raycastOrigin)
            return raycastOrigin.position;
        return transform.position;
    }
}
