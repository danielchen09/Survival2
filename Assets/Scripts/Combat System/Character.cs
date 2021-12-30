using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : MonoBehaviour {
    public CharacterObject characterObject;
    public float health;
    public float critChance;

    protected virtual void Start() {
        health = characterObject.baseHealth;
        critChance = characterObject.baseCritChance;
    }

    public virtual void TakeDamage(float damage, bool isCrit) {
        health -= damage;
        if (health <= 0)
            Die();
    }

    public virtual void Die() {

    }

    public abstract Vector3 FaceDirection();
    public abstract Vector3 RaycastOrigin();
}
