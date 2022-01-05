using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Chunk {
    public ChunkId id;
    public GameObject chunkGameObject;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public MeshCollider meshCollider;

    public GameObject grassSurface;

    public Mesh mesh;

    public bool hasDataGenerated = false;
    public bool hasChanged = false;
    public bool hasMeshGenerated = false;
    public bool hasColliderBaked = false;
    public bool hasSurface = false;
    public bool hasEntitiesSpawned = false;

    public int minScale = -1;

    public Chunk(ChunkId id, GameObject chunkGameObject) {
        this.id = id;
        this.chunkGameObject = chunkGameObject;
        chunkGameObject.name = $"Chunk({id.pos.x}, {id.pos.y}, {id.pos.z})";
        this.meshFilter = chunkGameObject.GetComponent<MeshFilter>();
        this.meshCollider = chunkGameObject.GetComponent<MeshCollider>();

        this.grassSurface = chunkGameObject.transform.GetChild(0).gameObject;
    }

    public void SetMeshData(NativeArray<VertexData> vertices, NativeArray<ushort> triangles, NativeArray<ushort> grassTriangles) {
        mesh = GenerateMesh(vertices, triangles);
        this.meshFilter.mesh = mesh;
        if (grassTriangles.Length > 0) {
            Mesh grassMesh = GenerateMesh(vertices, grassTriangles);
            this.grassSurface.GetComponent<MeshFilter>().mesh = grassMesh;
            this.grassSurface.SetActive(true);
        }
        this.hasMeshGenerated = true;
    }

    public Mesh GenerateMesh(NativeArray<VertexData> vertices, NativeArray<ushort> triangles) {
        Mesh mesh = new Mesh();

        mesh.SetVertexBufferParams(vertices.Length, VertexData.bufferMemoryLayout);
        mesh.SetIndexBufferParams(vertices.Length, IndexFormat.UInt16);

        mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length, 0, MeshUpdateFlags.DontValidateIndices);
        mesh.SetTriangles(triangles.ToArray(), 0);

        mesh.RecalculateBounds();
        return mesh;
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