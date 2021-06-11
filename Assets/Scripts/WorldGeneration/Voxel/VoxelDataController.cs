using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class VoxelDataController {
    public static Dictionary<ChunkId, VoxelData[]> voxelData = new Dictionary<ChunkId, VoxelData[]>();
    public static Dictionary<ChunkId, VoxelData[]> heightMap = new Dictionary<ChunkId, VoxelData[]>();

    public static void Deform(float3 hitPoint, float val) {
        ChunkId chunkHit = ChunkId.FromWorldCoord(hitPoint);
        VoxelId voxelHit = VoxelId.FromWorldCoord(hitPoint, chunkHit);
        List<VoxelId> neighbors = voxelHit.GetNeighbors(WorldSettings.ModifyVoxelRadius);
        foreach (VoxelId neighbor in neighbors) {
            ModifyVoxelData(chunkHit, neighbor, (VoxelData original) => new VoxelData() {
                density = original.density + val,
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
            ChunkController.chunks[overlappingChunk].workState.Set(WorkState.MESH);
        }
        ChunkController.chunks[chunkModified].workState.Set(WorkState.MESH);
    }

    public static void ModifyVoxelData(ChunkId chunkId, int index, Func<VoxelData, VoxelData> action) {
        voxelData[chunkId][index] = action.Invoke(voxelData[chunkId][index]);
    }

    public static void GenerateDataForChunks(List<Chunk> chunksToProcess) {
        List<JobData<VoxelDataGeneratorJob>> jobDataList = new List<JobData<VoxelDataGeneratorJob>>();
        foreach (Chunk chunk in chunksToProcess) {
            VoxelDataGeneratorJob job = new VoxelDataGeneratorJob() {
                chunkId = chunk.id,
                voxelData = new NativeArray<VoxelData>(
                    WorldSettings.chunkDimension.x * WorldSettings.chunkDimension.y * WorldSettings.chunkDimension.z, Allocator.TempJob),
                heightMap = new NativeArray<VoxelData>(
                    WorldSettings.chunkDimension.x * WorldSettings.chunkDimension.z, Allocator.TempJob)
            };
            jobDataList.Add(new JobData<VoxelDataGeneratorJob>(job, job.Schedule()));
        }
        for (int i = 0; i < chunksToProcess.Count; i++) {
            Chunk chunk = chunksToProcess[i]; 
            JobData<VoxelDataGeneratorJob> jobData = jobDataList[i];

            jobData.handle.Complete();
            if (voxelData.ContainsKey(chunk.id)) {
                voxelData[chunk.id] = jobData.job.voxelData.ToArray();
                heightMap[chunk.id] = jobData.job.heightMap.ToArray();
            } else {
                voxelData.Add(chunk.id, jobData.job.voxelData.ToArray());
                heightMap.Add(chunk.id, jobData.job.heightMap.ToArray());
            }
            jobData.job.Dispose();

            chunk.workState.Next();
        }
    }

    public static void GenerateMeshForChunks(List<Chunk> chunksToProcess, ChunkId playerChunkCoord) {
        List<JobData<MarchingCubeJob>> jobDataList = new List<JobData<MarchingCubeJob>>();
        foreach (Chunk chunk in chunksToProcess) {
            MarchingCubeJob job = new MarchingCubeJob() {
                dimension = WorldSettings.chunkDimension,
                voxelSize = WorldSettings.voxelSize,
                counter = new NativeCounter(Allocator.TempJob),
                voxelData = new NativeArray<VoxelData>(voxelData[chunk.id], Allocator.TempJob),
                vertexData = new NativeArray<VertexData>(voxelData[chunk.id].Length * 15, Allocator.TempJob),
                triangles = new NativeArray<ushort>(voxelData[chunk.id].Length * 15, Allocator.TempJob),
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
            chunk.SetMeshData(jobData.job.counter.Count * 3, jobData.job.vertexData, jobData.job.triangles);
            jobData.job.Dispose();

            chunk.workState.Next();
        }
    }
}