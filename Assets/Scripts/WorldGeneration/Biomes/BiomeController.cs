using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BiomeController {
    public static BiomeType GetBiome(float2 pos) {
        float energy = NoiseGenerator.GeneratePositive(pos, new NoiseSettings() {
            octaves = 2,
            frequency = 0.001f,
            lacunarity = 2f,
            persistence = 0.5f
        });
        energy = math.saturate(energy * energy);
        float temperature = NoiseGenerator.GeneratePositive(pos, new NoiseSettings() {
            octaves = 1,
            frequency = 0.003f,
            lacunarity = 1f,
            persistence = 1f
        });

        if (temperature < 0.25) {
            if (energy < 0.5)
                return BiomeType.GRASS;
            return BiomeType.SNOW;
        } else if (temperature < 0.5) {
            if (energy < 0.1)
                return BiomeType.DIRT;
            else if (energy < 0.6)
                return BiomeType.GRASS;
            else if (energy < 0.75)
                return BiomeType.PINK;
            else
                return BiomeType.BLUE;
        } else if (temperature < 0.75) {
            if (energy < 0.25)
                return BiomeType.ORANGE;
            else if (energy < 0.75)
                return BiomeType.GRASS;
            else
                return BiomeType.PURPLE;
        } else {
            if (energy < 0.25)
                return BiomeType.BLACK;
            else if (energy < 0.75)
                return BiomeType.DESERT;
            else
                return BiomeType.RED;
        }
    }

    public static Material GetMaterialForBiome(float2 terrainCoord) {
        Material material = MaterialController.grassType;
        switch (BiomeController.GetBiome(terrainCoord)) {
            case BiomeType.BLACK:
                material = MaterialController.blackType;
                break;
            case BiomeType.BLUE:
                material = MaterialController.blueType;
                break;
            case BiomeType.DESERT:
                material = MaterialController.sandType;
                break;
            case BiomeType.DIRT:
                material = MaterialController.dirtType;
                break;
            case BiomeType.GRASS:
                material = MaterialController.grassType;
                break;
            case BiomeType.ICE:
                material = MaterialController.iceType;
                break;
            case BiomeType.ORANGE:
                material = MaterialController.orangeType;
                break;
            case BiomeType.PINK:
                material = MaterialController.pinkType;
                break;
            case BiomeType.PURPLE:
                material = MaterialController.purpleType;
                break;
            case BiomeType.RED:
                material = MaterialController.redType;
                break;
            case BiomeType.SNOW:
                material = MaterialController.snowType;
                break;
        }

        float noise = NoiseGenerator.Generate(terrainCoord, new NoiseSettings() {
            octaves = 1,
            frequency = 0.5f,
            lacunarity = 1f,
            persistence = 1f
        });
        if (!material.canSpawnGrass) {
            int digit = (int)(noise * 100) % 3;
            float r = noise * 0.1f;
            switch (digit) {
                case 0:
                    material.color = new Color(math.saturate(material.color.r + r), material.color.g, material.color.b);
                    break;
                case 1:
                    material.color = new Color(material.color.r, math.saturate(material.color.g + r), material.color.b);
                    break;
                case 2:
                    material.color = new Color(material.color.r, material.color.g, math.saturate(material.color.b + r));
                    break;
            }
        }
        return material;
    }
}
