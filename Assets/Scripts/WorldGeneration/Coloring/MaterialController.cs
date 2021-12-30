using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MaterialController {
    public static readonly Material noType = new Material() {
        type = MaterialType.NONE,
        color = new Color(255, 0, 255) / 255f,
        canSpawnGrass = false
    };
    public static readonly Material stoneType = new Material() {
        type = MaterialType.STONE,
        color = new Color(137, 137, 137) / 255f,
        canSpawnGrass = false
    };
    public static readonly Material grassType = new Material() {
        type = MaterialType.GRASS,
        color = new Color(114, 183, 65) / 255f,
        canSpawnGrass = true
    };
    public static readonly Material waterType = new Material() {
        type = MaterialType.WATER,
        color = new Color(23, 147, 146) / 255f,
        canSpawnGrass = false
    };
    public static readonly Material dirtType = new Material() {
        type = MaterialType.DIRT,
        color = new Color(89, 56, 36) / 255f,
        canSpawnGrass = false
    };
    public static readonly Material iceType = new Material() {
        type = MaterialType.ICE,
        color = new Color(0, 113, 162) / 255f,
        canSpawnGrass = false
    };
    public static readonly Material purpleType = new Material() {
        type = MaterialType.PURPLE,
        color = new Color(192, 121, 239) / 255f,
        canSpawnGrass = false
    };
    public static readonly Material pinkType = new Material() {
        type = MaterialType.PINK,
        color = new Color(255, 105, 160) / 255f,
        canSpawnGrass = true
    };
    public static readonly Material sandType = new Material() {
        type = MaterialType.SAND,
        color = new Color(222, 165, 41) / 255f,
        canSpawnGrass = false
    };
    public static readonly Material blackType = new Material() {
        type = MaterialType.BLACK,
        color = new Color(0, 0, 0),
        canSpawnGrass = false
    };
    public static readonly Material redType = new Material() {
        type = MaterialType.RED,
        color = new Color(255, 10, 0),
        canSpawnGrass = false
    };
    public static readonly Material snowType = new Material() {
        type = MaterialType.SNOW,
        color = new Color(176, 246, 251),
        canSpawnGrass = false
    };
    public static readonly Material orangeType = new Material() {
        type = MaterialType.ORANGE,
        color = new Color(241, 109, 15) / 255,
        canSpawnGrass = false
    };
    public static readonly Material blueType = new Material() {
        type = MaterialType.BLUE,
        color = new Color(18, 48, 255) / 255,
        canSpawnGrass = false
    };
}
