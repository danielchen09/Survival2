using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class MarchingCubesTask : ThreadedTask {
    private VoxelData[] terrainData;
    public Chunk chunk;

    public List<VertexData> vertexData;
    public List<ushort> triangles;
    public List<ushort> grassMesh;

    public MarchingCubesTask(VoxelData[] terrainData, Chunk chunk) {
        this.terrainData = terrainData;
        this.chunk = chunk;
        this.vertexData = new List<VertexData>();
        this.triangles = new List<ushort>();
        this.grassMesh = new List<ushort>();
    }

    public override void ThreadFunction() {
        for (int x = 0; x < WorldSettings.chunkDimension.x - 1; x += 1) {
            for (int y = 0; y < WorldSettings.chunkDimension.y - 1; y += 1) {
                for (int z = 0; z < WorldSettings.chunkDimension.z - 1; z += 1) {
                    int3 coord = new int3(x, y, z);

                    int cubeIndex = 0;
                    for (int vertex = 0; vertex < 8; vertex++) {
                        if (terrainData[VertexIndex(coord, vertex)].density < WorldSettings.isoLevel) {
                            cubeIndex |= (1 << vertex);
                        }
                    }

                    if (cubeIndex == 0 || cubeIndex == 255)
                        continue;

                    for (int i = 0; i < 15 && MarchingCubeTable.triTable[cubeIndex * 16 + i] != -1; i += 3) {
                        int e1 = MarchingCubeTable.triTable[cubeIndex * 16 + i];
                        int e2 = MarchingCubeTable.triTable[cubeIndex * 16 + i + 1];
                        int e3 = MarchingCubeTable.triTable[cubeIndex * 16 + i + 2];

                        float3 v1 = InterpolateVertex(coord, e1) * WorldSettings.voxelSize;
                        float3 v2 = InterpolateVertex(coord, e2) * WorldSettings.voxelSize;
                        float3 v3 = InterpolateVertex(coord, e3) * WorldSettings.voxelSize;

                        float3 normal = math.normalize(math.cross(v2 - v1, v3 - v1));

                        VoxelData a1 = ChooseAttribute(coord, e1, v1);
                        VoxelData a2 = ChooseAttribute(coord, e2, v2);
                        VoxelData a3 = ChooseAttribute(coord, e3, v3);
                        Color32 color = VoteColor(a1.material.color, a2.material.color, a3.material.color);
                        bool CanSpawnGrass = VoteCanSpawnGrass(a1.material.canSpawnGrass, a2.material.canSpawnGrass, a3.material.canSpawnGrass);

                        vertexData.Add(new VertexData(v1, normal, color));
                        triangles.Add((ushort)(vertexData.Count - 1));
                        vertexData.Add(new VertexData(v2, normal, color));
                        triangles.Add((ushort)(vertexData.Count - 1));
                        vertexData.Add(new VertexData(v3, normal, color));
                        triangles.Add((ushort)(vertexData.Count - 1));
                        if (CanSpawnGrass) {
                            grassMesh.Add((ushort)(vertexData.Count - 3));
                            grassMesh.Add((ushort)(vertexData.Count - 2));
                            grassMesh.Add((ushort)(vertexData.Count - 1));
                        }
                    }
                }
            }
        }
    }

    private Color32 VoteColor(Color32 c1, Color32 c2, Color32 c3) {
        if (Utils.Equals(c1, c2) || Utils.Equals(c1, c3))
            return c1;
        if (Utils.Equals(c2, c3))
            return c3;
        return c1;
    }
    private bool VoteCanSpawnGrass(bool b1, bool b2, bool b3) {
        if (b1 == b2 || b1 == b3)
            return b1;
        if (b2 == b3)
            return b3;
        return b1;
    }

    private VoxelData ChooseAttribute(int3 coord, int edge, float3 edgePos) {
        int neighbor1 = MarchingCubeTable.edgeNeighbors[edge * 2];
        int neighbor2 = MarchingCubeTable.edgeNeighbors[edge * 2 + 1];

        VoxelData c1 = terrainData[VertexIndex(coord, neighbor1)];
        VoxelData c2 = terrainData[VertexIndex(coord, neighbor2)];

        float3 p1 = VertexCoord(coord, neighbor1);
        float3 p2 = VertexCoord(coord, neighbor2);

        return Utils.Magnitude(edgePos - p1) < Utils.Magnitude(edge - p2) ? c1 : c2;
    }

    private float3 InterpolateVertex(int3 coord, int edge) {
        int neighbor1 = MarchingCubeTable.edgeNeighbors[edge * 2];
        int neighbor2 = MarchingCubeTable.edgeNeighbors[edge * 2 + 1];

        float v1 = terrainData[VertexIndex(coord, neighbor1)].density;
        float v2 = terrainData[VertexIndex(coord, neighbor2)].density;

        float3 p1 = VertexCoord(coord, neighbor1);
        float3 p2 = VertexCoord(coord, neighbor2);

        if (math.abs(WorldSettings.isoLevel - v1) < 0.00001f)
            return p1;
        if (math.abs(WorldSettings.isoLevel - v2) < 0.00001f)
            return p2;
        if (math.abs(v1 - v2) < 0.00001f)
            return p1;
        return p1 + (WorldSettings.isoLevel - v1) * (p2 - p1) / (v2 - v1);
    }

    private int VertexIndex(int3 coord, int vertex) {
        return Utils.CoordToIndex(VertexCoord(coord, vertex));
    }

    private int3 VertexCoord(int3 coord, int vertex) {
        return coord + MarchingCubeTable.vertexOffsets[vertex];
    }

    public override void OnFinish() {
        chunk.SetMeshData(vertexData, triangles, grassMesh);
        chunk.hasChanged = false;
        chunk.hasMeshGenerated = true;
    }

    public override int GetHashCode() {
        return chunk.GetHashCode();
    }

    public override bool Equals(object obj) {
        if (obj.GetType() != this.GetType())
            return false;
        return ((MarchingCubesTask)obj).chunk.Equals(chunk);
    }
}
