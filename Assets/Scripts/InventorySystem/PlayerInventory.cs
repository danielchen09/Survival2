using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : Inventory {
    public int capacity = 54;
    public ItemSlot[] container;
    public ItemSlot headSlot;
    public ItemSlot bodySlot;
    public ItemSlot legSlot;
    public ItemSlot feetSlot;
    public ItemSlot[] hotbar;

    public int equiptIndex = 0;

    public PlayerInventoryUI inventoryUI;

    private void Start() {
        container = new ItemSlot[capacity];
        for (int i = 0; i < capacity; i++) {
            container[i] = new ItemSlot(this, 0, i);
        }
        hotbar = new ItemSlot[10];
        for (int i = 0; i < 10; i++) {
            hotbar[i] = new ItemSlot(this, 1, i, null,
                (itemSlot) => {
                    if (itemSlot.index == equiptIndex) {
                        GameManager.instance.playerController.Equipt(itemSlot.item);
                    }
                    GameManager.instance.uiController.hudController.UpdateHotbar();
                },
                (itemSlot) => {
                    if (itemSlot.index == equiptIndex) {
                        GameManager.instance.playerController.UnEquipt();
                    }
                    GameManager.instance.uiController.hudController.UpdateHotbar();
                }
            );
        }
        tempSlot = new ItemSlot(this, -1, 0);
        headSlot = new ItemSlot(this, 2, 0, new ItemType[] { ItemType.HEAD });
        bodySlot = new ItemSlot(this, 2, 0, new ItemType[] { ItemType.BODY });
        legSlot = new ItemSlot(this, 2, 0, new ItemType[] { ItemType.LEG });
        feetSlot = new ItemSlot(this, 2, 0, new ItemType[] { ItemType.FEET });
        inventoryUI.Init(this);
        GameManager.instance.uiController.hudController.Init();
    }

    public ItemSlot SetHotbarIndex(int index) {
        equiptIndex = index;
        GameManager.instance.uiController.hudController.UpdateHotbar();
        return hotbar[equiptIndex];
    }

    public override int AddItem(Item item, int count) {
        for (int i = 0; i < capacity; i++) {
            if (container[i].item is null)
                continue;
            if (container[i].item.itemObject == item.itemObject) {
                int amountAdded = Mathf.Min(item.itemObject.stackSize - container[i].count, count);
                count -= amountAdded;
                container[i].count += amountAdded;
                inventoryUI.UpdateUI(i);
                if (count == 0)
                    return 0;
            }
        }
        return AddToNewSlot(item, count);
    }

    private int AddToNewSlot(Item item, int count) {
        for (int i = 0; i < capacity; i++) {
            if (container[i].item is null) {
                container[i].item = item;
                container[i].count = count;
                inventoryUI.UpdateUI(i);
                return 0;
            }
        }
        return count;
    }
}
