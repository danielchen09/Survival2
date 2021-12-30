using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public abstract class ItemObject : ScriptableObject {
    public ItemType itemType;

    public GameObject prefab;
    public Sprite icon;
    public string itemName;
    public string description;
    public int stackSize;

    public Vector3 EquiptPosition = Vector3.zero;
    public Vector3 EquiptRotation = Vector3.zero;
    public float EquiptScale = 1;

    public string idleAnimation;

    public virtual void OnLeftClick() {

    }

    public virtual void OnRightClick() {

    }

    public virtual void OnUse() {

    }
}
