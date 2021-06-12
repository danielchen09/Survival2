using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct MarchingCubeJob : IJob {
    [ReadOnly]
    public int3 dimension;
    [ReadOnly]
    public float voxelSize;
    [ReadOnly]
    public int lod; // powers of 2, higher lod means lower resolution

    [ReadOnly]
    public NativeArray<VoxelData> voxelData;
    [NativeDisableParallelForRestriction]
    public NativeList<VertexData> vertexData;
    [NativeDisableParallelForRestriction]
    public NativeList<ushort> triangles;

    [ReadOnly]
    public ChunkId chunkId;

    public void Execute() {
        for (int x = 0; x < dimension.x - lod; x += lod) {
            for (int y = 0; y < dimension.y - lod; y += lod) {
                for (int z = 0; z < dimension.z - lod; z += lod) {
                    int3 coord = new int3(x, y, z);

                    int cubeIndex = 0;
                    for (int vertex = 0; vertex < 8; vertex++) {
                        if (voxelData[VertexIndex(coord, vertex)].density < WorldSettings.isoLevel) {
                            cubeIndex |= (1 << vertex);
                        }
                    }

                    if (cubeIndex == 0 || cubeIndex == 255)
                        continue;
                    
                    for (int i = 0; i < 15 && MarchingCubeTable.triTable[cubeIndex * 16 + i] != -1; i += 3) {
                        int e1 = MarchingCubeTable.triTable[cubeIndex * 16 + i];
                        int e2 = MarchingCubeTable.triTable[cubeIndex * 16 + i + 1];
                        int e3 = MarchingCubeTable.triTable[cubeIndex * 16 + i + 2];

                        float3 v1 = InterpolateVertex(coord, e1) * voxelSize;
                        float3 v2 = InterpolateVertex(coord, e2) * voxelSize;
                        float3 v3 = InterpolateVertex(coord, e3) * voxelSize;

                        float3 normal = math.normalize(math.cross(v2 - v1, v3 - v1));

                        Color32 color = VoteColor(ChooseColor(coord, e1, v1), ChooseColor(coord, e2, v2), ChooseColor(coord, e3, v3));
                        Color32 rcolor = RandomizeColor(color, 0.02f, new float3(x, y, z));

                        vertexData.Add(new VertexData(v1, normal, color));
                        triangles.Add((ushort)(vertexData.Length - 1));
                        vertexData.Add(new VertexData(v2, normal, color));
                        triangles.Add((ushort)(vertexData.Length - 1));
                        vertexData.Add(new VertexData(v3, normal, color));
                        triangles.Add((ushort)(vertexData.Length - 1));
                    }
                }
            }
        }
    }

    private Color32 RandomizeColor(Color32 color, float radius, float3 pos) {
        pos = pos * voxelSize + chunkId.ToWorldCoord();
        float phi = 2 * math.PI * NoiseGenerator.SnoisePositive(pos * 0.05f);
        float theta = 2 * math.PI * NoiseGenerator.SnoisePositive(pos * 0.05f);
        return Utils.Round(new Color(
            radius * math.cos(phi) * math.sin(theta),
            radius * math.sin(phi) * math.cos(theta),
            radius * math.cos(theta)), radius / 3f) + color;
    }

    private Color32 VoteColor(Color32 c1, Color32 c2, Color32 c3) {
        if (Utils.Equals(c1, c2) || Utils.Equals(c1, c3))
            return c1;
        if (Utils.Equals(c2, c3))
            return c3;
        return c1;
    }

    private Color InterpolateColor(int3 coord, int edge, float3 edgePos) {
        int neighbor1 = MarchingCubeTable.edgeNeighbors[edge * 2];
        int neighbor2 = MarchingCubeTable.edgeNeighbors[edge * 2 + 1];

        Color c1 = voxelData[VertexIndex(coord, neighbor1)].material.color;
        Color c2 = voxelData[VertexIndex(coord, neighbor2)].material.color;

        float3 p1 = VertexCoord(coord, neighbor1);
        float3 p2 = VertexCoord(coord, neighbor2);

        float ratio = Utils.Magnitude(edgePos - p1) / Utils.Magnitude(p2 - p1);
        return c1 + (c2 - c1) * ratio;
    }

    private Color32 ChooseColor(int3 coord, int edge, float3 edgePos) {
        int neighbor1 = MarchingCubeTable.edgeNeighbors[edge * 2];
        int neighbor2 = MarchingCubeTable.edgeNeighbors[edge * 2 + 1];

        Color32 c1 = voxelData[VertexIndex(coord, neighbor1)].material.color;
        Color32 c2 = voxelData[VertexIndex(coord, neighbor2)].material.color;

        float3 p1 = VertexCoord(coord, neighbor1);
        float3 p2 = VertexCoord(coord, neighbor2);

        return Utils.Magnitude(edgePos - p1) < Utils.Magnitude(edge - p2) ? c1 : c2;
    }

    private float3 InterpolateVertex(int3 coord, int edge) {
        int neighbor1 = MarchingCubeTable.edgeNeighbors[edge * 2];
        int neighbor2 = MarchingCubeTable.edgeNeighbors[edge * 2 + 1];

        float v1 = voxelData[VertexIndex(coord, neighbor1)].density;
        float v2 = voxelData[VertexIndex(coord, neighbor2)].density;

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
        return coord + MarchingCubeTable.vertexOffsets[vertex] * lod;
    }

    public void Dispose() {
        this.voxelData.Dispose();
        this.vertexData.Dispose();
        this.triangles.Dispose();
    }
}