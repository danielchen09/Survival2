using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ItemSlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    public ItemSlot slot;
    public Image image;
    public TMP_Text text;

    public Sprite defaultImage;

    public bool interactable = true;

    public void UpdateUI() {
        UpdateUI(slot);
    }

    public void UpdateUI(ItemSlot slot) {
        this.slot = slot;
        if (slot is null || slot.item is null) {
            text.SetText("");
            if (!(defaultImage == null)) {
                image.sprite = defaultImage;
                image.color = new Color(1, 1, 1, 0.4f);
                return;
            }
            image.color = new Color(1, 1, 1, 0);
            return;
        }
        image.color = Color.white;
        image.sprite = slot.item.itemObject.icon;
        if (slot.item.itemObject.stackSize > 1)
            text.SetText(slot.count.ToString());
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (!interactable)
            return;
        if (Input.GetKey(KeyCode.LeftShift)) {
            Debug.Log("shift click");
            return;
        }
        slot.SwapWithTempSlot();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (!interactable)
            return;
        this.GetComponent<Image>().color = new Color(1, 1, 1, 0.8f);
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (!interactable)
            return;
        this.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
    }
}
