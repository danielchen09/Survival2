using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public struct ChunkData {
    public NativeArray<VoxelData> voxelData;

    public ChunkData(VoxelData[] voxelData, Allocator allocator) {
        this.voxelData = new NativeArray<VoxelData>(voxelData, allocator);
    }

    public static int CoordToIndex(int3 coord) {
        return Utils.CoordToIndex(coord + 1, 19);
    }
}
