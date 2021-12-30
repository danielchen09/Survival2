using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class VoxelDataGenerationTask : ThreadedTask {
    private VoxelData[] terrainData;
    public Chunk chunk;

    public bool hasSurface;

    public VoxelDataGenerationTask(Chunk chunk) {
        this.chunk = chunk;
        this.terrainData = new VoxelData[WorldSettings.chunkDataLength];
    }


    public override void ThreadFunction() {
        hasSurface = false;
        for (int x = 0; x < WorldSettings.chunkDimension.x; x++) {
            for (int z = 0; z < WorldSettings.chunkDimension.z; z++) {
                float3 chunkCoord = chunk.id.ToWorldCoord();
                float2 terrainCoord = new float2(x * WorldSettings.voxelSize + chunkCoord.x, z * WorldSettings.voxelSize + chunkCoord.z);

                float noise2d = NoiseGenerator.GeneratePositive(terrainCoord, new NoiseSettings() {
                    octaves = 1,
                    frequency = 0.001f,
                    lacunarity = 1f,
                    persistence = 1f
                });
                float terrainHeight = NoiseGenerator.GeneratePositive(terrainCoord, new NoiseSettings() {
                    octaves = 4,
                    frequency = noise2d * 0.01f,
                    lacunarity = 2f,
                    persistence = 0.4f
                }) * noise2d * (WorldSettings.WorldHeight - WorldSettings.voxelSize) + WorldSettings.voxelSize;
                terrainHeight = terrainHeight + 1 / (WorldSettings.WorldHeight - terrainHeight);

                Material material = MaterialController.stoneType;
                for (int y = 0; y < WorldSettings.chunkDimension.y; y++) {
                    float3 coord = new float3(x, y, z) * WorldSettings.voxelSize + chunkCoord;
                    float height = y * WorldSettings.voxelSize + chunkCoord.y;

                    float density = (terrainHeight - height) / WorldSettings.WorldHeight;
                    density += NoiseGenerator.Generate(coord, new NoiseSettings() {
                        octaves = 4,
                        frequency = 0.01f,
                        lacunarity = 2f,
                        persistence = 0.5f
                    }) * noise2d * 0.1f;

                    if (density > 0 && density < WorldSettings.voxelSize / WorldSettings.WorldHeight * 2) {
                        // surface
                        material = BiomeController.GetMaterialForBiome(terrainCoord);
                        hasSurface = true;
                    }
                    if (height > WorldSettings.WorldHeight * 0.3f + NoiseGenerator.Snoise(terrainCoord) * WorldSettings.voxelSize * 10)
                        material = MaterialController.stoneType;

                    if (height <= WorldSettings.voxelSize - WorldSettings.WorldDepth) {
                        density = 1;
                    } else if (density > 0.2f) {
                        density = NoiseGenerator.GenerateRidge(new float3(terrainCoord.x, height, terrainCoord.y), new NoiseSettings() {
                            octaves = 3,
                            frequency = 0.03f,
                            lacunarity = 2f,
                            persistence = 0.5f
                        }) + math.clamp(0.45f - math.abs(height) / WorldSettings.WorldDepth, 0.2f, 0.45f);
                    }
                    terrainData[Utils.CoordToIndex(new int3(x, y, z))] = new VoxelData(math.clamp(density, -1, 1), material);
                }
            }
        }
    }

    public override void OnFinish() {
        chunk.hasDataGenerated = true;
        GameManager.instance.voxelDataController.terrainData[chunk.id] = terrainData;
    }

    public override int GetHashCode() {
        return chunk.GetHashCode();
    }

    public override bool Equals(object obj) {
        if (obj.GetType() != this.GetType())
            return false;
        return ((VoxelDataGenerationTask)obj).chunk.Equals(chunk);
    }
}
