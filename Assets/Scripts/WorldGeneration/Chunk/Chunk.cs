using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Chunk {
    public ChunkId id;
    public GameObject chunkGameObject;
    public MeshFilter meshFilter;
    public MeshCollider meshCollider;

    public GameObject waterChunkGameObject;
    public Mesh mesh;
    public MeshFilter waterMeshFilter;
    public MeshCollider waterMeshCollider;

    public int meshLod = -1;
    public bool hasDataGenerated = false;
    public bool hasChanged = false;
    public bool hasMeshBaked = false;
    public bool hasSurface;

    private GameObject[] treePrefabs;

    public int minScale = -1;

    public Chunk(ChunkId id, GameObject chunkGameObject, GameObject[] treePrefabs) {
        this.id = id;
        this.chunkGameObject = chunkGameObject;
        chunkGameObject.name = $"Chunk({id.id.x}, {id.id.y}, {id.id.z})";
        this.meshFilter = chunkGameObject.GetComponent<MeshFilter>();
        this.meshCollider = chunkGameObject.GetComponent<MeshCollider>();

        this.waterChunkGameObject = chunkGameObject.transform.GetChild(0).gameObject;
        this.waterMeshFilter = this.waterChunkGameObject.GetComponent<MeshFilter>();
        this.waterMeshCollider = this.waterChunkGameObject.GetComponent<MeshCollider>();

        this.treePrefabs = treePrefabs;
    }

    public void SetMeshData(int vertexCount, NativeArray<VertexData> vertices, NativeArray<ushort> triangles) {
        mesh = new Mesh();
        SubMeshDescriptor subMesh = new SubMeshDescriptor();

        mesh.SetVertexBufferParams(vertexCount, VertexData.bufferMemoryLayout);
        mesh.SetIndexBufferParams(vertexCount, IndexFormat.UInt16);

        mesh.SetVertexBufferData(vertices, 0, 0, vertexCount, 0, MeshUpdateFlags.DontValidateIndices);
        mesh.SetIndexBufferData(triangles, 0, 0, vertexCount, MeshUpdateFlags.DontValidateIndices);

        mesh.subMeshCount = 1;
        subMesh.indexCount = vertexCount;
        mesh.SetSubMesh(0, subMesh);

        mesh.RecalculateBounds();

        this.meshFilter.mesh = mesh;
        //this.meshCollider.sharedMesh = mesh;
    }

    public void SetCollider() {
        this.meshCollider.sharedMesh = mesh;
    }

    public void Spawn() {
        if (id.id.y < 0)
            return;
        float x = Random.Range(0, WorldSettings.chunkDimension.x * WorldSettings.voxelSize);
        float z = Random.Range(0, WorldSettings.chunkDimension.z * WorldSettings.voxelSize);
        float chunkHeight = WorldSettings.chunkDimension.y * WorldSettings.voxelSize;
        Vector3 rayStart = new Vector3(x, chunkHeight, z);
        if (Physics.Raycast(chunkGameObject.transform.TransformPoint(rayStart), Vector3.down, out RaycastHit hitInfo, chunkHeight, 1 << LayerMask.NameToLayer("Ground"))) {
            Vector3 spawnPoint = chunkGameObject.transform.InverseTransformDirection(hitInfo.point);
            if (spawnPoint.y < 0)
                return;
            int treeIndex = Random.Range(0, treePrefabs.Length);
            GameObject treeGameObject = Object.Instantiate(treePrefabs[treeIndex], spawnPoint, Quaternion.identity);
            treeGameObject.transform.position += Vector3.down * 0.5f;
            treeGameObject.transform.parent = chunkGameObject.transform;
            treeGameObject.transform.Rotate(0, Random.Range(0f, 360f), 0);
        }
    }

    public void Load() {
        chunkGameObject.SetActive(true);
    }

    public void Unload() {
        chunkGameObject.SetActive(false);
    }
}