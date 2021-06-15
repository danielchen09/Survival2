using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct VoxelDataGeneratorJob : IJob {
    public ChunkId chunkId;

    public NativeArray<VoxelData> voxelData;
    public bool hasSurface;

    public void Execute() {
        for (int x = 0; x < WorldSettings.chunkDimension.x; x++) {
            for (int z = 0; z < WorldSettings.chunkDimension.z; z++) {
                float3 chunkCoord = chunkId.ToWorldCoord();
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

                Material material = MaterialController.stoneType;
                for (int y = 0; y < WorldSettings.chunkDimension.y; y++) {
                    float height = y * WorldSettings.voxelSize + chunkCoord.y;

                    float density = (terrainHeight - height) / WorldSettings.WorldHeight;
                    if (density > 0 && density < WorldSettings.voxelSize / WorldSettings.WorldHeight * 2) {
                        // surface
                        material = MaterialController.grassType;
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
                    voxelData[Utils.CoordToIndex(new int3(x, y, z))] = new VoxelData(math.clamp(density, -1, 1), material);
                }
            }
        }
    }

    private void SetMaterial(int index, Material material) {
        voxelData[index] = new VoxelData(voxelData[index].density, material);
    }

    public void Dispose() {
        this.voxelData.Dispose();
    }
}