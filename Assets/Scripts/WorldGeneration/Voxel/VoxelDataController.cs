using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class VoxelDataController {
    public static Dictionary<ChunkId, VoxelData[]> voxelData = new Dictionary<ChunkId, VoxelData[]>();

    // Deforming code
    //public static void Deform(float3 hitPoint, float val) {
    //    ChunkId chunkHit = ChunkId.FromWorldCoord(hitPoint);
    //    VoxelId voxelHit = VoxelId.FromWorldCoord(hitPoint, chunkHit);
    //    List<VoxelId> neighbors = voxelHit.GetNeighbors(WorldSettings.ModifyVoxelRadius);
    //    foreach (VoxelId neighbor in neighbors) {
    //        ModifyVoxelData(chunkHit, neighbor, (VoxelData original) => new VoxelData() {
    //            density = Mathf.Clamp(original.density + (val > 0 ? val / 30 : val) * (10 - 9 * Mathf.Pow(original.density, 2)), -1, 1),
    //            material = original.material
    //        });
    //    }
    //}

    //public static void ModifyVoxelData(ChunkId chunkId, VoxelId voxelId, Func<VoxelData, VoxelData> action) {
    //    ChunkId chunkModified = new ChunkId(chunkId.pos + voxelId.ChunkOffset());
    //    if (!voxelData.ContainsKey(chunkModified))
    //        return;
    //    VoxelId modifiedVoxel = voxelId.Translate(chunkId, chunkModified);
    //    ModifyVoxelData(chunkModified, Utils.CoordToIndex(modifiedVoxel.id), action);
    //    List<ChunkId> overlappingChunks = modifiedVoxel.OverlappingChunks(chunkModified);
    //    foreach (ChunkId overlappingChunk in overlappingChunks) {
    //        if (!voxelData.ContainsKey(overlappingChunk))
    //            continue;
    //        ModifyVoxelData(overlappingChunk, Utils.CoordToIndex(modifiedVoxel.Translate(chunkModified, overlappingChunk).id), action);
    //    }
    //}

    //public static void ModifyVoxelData(ChunkId chunkId, int index, Func<VoxelData, VoxelData> action) {
    //    voxelData[chunkId][index] = action.Invoke(voxelData[chunkId][index]);
    //    ChunkController.chunks[chunkId].hasChanged = true;
    //}

    public static void GenerateDataForChunks(List<Chunk> chunksToProcess) {
        List<JobData<VoxelDataGeneratorJob>> jobDataList = new List<JobData<VoxelDataGeneratorJob>>();
        foreach (Chunk chunk in chunksToProcess) {
            Utils.For3(1 << chunk.lod, (x, y, z) => {
                ChunkId chunkId = new ChunkId(chunk.id.pos + new int3(x, y, z));
                if (voxelData.ContainsKey(chunkId)) {
                    jobDataList.Add(JobData<VoxelDataGeneratorJob>.NO_OP);
                    return;
                }
                VoxelDataGeneratorJob job = new VoxelDataGeneratorJob() {
                    chunkId = chunkId,
                    voxelData = new NativeArray<VoxelData>(
                        WorldSettings.chunkDimension * WorldSettings.chunkDimension * WorldSettings.chunkDimension, Allocator.Persistent)
                };
                jobDataList.Add(new JobData<VoxelDataGeneratorJob>(job, job.Schedule()));
            });
            chunk.hasDataGenerated = true;
        }
        foreach (JobData<VoxelDataGeneratorJob> jobData in jobDataList) {
            if (jobData.noop)
                continue;
            jobData.handle.Complete();
            if (!voxelData.ContainsKey(jobData.job.chunkId))
                voxelData.Add(jobData.job.chunkId, jobData.job.voxelData.ToArray());
            jobData.job.Dispose();
        }
    }

    public static void GenerateMeshForChunks(List<Chunk> chunksToProcess) {
        List<JobData<MarchingCubeJob>> jobDataList = new List<JobData<MarchingCubeJob>>();
        
        foreach (Chunk chunk in chunksToProcess) {
            if (chunk.hasMeshGenerated && !chunk.hasChanged) {
                jobDataList.Add(JobData<MarchingCubeJob>.NO_OP);
                continue;
            }
            int size = 1 << chunk.lod;
            int length = voxelData[chunk.id].Length;
            NativeArray<VoxelData> voxelDataList = 
                new NativeArray<VoxelData>(size * size * size * length, Allocator.TempJob);
            Utils.For3(1 << chunk.lod, (x, y, z) => {
                int3 dataPos = new int3(x, y, z);
                NativeArray<VoxelData>.Copy(voxelData[chunk.id.Offset(dataPos)], 0, voxelDataList, Utils.CoordToIndex(dataPos, size) * length, length);
            });

            MarchingCubeJob job = new MarchingCubeJob() {
                chunkId = chunk.id,
                voxelSize = WorldSettings.voxelSize,
                lod = chunk.lod,
                voxelDataList = voxelDataList,
                vertices = new NativeList<VertexData>(Allocator.TempJob),
                triangles = new NativeList<ushort>(Allocator.TempJob),
                normalCount = new NativeList<int>(Allocator.TempJob),
                sharedData = new NativeArray<ushort>(16 * 16 * 16 * 4, Allocator.TempJob)
            };
            jobDataList.Add(new JobData<MarchingCubeJob>(job, job.Schedule()));
        }
        for (int i = 0; i < chunksToProcess.Count; i++) {
            if (jobDataList[i].noop)
                continue;

            Chunk chunk = chunksToProcess[i];
            JobData<MarchingCubeJob> jobData = jobDataList[i];

            jobData.handle.Complete();
            chunk.SetMeshData(jobData.job.vertices.Length, jobData.job.vertices.AsArray(), jobData.job.triangles.AsArray());
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