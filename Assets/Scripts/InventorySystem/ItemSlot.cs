using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class ItemSlot {
    public Inventory inventory;
    public ItemSlotUI ui;
    public Item item;
    public int count;

    public int containerId;
    public int index;

    public ItemType[] acceptType;
    public Action<ItemSlot> OnAddItem;
    public Action<ItemSlot> OnRemoveItem;

    public ItemSlot(Inventory inventory, int containerId, int index, ItemType[] acceptType = null, Action<ItemSlot> OnAddItem = null, Action<ItemSlot> OnRemoveItem = null) {
        this.inventory = inventory;
        this.ui = null;
        this.item = null;
        this.count = 0;

        this.containerId = containerId;
        this.index = index;

        this.acceptType = acceptType;
        if (acceptType is null)
            this.acceptType = new ItemType[] { ItemType.EVERYTHING };

        this.OnAddItem = OnAddItem;
        this.OnRemoveItem = OnRemoveItem;
    }

    public ItemSlot(ItemSlot slot) {
        this.inventory = slot.inventory;
        this.ui = slot.ui;
        this.item = slot.item;
        this.count = slot.count;
    }

    public bool AcceptItemType(Item item) {
        foreach (ItemType type in acceptType) {
            if (type == ItemType.EVERYTHING)
                return true;
            if (type == item.itemObject.itemType)
                return true;
        }
        return false;
    }

    public void Equipt(ItemSlot slot) {
        Equipt(slot.item, slot.count);
    }

    public void Equipt(Item item, int count) {
        this.item = item;
        this.count = count;
        UpdateUI();
        if (item is null) {
            if (OnRemoveItem != null)
                OnRemoveItem.Invoke(this);
        } else {
            if (OnAddItem != null)
                OnAddItem.Invoke(this);
        }
    }

    public void SwapWithTempSlot() {
        inventory.SwapWithTempSlot(this);
    }

    public void UpdateUI() {
        ui.UpdateUI(this);
    }
}
