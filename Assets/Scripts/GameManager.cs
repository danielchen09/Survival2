using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager instance;

    public GameObject player;
    public PlayerController playerController;
    public UIController uiController;
    public PlayerInventory playerInventory;

    public VoxelDataController voxelDataController;

    private void Awake() {
        instance = GetComponent<GameManager>();
        voxelDataController = new VoxelDataController();
    }
    
}
