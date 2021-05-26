using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1)) {
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hitInfo, WorldSettings.ReachDistance, 1 << LayerMask.NameToLayer("Ground"))) {
                VoxelDataController.ModifyVoxelData(hitInfo.point, (Input.GetMouseButton(0) ? -1 : 1) * WorldSettings.VoxelModifier);
            }
        }
    }
}
