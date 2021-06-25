using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ChunkController : MonoBehaviour {
    public const int MAX_LOD = 4; // 1 cell / 16x16 chunk(0)
    public static Dictionary<ChunkId, Chunk>[] chunks = new Dictionary<ChunkId, Chunk>[MAX_LOD];
    public static OctTree octTree = new OctTree();
    public GameObject chunkPrefab;

    public Transform player;
    public ChunkId playerChunkPos;

    private void Awake() {
        for (int i = 0; i < MAX_LOD; i++) {
            chunks[i] = new Dictionary<ChunkId, Chunk>();
        }
    }

    private void Update() {
        if (!ChunkId.FromWorldCoord(player.position).Equals(playerChunkPos)) {
            UnloadChunks();
            BuildOctTree();
            InitNewChunks();
            ProcessChunks();
            playerChunkPos = ChunkId.FromWorldCoord(player.position);
        }
    }

    private void UnloadChunks() {
        foreach (OctTreeNode node in octTree.activeNodes) {
            chunks[node.lod][node.id].Unload();
        }
    }

    private void BuildOctTree() {
        octTree = new OctTree(MAX_LOD, player.transform.position);
    }

    private void InitNewChunks() {
        foreach (OctTreeNode node in octTree.activeNodes) {
            if (chunks[node.lod].ContainsKey(node.id)) {
                chunks[node.lod][node.id].Load();
            } else {
                GameObject chunkGameObject = Instantiate(chunkPrefab, node.id.ToWorldCoord(), Quaternion.identity);
                chunks[node.lod].Add(node.id, new Chunk(node.id, chunkGameObject, node.lod));
            }
        }
    }

    private void ProcessChunks() {
        List<Chunk> chunksToProcess = new List<Chunk>();
        foreach (OctTreeNode node in octTree.activeNodes) {
            chunksToProcess.Add(chunks[node.lod][node.id]);
        }
        VoxelDataController.GenerateDataForChunks(chunksToProcess);
        VoxelDataController.GenerateMeshForChunks(chunksToProcess);
    }

    private void OnApplicationQuit() {
    }
}