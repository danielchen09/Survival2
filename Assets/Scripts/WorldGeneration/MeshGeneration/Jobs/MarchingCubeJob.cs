using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct MarchingCubeJob : IJob {
    public NativeCounter counter;
    [ReadOnly]
    public NativeArray<VoxelData> voxelData;
    [NativeDisableParallelForRestriction, WriteOnly]
    public NativeArray<VertexData> vertexData;
    [NativeDisableParallelForRestriction, WriteOnly]
    public NativeArray<ushort> triangles;

    public void Execute() {
        for (int x = 0; x < WorldSettings.chunkDimension.x - 1; x++) {
            for (int y = 0; y < WorldSettings.chunkDimension.y - 1; y++) {
                for (int z = 0; z < WorldSettings.chunkDimension.z - 1; z++) {
                    int3 coord = new int3(x, y, z);

                    int cubeIndex = 0;
                    for (int vertex = 0; vertex < 8; vertex++) {
                        if (voxelData[VertexIndex(coord, vertex)].density < WorldSettings.isoLevel) {
                            cubeIndex |= (1 << vertex);
                        }
                    }

                    if (cubeIndex == 0 || cubeIndex == 255)
                        continue;

                    for (int i = 0; i < 15 && MarchingCubeTable.triTable[cubeIndex, i] != -1; i += 3) {
                        int vertexIndex = counter.Increment() * 3;

                        float3 v1 = Interpolate(coord, MarchingCubeTable.triTable[cubeIndex, i]);
                        float3 v2 = Interpolate(coord, MarchingCubeTable.triTable[cubeIndex, i + 1]);
                        float3 v3 = Interpolate(coord, MarchingCubeTable.triTable[cubeIndex, i + 2]);

                        float3 normal = math.normalize(math.cross(v2 - v1, v3 - v1));

                        vertexData[vertexIndex] = new VertexData(v1, normal, voxelData[Utils.CoordToIndex(coord)].color);
                        triangles[vertexIndex] = (ushort)(vertexIndex);

                        vertexData[vertexIndex + 1] = new VertexData(v2, normal, voxelData[Utils.CoordToIndex(coord)].color);
                        triangles[vertexIndex + 1] = (ushort)(vertexIndex + 1);

                        vertexData[vertexIndex + 2] = new VertexData(v3, normal, voxelData[Utils.CoordToIndex(coord)].color);
                        triangles[vertexIndex + 2] = (ushort)(vertexIndex + 2);
                    }
                }
            }
        }
    }

    private float3 Interpolate(int3 coord, int edge) {
        int neighbor1 = MarchingCubeTable.edgeNeighbors[edge, 0];
        int neighbor2 = MarchingCubeTable.edgeNeighbors[edge, 1];

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
        return coord + MarchingCubeTable.vertexOffsets[vertex];
    }

    public void Dispose() {
        this.counter.Dispose();
        this.voxelData.Dispose();
        this.vertexData.Dispose();
        this.triangles.Dispose();
    }
}