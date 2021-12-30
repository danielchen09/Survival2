using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory System/Items/Weapon")]
public class Weapon : ItemObject {
    public float baseDamage = 0;
    public float range;
    public string primaryAttackAnimation;
    public string secondaryAttackAnimation;

    private void Awake() {
        itemType = ItemType.WEAPON;
    }

    public override void OnLeftClick() {
        GameManager.instance.playerController.PlayAnimation(primaryAttackAnimation);
    }

    public override void OnRightClick() {
        GameManager.instance.playerController.PlayAnimation(secondaryAttackAnimation);
    }
}
