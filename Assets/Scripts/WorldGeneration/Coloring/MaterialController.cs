using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MaterialController {
    public static readonly Material noType = new Material() {
        type = MaterialType.NONE,
        color = new Color(255, 0, 255) / 255f,
        isTerrain = true
    };
    public static readonly Material stoneType = new Material() {
        type = MaterialType.STONE,
        color = new Color(137, 137, 137) / 255f,
        isTerrain = true
    };
    public static readonly Material grassType = new Material() {
        type = MaterialType.GRASS,
        color = new Color(71, 135, 97) / 255f,
        isTerrain = true
    };
    public static readonly Material waterType = new Material() {
        type = MaterialType.WATER,
        color = new Color(23, 147, 146) / 255f,
        isTerrain = false
    };
    public static readonly Material dirtType = new Material() {
        type = MaterialType.DIRT,
        color = new Color(89, 56, 36) / 255f,
        isTerrain = true
    };
}
