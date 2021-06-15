using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Chunk {
    public ChunkId id;
    public GameObject chunkGameObject;
    public MeshFilter meshFilter;
    public MeshCollider meshCollider;

    public Mesh mesh;

    public bool hasDataGenerated = false;
    public bool hasChanged = false;
    public bool hasMeshGenerated = false;
    public bool hasColliderBaked = false;

    private GameObject[] treePrefabs;

    public int minScale = -1;

    public Chunk(ChunkId id, GameObject chunkGameObject) {
        this.id = id;
        this.chunkGameObject = chunkGameObject;
        chunkGameObject.name = $"Chunk({id.id.x}, {id.id.y}, {id.id.z})";
        this.meshFilter = chunkGameObject.GetComponent<MeshFilter>();
        this.meshCollider = chunkGameObject.GetComponent<MeshCollider>();
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
        this.hasMeshGenerated = true;
    }

    public void SetCollider() {
        this.meshCollider.sharedMesh = mesh;
    }

    public void Load() {
        chunkGameObject.SetActive(true);
    }

    public void Unload() {
        chunkGameObject.SetActive(false);
    }
}