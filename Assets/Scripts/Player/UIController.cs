using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour {
    public GameObject hudUI;
    public HUDController hudController;
    public GameObject inventoryUI;
    public GameObject damagePopup;

    public Vector2 damagePopupRange;

    private void Awake() {
        hudController = hudUI.GetComponent<HUDController>();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            inventoryUI.SetActive(!inventoryUI.activeInHierarchy);
            GameManager.instance.playerController.LockCamera(inventoryUI.activeInHierarchy);
        }
    }

    public void SpawnDamagePopup(Vector3 position, float damage, bool isCrit) {
        Vector3 randomizedPosition = position + new Vector3(Random.Range(0f, 1f) * damagePopupRange.x, Random.Range(0f, 1f) * damagePopupRange.y, 0);
        GameObject damagePopupObject = Instantiate(damagePopup, randomizedPosition, Quaternion.identity);
        damagePopupObject.transform.LookAt(GameManager.instance.player.transform);
        damagePopupObject.GetComponent<DamagePopup>().Init(damage, isCrit);
    }
}
