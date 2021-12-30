using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Character {
    public PlayerLook playerLook;
    public PlayerMove playerMove;

    public bool canControlTerrain = true;
    public GameObject lightbulbObject;

    public Animator animator;

    public GameObject itemHolder;

    void Update() {
        if (playerLook.lockCamera) {
            return;
        }
        if (Input.GetMouseButton(1) || Input.GetMouseButton(0)) {
            Deform();
        }
        ItemSlot handSlot = GameManager.instance.playerInventory.hotbar[GameManager.instance.playerInventory.equiptIndex];
        if (Input.GetMouseButtonDown(0)) {
            if (!(handSlot.item is null)) {
                handSlot.item.OnLeftClick();
            } else {
                Punch();
            }
        }
        if (Input.GetMouseButtonDown(1)) {
            if (!(handSlot.item is null)) {
                handSlot.item.OnRightClick();
            } else {
                Secondary();
            }
        }

        for (int i = 0; i < 9; i++) {
            // 1 -> 49
            if (Input.GetKeyDown((KeyCode)(49 + i))) {
                if (i != GameManager.instance.playerInventory.equiptIndex) {
                    UnEquipt();
                    Equipt(GameManager.instance.playerInventory.SetHotbarIndex(i).item);
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha0)) {
            if (GameManager.instance.playerInventory.equiptIndex != 9) {
                UnEquipt();
                Equipt(GameManager.instance.playerInventory.SetHotbarIndex(9).item);
            }
        }
        if (Input.mouseScrollDelta.y != 0) {
            UnEquipt();
            int hotbarIndex = GameManager.instance.playerInventory.equiptIndex;
            Equipt(GameManager.instance.playerInventory.SetHotbarIndex(Utils.Mod(hotbarIndex - (int)Input.mouseScrollDelta.y, 10)).item);
        }
    }

    public void Deform() {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hitInfo, WorldSettings.ReachDistance, 1 << LayerMask.NameToLayer("Ground"))) {
            if (canControlTerrain) {
                GameManager.instance.voxelDataController.Deform(hitInfo.point, (Input.GetMouseButton(0) ? -1 : 1) * WorldSettings.VoxelModifier * Time.deltaTime);
            }
            StaticEntityManager.Deform(hitInfo.point);
        }
    }

    private void Punch() {
        PlayAnimation("punch");
    }
    private void Secondary() {
        PlayAnimation("lol");
    }

    public void PlayAnimation(string animation) {
        animator.Play(animation);
    }

    public void Equipt(Item item) {
        if (item is null)
            return;
        GameObject itemObject = Instantiate(item.itemObject.prefab, Vector3.zero, Quaternion.identity);
        itemObject.transform.parent = itemHolder.transform;
        itemObject.transform.localPosition = item.itemObject.EquiptPosition;
        itemObject.transform.localRotation = Quaternion.Euler(item.itemObject.EquiptRotation);
        SetLayer(itemObject, LayerMask.NameToLayer("Item"));
        PlayAnimation(item.itemObject.idleAnimation);
    }

    public void UnEquipt() {
        if (itemHolder.transform.childCount > 0)
            Destroy(itemHolder.transform.GetChild(0).gameObject);
        PlayAnimation("idle");
    }

    public void LockCamera(bool isLock) {
        playerLook.LockCamera(isLock);
    }

    private void SetLayer(GameObject obj, int layer) {
        if (obj.transform.childCount == 0) {
            obj.layer = layer;
            return;
        }
        for (int i = 0; i < obj.transform.childCount; i++) {
            SetLayer(obj.transform.GetChild(i).gameObject, layer);
        }
    }

    public override Vector3 FaceDirection() {
        return Camera.main.transform.forward;
    }

    public override Vector3 RaycastOrigin() {
        return Camera.main.transform.position;
    }
}
