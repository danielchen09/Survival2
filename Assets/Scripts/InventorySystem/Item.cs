using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item : MonoBehaviour {
    public ItemObject itemObject;

    public virtual void OnLeftClick() {
        itemObject.OnLeftClick();
    }

    public virtual void OnRightClick() {
        itemObject.OnRightClick();
    }

    public override string ToString() {
        return $"{itemObject.itemName}";
    }
}
