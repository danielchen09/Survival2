﻿using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ChunkController : MonoBehaviour {
    public static Dictionary<ChunkId, Chunk> chunks;
    public GameObject chunkPrefab;
    public Transform chunkHolder;

    public StaticEntityManager staticEntitySpawner;

    public Transform player;

    public WorkState workState;
    private List<Chunk> chunksToProcess;
    private List<JobData> runningJobs;

    private void Awake() {
        chunks = new Dictionary<ChunkId, Chunk>();
        workState = new WorkState();
        chunksToProcess = new List<Chunk>();
        runningJobs = new List<JobData>();
        staticEntitySpawner = GetComponent<StaticEntityManager>();
    }

    private void Update() {

        foreach (JobData jobData in runningJobs) {
            jobData.Complete();
        }
        runningJobs.Clear();
        UnloadChunk();
        LoadExistingChunks();
        InitNewChunks();
        GetChunksToProcess();
        ProcessChunks();
    }

    private void LateUpdate() {
    }

    private void GetChunksToProcess() {
        switch (workState.workState) {
            case WorkState.FILL: 
                foreach (Chunk chunk in chunks.Values) {
                    if (!chunk.hasDataGenerated)
                        chunksToProcess.Add(chunk);
                }
                break;
            case WorkState.MESH:
                foreach (Chunk chunk in chunks.Values) {
                    if (chunk.hasDataGenerated && (chunk.hasChanged || !chunk.hasMeshGenerated)) {
                        chunksToProcess.Add(chunk);
                    }
                }
                break;
            case WorkState.BAKE:
                foreach (Chunk chunk in chunks.Values) {
                    if (chunk.hasMeshGenerated && !chunk.hasColliderBaked) {
                        chunksToProcess.Add(chunk);
                    }
                }
                break;
            case WorkState.SPAWN:
                foreach (Chunk chunk in chunks.Values) {
                    if (chunk.hasSurface && !chunk.hasEntitiesSpawned) {
                        chunksToProcess.Add(chunk);
                    }
                }
                break;
        }
    }

    private void ProcessChunks() {
        switch (workState.workState) {
            case WorkState.FILL:
                runningJobs = GameManager.instance.voxelDataController.GenerateDataForChunks(chunksToProcess);
                break;
            case WorkState.MESH:
                runningJobs = GameManager.instance.voxelDataController.GenerateMeshForChunks(chunksToProcess, GetPlayerChunkCoord());
                break;
            case WorkState.BAKE:
                GameManager.instance.voxelDataController.BakeColliderForChunks(chunksToProcess);
                break;
            case WorkState.SPAWN:
                staticEntitySpawner.Spawn(chunksToProcess);
                break;
        }
        workState.NextInLoop();
        chunksToProcess.Clear();
    }

    private void LoadExistingChunks() {
        ChunkId playerChunkCoord = GetPlayerChunkCoord();
        foreach (ChunkId chunk in chunks.Keys) {
            if (Utils.Magnitude(chunk.pos - playerChunkCoord.pos) <= WorldSettings.RenderDistanceInChunks) {
                chunks[chunk].Load();
            }
        }
    }

    private void InitNewChunks() {
        ChunkId playerChunkCoord = GetPlayerChunkCoord();
        int renderDst = WorldSettings.RenderDistanceInChunks;
        for (int x = -renderDst; x <= renderDst; x++) {
            for (int y = -renderDst; y <= renderDst; y++) {
                for (int z = -renderDst; z <= renderDst; z++) {
                    ChunkId newChunk = new ChunkId(new int3(x, y, z) + playerChunkCoord.pos);
                    if (newChunk[1] < -WorldSettings.WorldDepthInChunks || newChunk[1] > WorldSettings.WorldHeightInChunks)
                        continue;
                    if (!chunks.ContainsKey(newChunk) && Utils.Magnitude(newChunk.pos - playerChunkCoord.pos) <= WorldSettings.RenderDistanceInChunks) {
                        GameObject chunkGameObject = Instantiate(chunkPrefab, newChunk.ToWorldCoord(), Quaternion.identity, chunkHolder);
                        Chunk chunk = new Chunk(newChunk, chunkGameObject);
                        chunks.Add(newChunk, chunk);
                    }
                }
            }
        }
    }

    private void UnloadChunk() {
        ChunkId playerChunkCoord = GetPlayerChunkCoord();
        foreach (ChunkId chunk in chunks.Keys) {
            if (Utils.Magnitude(chunk.pos - playerChunkCoord.pos) > WorldSettings.RenderDistanceInChunks) {
                chunks[chunk].Unload();
            }
        }
    }

    private ChunkId GetPlayerChunkCoord() {
        return new ChunkId((int3)math.floor((float3)player.position / ((float3)WorldSettings.chunkDimension * WorldSettings.voxelSize)));
    }
}