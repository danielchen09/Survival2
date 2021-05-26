using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct VoxelDataGeneratorJob : IJob {
    public ChunkId chunk;

    public NativeArray<VoxelData> voxelData;

    public void Execute() {
        for (int x = 0; x < WorldSettings.chunkDimension.x; x++) {
            for (int z = 0; z < WorldSettings.chunkDimension.z; z++) {
                float3 chunkCoord = chunk.ToWorldCoord();
                float2 terrainCoord = new float2(x * WorldSettings.voxelSize + chunkCoord.x, z * WorldSettings.voxelSize + chunkCoord.z);

                float terrainHeight = NoiseGenerator.Generate(terrainCoord, new NoiseSettings() {
                    octaves = 5,
                    frequency = 0.005f,
                    lacunarity = 2f,
                    persistence = 0.6f
                }) * WorldSettings.WorldHeight;

                float highestY = -WorldSettings.WorldDepth;
                for (int y = 0; y < WorldSettings.chunkDimension.y; y++) {
                    float height = y * WorldSettings.voxelSize + chunkCoord.y;

                    float density = math.clamp((terrainHeight - height) / WorldSettings.WorldHeight, 0f, 1);
                    if (height <= WorldSettings.voxelSize - WorldSettings.WorldDepth) {
                        density = 1;
                    } else if (height < WorldSettings.voxelSize) {
                        density = NoiseGenerator.Generate(new float3(terrainCoord.x, height, terrainCoord.y), new NoiseSettings() {
                            octaves = 3,
                            frequency = 0.05f,
                            lacunarity = 2f,
                            persistence = 0.3f
                        }) + 0.3f;
                    }
                    if (density < WorldSettings.isoLevel)
                        highestY = height;
                    voxelData[Utils.CoordToIndex(new int3(x, y, z))] = new VoxelData(density, Color.white);
                }
                for (int y = 0; y < WorldSettings.chunkDimension.y; y++) {
                    float height = y * WorldSettings.voxelSize + chunkCoord.y;
                    if (height < highestY) {
                        VoxelData data = voxelData[Utils.CoordToIndex(new int3(x, y, z))];
                        voxelData[Utils.CoordToIndex(new int3(x, y, z))] = new VoxelData(data.density, new Color(172, 176, 193) / 255f);
                    }
                }
            }
        }
    }

    public void Dispose() {
        this.voxelData.Dispose();
    }
}