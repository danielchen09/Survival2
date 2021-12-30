using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class VoxelDataController {
    public Dictionary<ChunkId, VoxelData[]> terrainData;

    public VoxelDataController() {
        this.terrainData = new Dictionary<ChunkId, VoxelData[]>();
    }

    public void Deform(float3 hitPoint, float val) {
        ChunkId chunkHit = ChunkId.FromWorldCoord(hitPoint);
        VoxelId voxelHit = VoxelId.FromWorldCoord(hitPoint, chunkHit);
        List<VoxelId> neighbors = voxelHit.GetNeighbors(WorldSettings.ModifyVoxelRadius);
        foreach (VoxelId neighbor in neighbors) {
            ModifyVoxelData(chunkHit, neighbor, (VoxelData original) => new VoxelData() {
                density = Mathf.Clamp(original.density + val * (10 - 9 * Mathf.Pow(original.density, 2)), -1, 1),
                material = original.material
            });
        }
    }

    //public static bool TryGetVoxelData(ChunkId chunkId, VoxelId voxelId, out VoxelData result) {
    //    result = new VoxelData();
    //    ChunkId targetChunk = new ChunkId(chunkId.pos + voxelId.ChunkOffset());
    //    if (!voxelData.ContainsKey(targetChunk))
    //        return false;
    //    VoxelId targetVoxel = voxelId.Translate(chunkId, targetChunk);
    //    result = voxelData[targetChunk][Utils.CoordToIndex(targetVoxel.id)];
    //    return true;
    //}

    public void ModifyVoxelData(ChunkId chunkId, VoxelId voxelId, Func<VoxelData, VoxelData> action) {
        ChunkId chunkModified = new ChunkId(chunkId.pos + voxelId.ChunkOffset());
        if (!terrainData.ContainsKey(chunkModified))
            return;
        VoxelId modifiedVoxel = voxelId.Translate(chunkId, chunkModified);
        ModifyVoxelData(chunkModified, Utils.CoordToIndex(modifiedVoxel.id), action);
        List<ChunkId> overlappingChunks = modifiedVoxel.OverlappingChunks(chunkModified);
        foreach (ChunkId overlappingChunk in overlappingChunks) {
            if (!terrainData.ContainsKey(overlappingChunk))
                continue;
            ModifyVoxelData(overlappingChunk, Utils.CoordToIndex(modifiedVoxel.Translate(chunkModified, overlappingChunk).id), action);
        }
    }

    public void ModifyVoxelData(ChunkId chunkId, int index, Func<VoxelData, VoxelData> action) {
        terrainData[chunkId][index] = action.Invoke(terrainData[chunkId][index]);
        ChunkController.chunks[chunkId].hasChanged = true;
    }
}