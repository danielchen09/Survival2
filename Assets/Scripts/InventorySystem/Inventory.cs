using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Inventory : MonoBehaviour {
    public ItemSlot tempSlot;
    public abstract int AddItem(Item item, int count);

    public void SwapWithTempSlot(ItemSlot slot) {
        if (!(tempSlot.item is null) && !slot.AcceptItemType(tempSlot.item)) {
            return;
        }
        ItemSlot tmp = new ItemSlot(slot);
        slot.Equipt(tempSlot);
        tempSlot.Equipt(tmp);
    }
}
