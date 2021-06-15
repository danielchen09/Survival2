using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class VoxelDataController {
    public static Dictionary<ChunkId, VoxelData[]> voxelData = new Dictionary<ChunkId, VoxelData[]>();

    public static void Deform(float3 hitPoint, float val) {
        ChunkId chunkHit = ChunkId.FromWorldCoord(hitPoint);
        VoxelId voxelHit = VoxelId.FromWorldCoord(hitPoint, chunkHit);
        List<VoxelId> neighbors = voxelHit.GetNeighbors(WorldSettings.ModifyVoxelRadius);
        foreach (VoxelId neighbor in neighbors) {
            ModifyVoxelData(chunkHit, neighbor, (VoxelData original) => new VoxelData() {
                density = Mathf.Clamp(original.density + (val > 0 ? val / 30 : val) * (10 - 9 * Mathf.Pow(original.density, 2)), -1, 1),
                material = original.material
            });
        }
    }

    public static bool TryGetVoxelData(ChunkId chunkId, VoxelId voxelId, out VoxelData result) {
        result = new VoxelData();
        ChunkId targetChunk = new ChunkId(chunkId.id + voxelId.ChunkOffset());
        if (!voxelData.ContainsKey(targetChunk))
            return false;
        VoxelId targetVoxel = voxelId.Translate(chunkId, targetChunk);
        result = voxelData[targetChunk][Utils.CoordToIndex(targetVoxel.id)];
        return true;
    }

    public static void ModifyVoxelData(ChunkId chunkId, VoxelId voxelId, Func<VoxelData, VoxelData> action) {
        ChunkId chunkModified = new ChunkId(chunkId.id + voxelId.ChunkOffset());
        if (!voxelData.ContainsKey(chunkModified))
            return;
        VoxelId modifiedVoxel = voxelId.Translate(chunkId, chunkModified);
        ModifyVoxelData(chunkModified, Utils.CoordToIndex(modifiedVoxel.id), action);
        List<ChunkId> overlappingChunks = modifiedVoxel.OverlappingChunks(chunkModified);
        foreach (ChunkId overlappingChunk in overlappingChunks) {
            if (!voxelData.ContainsKey(overlappingChunk))
                continue;
            ModifyVoxelData(overlappingChunk, Utils.CoordToIndex(modifiedVoxel.Translate(chunkModified, overlappingChunk).id), action);
        }
    }

    public static void ModifyVoxelData(ChunkId chunkId, int index, Func<VoxelData, VoxelData> action) {
        voxelData[chunkId][index] = action.Invoke(voxelData[chunkId][index]);
        ChunkController.chunks[chunkId].hasChanged = true;
    }
    public static void SetVoxelData(ChunkId chunkId, VoxelData[] newVoxelData) {
        voxelData[chunkId] = newVoxelData;
        ChunkController.chunks[chunkId].hasChanged = true;
    }

    public static void GenerateDataForChunks(List<Chunk> chunksToProcess) {
        List<JobData<VoxelDataGeneratorJob>> jobDataList = new List<JobData<VoxelDataGeneratorJob>>();
        foreach (Chunk chunk in chunksToProcess) {
            VoxelDataGeneratorJob job = new VoxelDataGeneratorJob() {
                chunkId = chunk.id,
                voxelData = new NativeArray<VoxelData>(
                    WorldSettings.chunkDimension.x * WorldSettings.chunkDimension.y * WorldSettings.chunkDimension.z, Allocator.TempJob)
            };
            jobDataList.Add(new JobData<VoxelDataGeneratorJob>(job, job.Schedule()));
        }
        for (int i = 0; i < chunksToProcess.Count; i++) {
            Chunk chunk = chunksToProcess[i]; 
            JobData<VoxelDataGeneratorJob> jobData = jobDataList[i];

            jobData.handle.Complete();
            if (voxelData.ContainsKey(chunk.id)) {
                voxelData[chunk.id] = jobData.job.voxelData.ToArray();
            } else {
                voxelData.Add(chunk.id, jobData.job.voxelData.ToArray());
            }
            jobData.job.Dispose();

            chunk.hasDataGenerated = true;
        }
    }

    public static void GenerateMeshForChunks(List<Chunk> chunksToProcess, ChunkId playerChunkCoord) {
        List<JobData<MarchingCubeJob>> jobDataList = new List<JobData<MarchingCubeJob>>();
        foreach (Chunk chunk in chunksToProcess) {
            MarchingCubeJob job = new MarchingCubeJob() {
                dimension = WorldSettings.chunkDimension,
                voxelSize = WorldSettings.voxelSize,
                voxelData = new NativeArray<VoxelData>(voxelData[chunk.id], Allocator.TempJob),
                vertexData = new NativeList<VertexData>(Allocator.TempJob),
                triangles = new NativeList<ushort>(Allocator.TempJob),
                chunkId = chunk.id
            };
            jobDataList.Add(new JobData<MarchingCubeJob>(job, job.Schedule()));
        }
        for (int i = 0; i < chunksToProcess.Count; i++) {
            if (jobDataList[i].noop)
                continue;

            Chunk chunk = chunksToProcess[i];
            JobData<MarchingCubeJob> jobData = jobDataList[i];

            jobData.handle.Complete();
            chunk.SetMeshData(jobData.job.vertexData.Length, jobData.job.vertexData.AsArray(), jobData.job.triangles.AsArray());
            jobData.job.Dispose();

            chunk.hasChanged = false;
            chunk.hasColliderBaked = false;
        }
    }

    public static void BakeColliderForChunks(List<Chunk> chunksToProcess) {
        NativeArray<int> meshIds = new NativeArray<int>(chunksToProcess.Count, Allocator.TempJob);
        for (int i = 0; i < chunksToProcess.Count; i++) {
            meshIds[i] = chunksToProcess[i].mesh.GetInstanceID();
        }
        new BakeColliderJob() {
            meshIds = meshIds
        }.Schedule(meshIds.Length, 10).Complete();
        meshIds.Dispose();
        foreach (Chunk chunk in chunksToProcess) {
            chunk.SetCollider();
            chunk.hasColliderBaked = true;
        }
    }
}