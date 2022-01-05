using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class VoxelDataController {
    public NativeArray<VoxelData> terrainData;
    private int dataEndPos;
    public NativeHashMap<int3, int> dataPos;

    public VoxelDataController() {
        this.dataEndPos = 0;
        this.terrainData = new NativeArray<VoxelData>(WorldSettings.chunkDataLength, Allocator.Persistent);
        this.dataPos = new NativeHashMap<int3, int>(2000, Allocator.Persistent);
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
        if (!dataPos.ContainsKey(chunkModified.pos))
            return;
        VoxelId modifiedVoxel = voxelId.Translate(chunkId, chunkModified);
        ModifyVoxelData(chunkModified, Utils.CoordToIndex(modifiedVoxel.id), action);
        List<ChunkId> overlappingChunks = modifiedVoxel.OverlappingChunks(chunkModified);
        foreach (ChunkId overlappingChunk in overlappingChunks) {
            if (!dataPos.ContainsKey(overlappingChunk.pos))
                continue;
            ModifyVoxelData(overlappingChunk, Utils.CoordToIndex(modifiedVoxel.Translate(chunkModified, overlappingChunk).id), action);
        }
    }

    public void ModifyVoxelData(ChunkId chunkId, int index, Func<VoxelData, VoxelData> action) {
        terrainData[dataPos[chunkId.pos] + index] = action.Invoke(terrainData[dataPos[chunkId.pos] + index]);
        ChunkController.chunks[chunkId].hasChanged = true;
    }

    public List<JobData> GenerateDataForChunks(List<Chunk> chunksToProcess) {
        List<ChunkId> chunkIds = new List<ChunkId>();
        foreach (Chunk chunk in chunksToProcess) {
            AddChunk(chunk);
            chunkIds.Add(chunk.id);
            chunk.hasDataGenerated = true;
        }

        VoxelDataGeneratorJob job = new VoxelDataGeneratorJob(
            terrainData,
            new NativeArray<bool>(1, Allocator.TempJob),
            dataPos,
            new NativeArray<ChunkId>(chunkIds.ToArray(), Allocator.TempJob)
        );
        List<JobData> jobDataList = new List<JobData>();
        jobDataList.Add(new JobData(job.Schedule(chunkIds.Count, 1), () => {
            job.Dispose();
        }));
        return jobDataList;
    }

    public void AddChunk(Chunk chunk) {
        if (dataPos.ContainsKey(chunk.id.pos))
            return;
        if (dataEndPos >= terrainData.Length - 1)
            ResizeArray();
        this.dataPos[chunk.id.pos] = dataEndPos;
        this.dataEndPos += WorldSettings.chunkDataLength;
    }

    private void ResizeArray() {
        NativeArray<VoxelData> newTerrainData = new NativeArray<VoxelData>(this.terrainData.Length * 2, Allocator.Persistent);
        NativeArray<VoxelData>.Copy(this.terrainData, newTerrainData, this.terrainData.Length);
        this.terrainData.Dispose();
        this.terrainData = newTerrainData;
    }

    public List<JobData> GenerateMeshForChunks(List<Chunk> chunksToProcess, ChunkId playerChunkCoord) {
        List<JobData> jobDataList = new List<JobData>();
        foreach (Chunk chunk in chunksToProcess) {
            MarchingCubeJob job = new MarchingCubeJob() {
                dimension = WorldSettings.chunkDimension,
                voxelSize = WorldSettings.voxelSize,
                terrainData = terrainData,
                vertexData = new NativeList<VertexData>(Allocator.TempJob),
                triangles = new NativeList<ushort>(Allocator.TempJob),
                grassMesh = new NativeList<ushort>(Allocator.TempJob),
                chunkId = chunk.id,
                startIndex = dataPos[chunk.id.pos]
            };
            jobDataList.Add(new JobData(job.Schedule(), () => {
                chunk.SetMeshData(job.vertexData.AsArray(), job.triangles.AsArray(), job.grassMesh.AsArray());
                job.Dispose();

                chunk.hasChanged = false;
                chunk.hasColliderBaked = false;
            }));
        }
        return jobDataList;
    }

    public void BakeColliderForChunks(List<Chunk> chunksToProcess) {
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