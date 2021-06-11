using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class WaterUpdateJob : IJob {
    public ChunkId chunkId;
    
    public NativeArray<VoxelData> voxelData;
    public NativeArray<float> overflow;

    public void Execute() {
        for (int x = 0; x < WorldSettings.chunkDimension.x; x++) {
            for (int y = 0; y < WorldSettings.chunkDimension.y; y++) {
                for (int z = 0; z < WorldSettings.chunkDimension.z; z++) {
                }
            }
        }
    }
}
