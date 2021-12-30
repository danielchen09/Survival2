using System.Collections.Generic;
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

    public void SetMeshData(List<VertexData> vertices, List<ushort> triangles, List<ushort> grassTriangles) {
        mesh = GenerateMesh(vertices, triangles);
        this.meshFilter.mesh = mesh;
        if (grassTriangles.Count > 0) {
            Mesh grassMesh = GenerateMesh(vertices, grassTriangles);
            this.grassSurface.GetComponent<MeshFilter>().mesh = grassMesh;
            this.grassSurface.SetActive(true);
        }
        this.hasMeshGenerated = true;
    }

    public Mesh GenerateMesh(List<VertexData> vertices, List<ushort> triangles) {
        Mesh mesh = new Mesh();
        SubMeshDescriptor subMesh = new SubMeshDescriptor();

        mesh.SetVertexBufferParams(vertices.Count, VertexData.bufferMemoryLayout);
        mesh.SetIndexBufferParams(vertices.Count, IndexFormat.UInt16);

        mesh.SetVertexBufferData(vertices, 0, 0, vertices.Count, 0, MeshUpdateFlags.DontValidateIndices);
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.subMeshCount = 1;
        subMesh.indexCount = triangles.Count;
        subMesh.topology = MeshTopology.Triangles;
        mesh.SetSubMesh(0, subMesh);
        Vector2[] uvs = new Vector2[vertices.Count];
        for (int i = 0; i < vertices.Count; i++) {
            uvs[i] = new Vector2(vertices[i].position.x / (WorldSettings.chunkDimension.x - 1) / WorldSettings.voxelSize, vertices[i].position.z / (WorldSettings.chunkDimension.z - 1) / WorldSettings.voxelSize);
        }
        mesh.uv = uvs;

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

    public override int GetHashCode() {
        return id.GetHashCode();
    }
}