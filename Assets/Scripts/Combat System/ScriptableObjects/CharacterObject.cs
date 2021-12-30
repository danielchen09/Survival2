using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Combat System/Character")]
public class CharacterObject : ScriptableObject {
    public float baseHealth;
    public float baseAttackSpeed;
    public float baseAttackDamage;
    public float baseSpeed;
    [Range(0f, 1f)]
    public float baseCritChance;
}
