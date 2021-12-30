using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryUI : MonoBehaviour {
    private PlayerInventory playerInventory;

    public GameObject itemSlots;
    public GameObject hotbarSlots;
    public GameObject itemSlotPrefab;

    public TempItemSlotUI tempSlotUI;
    public ItemSlotUI[] containerUIs;
    public ItemSlotUI[] hotbarUIs;
    public ItemSlotUI headSlotUI;
    public ItemSlotUI bodySlotUI;
    public ItemSlotUI legSlotUI;
    public ItemSlotUI feetSlotUI;

    public void Init(PlayerInventory playerInventory) {
        this.playerInventory = playerInventory;
        this.containerUIs = new ItemSlotUI[playerInventory.capacity];
        this.hotbarUIs = new ItemSlotUI[10];
        Init(playerInventory.container, containerUIs, playerInventory.capacity, itemSlots);
        Init(playerInventory.hotbar, hotbarUIs, 10, hotbarSlots);
        Init(playerInventory.tempSlot, tempSlotUI);
        Init(playerInventory.headSlot, headSlotUI);
        Init(playerInventory.bodySlot, bodySlotUI);
        Init(playerInventory.legSlot, legSlotUI);
        Init(playerInventory.feetSlot, feetSlotUI);
    }

    private void Init(ItemSlot[] slots, ItemSlotUI[] uis, int capacity, GameObject parent) {
        for (int i = 0; i < capacity; i++) {
            GameObject itemSlotObject = Instantiate(itemSlotPrefab);
            itemSlotObject.transform.SetParent(parent.transform, false);
            uis[i] = itemSlotObject.GetComponent<ItemSlotUI>();
            Init(i, slots, uis);
        }
    }

    private void Init(int index, ItemSlot[] slots, ItemSlotUI[] uis) {
        Init(slots[index], uis[index]);
    }

    private void Init(ItemSlot slot, ItemSlotUI ui) {
        slot.ui = ui;
        ui.slot = slot;
        ui.UpdateUI();
    }

    public void UpdateUI() {
        for (int i = 0; i < playerInventory.capacity; i++) {
            UpdateUI(i);
        }
    }

    public void UpdateUI(int index) {
        containerUIs[index].UpdateUI(playerInventory.container[index]);
    }
}
