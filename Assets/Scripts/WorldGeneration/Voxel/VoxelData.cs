using UnityEngine;

public struct VoxelData {
    public float density;
    public Color32 color;

    public VoxelData(float density, Color32 color) {
        this.density = density;
        this.color = color;
    }
}