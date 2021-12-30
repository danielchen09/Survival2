using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ChunkController : MonoBehaviour {
    public static Dictionary<ChunkId, Chunk> chunks;
    public GameObject chunkPrefab;
    public Transform chunkHolder;

    public StaticEntityManager staticEntitySpawner;

    public Transform player;

    public WorkState workState;
    private Queue<ThreadedTask> pendingTasks;
    private HashSet<ThreadedTask> pendingTasksIndex;
    private List<ThreadedTask> runningThreads;
    private int maxRunningThreads;

    private void Awake() {
        chunks = new Dictionary<ChunkId, Chunk>();
        workState = new WorkState();
        pendingTasks = new Queue<ThreadedTask>();
        pendingTasksIndex = new HashSet<ThreadedTask>();
        runningThreads = new List<ThreadedTask>();
        staticEntitySpawner = GetComponent<StaticEntityManager>();
        maxRunningThreads = SystemInfo.processorCount - 4;
    }

    private void Update() {
        UnloadChunk();
        LoadExistingChunks();
        InitNewChunks();
        GetChunksToProcess();
        ProcessChunks();
    }

    private void GetChunksToProcess() {
        foreach (Chunk chunk in chunks.Values) {
            ThreadedTask task;
            if (!chunk.hasDataGenerated)
                task = new VoxelDataGenerationTask(chunk);
            else if (chunk.hasDataGenerated && (chunk.hasChanged || !chunk.hasMeshGenerated))
                task = new MarchingCubesTask(GameManager.instance.voxelDataController.terrainData[chunk.id], chunk);
            else if (chunk.hasMeshGenerated && !chunk.hasColliderBaked)
                task = new BakeColliderTask(chunk);
            else continue;

            if (pendingTasksIndex.Contains(task))
                continue;
            pendingTasksIndex.Add(task);
            pendingTasks.Enqueue(task);
        }
        while (runningThreads.Count < maxRunningThreads && pendingTasks.Count > 0) {
            ThreadedTask task = pendingTasks.Dequeue();
            pendingTasksIndex.Remove(task);
            runningThreads.Add(task.Start());
        }
        Debug.Log(pendingTasks.Count);
    }

    private void ProcessChunks() {
        for (int i = runningThreads.Count - 1; i >= 0; i--) {
            ThreadedTask task = runningThreads[i];
            if (!task.IsDone)
                continue;
            task.OnFinish();
            runningThreads.RemoveAt(i);
        }
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
                        chunkGameObject.SetActive(false);
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