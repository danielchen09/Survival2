using UnityEngine;

public struct VoxelData {
    public float density;
    public Material material;
    public VoxelData(float density, Material materialType) {
        this.density = density;
        this.material = materialType;
    }
}