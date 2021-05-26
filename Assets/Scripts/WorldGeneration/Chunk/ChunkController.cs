﻿using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ChunkController : MonoBehaviour {
    public static Dictionary<ChunkId, Chunk> chunks;
    public GameObject chunkPrefab;

    public Transform player;

    public WorkState workState;

    private const int MAX_PROCESS_CHUNKS = 20;
    private List<Chunk> chunksToProcess;

    private void Awake() {
        chunks = new Dictionary<ChunkId, Chunk>();
        workState = new WorkState();
        chunksToProcess = new List<Chunk>();
    }

    private void Update() {
        UnloadChunk();
        LoadExistingChunks();
        InitNewChunks();
        GetChunksToProcess();
        ProcessChunks();
    }

    private void GetChunksToProcess() {
        int count = 0;
        foreach (Chunk chunk in chunks.Values) {
            if (chunk.workState.workState == workState.workState) {
                if (count++ >= MAX_PROCESS_CHUNKS)
                    return;
                chunksToProcess.Add(chunk);
            }
        }
    }

    private void ProcessChunks() {
        if (chunksToProcess.Count == 0) {
            workState.NextInLoop();
            return;
        }
        switch (workState.workState) {
            case WorkState.FILL:
                VoxelDataController.GenerateDataForChunks(chunksToProcess);
                break;
            case WorkState.MESH:
                VoxelDataController.GenerateMeshForChunks(chunksToProcess);
                break;
        }
        chunksToProcess.Clear();
    }

    private void LoadExistingChunks() {
        ChunkId playerChunkCoord = GetPlayerChunkCoord();
        foreach (ChunkId chunk in chunks.Keys) {
            if (Utils.Magnitude(chunk.id - playerChunkCoord.id) <= WorldSettings.RenderDistanceInChunks) {
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
                    ChunkId newChunk = new ChunkId(new int3(x, y, z) + playerChunkCoord.id);
                    if (newChunk[1] < -WorldSettings.WorldDepthInChunks || newChunk[1] > WorldSettings.WorldHeightInChunks)
                        continue;
                    if (!chunks.ContainsKey(newChunk) && Utils.Magnitude(newChunk.id - playerChunkCoord.id) <= WorldSettings.RenderDistanceInChunks) {
                        GameObject chunkGameObject = Instantiate(chunkPrefab, newChunk.ToWorldCoord(), Quaternion.identity);
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
            if (Utils.Magnitude(chunk.id - playerChunkCoord.id) > WorldSettings.RenderDistanceInChunks) {
                chunks[chunk].Unload();
            }
        }
    }

    private ChunkId GetPlayerChunkCoord() {
        return new ChunkId((int3)math.floor((float3)player.position / ((float3)WorldSettings.chunkDimension * WorldSettings.voxelSize)));
    }
}