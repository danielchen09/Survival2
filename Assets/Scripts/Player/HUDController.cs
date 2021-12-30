using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUDController : MonoBehaviour {
    public TMP_Text biomeText;
    public GameObject hotbarSlots;
    public ItemSlotUI[] itemSlotUIs;

    private void Update() {
        Vector3 playerPos = GameManager.instance.player.transform.position;
        biomeText.SetText("Biome: " + BiomeController.GetBiome(new Unity.Mathematics.float2(playerPos.x, playerPos.z)));
    }

    public void Init() {
        itemSlotUIs = new ItemSlotUI[10];
        for (int i = 0; i < 10; i++) {
            GameObject itemSlotObject = Instantiate(GameManager.instance.playerInventory.inventoryUI.itemSlotPrefab);
            itemSlotObject.transform.SetParent(hotbarSlots.transform, false);
            itemSlotUIs[i] = itemSlotObject.GetComponent<ItemSlotUI>();
            itemSlotUIs[i].interactable = false;
        }
        UpdateHotbar();
    }

    public void UpdateHotbar() {
        for (int i = 0; i < 10; i++) {
            ItemSlotUI ui = GameManager.instance.playerInventory.inventoryUI.hotbarUIs[i];
            itemSlotUIs[i].image.color = ui.image.color;
            if (i == GameManager.instance.playerInventory.equiptIndex)
                itemSlotUIs[i].GetComponent<Image>().color = new Color(1, 1, 1, 1);
            else
                itemSlotUIs[i].GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
            itemSlotUIs[i].image.sprite = ui.image.sprite;
            itemSlotUIs[i].text = ui.text;
        }
    }
}
