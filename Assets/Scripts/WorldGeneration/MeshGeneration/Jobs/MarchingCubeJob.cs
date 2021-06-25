using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct MarchingCubeJob : IJob {
    public const float EPSILON = 0.00001f;

    public float voxelSize;
    public int lod;
    public ChunkId chunkId;

    // outer level indexed by chunkid, inner level indexed by voxelid
    // outer level dimension: (1 << lod) ^ 3
    // inner level dimension: 17 ^ 3
    public NativeArray<VoxelData> voxelDataList;
    public NativeList<VertexData> vertices;
    public NativeList<int> normalCount;
    public NativeList<ushort> triangles;
    // 16x16x16x4 for each cell, edge 0 1 2 3
    public NativeArray<ushort> sharedData;

    public void Execute() {
        for (int x = 0; x < 16; x++) {
            for (int y = 0; y < 16; y++) {
                for (int z = 0; z < 16; z++) {
                    int3 coord = new int3(x, y, z);
                    if (IsTransitionCell(coord)) {
                        ConstructTransitionCell(coord);
                    } else {
                        ConstructRegularCell(coord);
                    }
                }
            }
        }
        for (int i = 0; i < vertices.Length; i++) {
            vertices[i] = ModifyNormal(vertices[i], math.normalize(vertices[i].normal / normalCount[i]));
        }
    }

    public bool IsTransitionCell(int3 coord) {
        return false; // TODO: determine whether a cell is a transition cell
    }

    public void ConstructRegularCell(int3 coord) {
        int caseCode = 0;
        for (int corner = 0; corner < 8; corner++) {
            if (GetData(CornerCoord(coord, corner)).density < 0) {
                caseCode |= (1 << corner);
            }
        }
        if (caseCode == 0 || caseCode == 255)
            return;

        byte cellClass = TransvoxelTable.RegularCellClass[caseCode];
        ushort triangleCount = (ushort)(TransvoxelTable.RegularGeometryCount[cellClass] & 0x0F);
        ushort[] vertexDataList = TransvoxelTable.RegularVertexData[caseCode];
        byte[] vertexIndex = TransvoxelTable.RegularVertexIndex[cellClass];

        for (int i = 0; i < triangleCount; i++) {
            ushort vertexData0 = vertexDataList[vertexIndex[i * 3]];
            ushort vertexData1 = vertexDataList[vertexIndex[i * 3 + 1]];
            ushort vertexData2 = vertexDataList[vertexIndex[i * 3 + 2]];

            float3 v0 = ProcessVertex(coord, vertexData0);
            float3 v1 = ProcessVertex(coord, vertexData1);
            float3 v2 = ProcessVertex(coord, vertexData2);

            float3 normal = math.normalize(math.cross(v1 - v0, v2 - v0));
            for (int j = 0; j < 3; j++) {
                int modifyIndex = triangles[triangles.Length - 1 - j];
                vertices[modifyIndex] = ModifyNormal(vertices[modifyIndex], vertices[modifyIndex].normal + normal);
                normalCount[modifyIndex] = normalCount[modifyIndex] + 1;
            }
        }
    }

    public void ConstructTransitionCell(int3 coord) {

    }

    public float3 ProcessVertex(int3 coord, ushort vertexData) {
        ushort v0 = (ushort)((vertexData >> 4) & 0x0F);
        ushort v1 = (ushort)(vertexData & 0x0F);

        float d0 = GetData(CornerCoord(coord, v0)).density;
        float d1 = GetData(CornerCoord(coord, v1)).density;

        // t larger => further away from p1, limited to 256 values
        float t = d1 / (d1 - d0);

        if (1 - t < EPSILON) {
            // at high numbered endpoint
            if (v1 == 7) {
                // cell owns vertex, create vertex on corner
                return AddVertexAtCorner(coord, v1);
            } else {
                // share vertex on corner
                int3 cellToShare = coord + GetShareOffsetFromCorner(v1);
                if (!InBound(cellToShare)) {
                    return AddVertexAtCorner(coord, v1);
                } else {
                    return ShareVertex(cellToShare, 0);
                }
            }
        } else if (t > EPSILON) {
            // at interior of edge
            if (v1 == 7) {
                // cell owns vertex, create vertex on edge
                return AddVertexAtEdge(coord, vertexData, t);
            } else {
                // share vertex, share vertex on edge
                int3 cellToShare = coord + GetShareOffsetFromCorner(v1);
                if (!InBound(cellToShare)) {
                    return AddVertexAtEdge(coord, vertexData, t);
                } else {
                    return ShareVertex(cellToShare, (vertexData >> 8) & 0x0F);
                }
            }
        } else {
            // at low numbered endpoint
            // share if possible
            int3 cellToShare = coord + GetShareOffsetFromCorner(v0);
            if (!InBound(cellToShare)) {
                return AddVertexAtCorner(coord, v0);
            } else {
                return ShareVertex(cellToShare, 0);
            }
        }
    }
    public float3 AddVertexAtCorner(int3 coord, int corner) {
        float3 vertexPos = (float3)CornerCoord(coord, corner) * (1 << lod) * voxelSize;
        ushort addIndex = (ushort)vertices.Length;
        if (corner == 7)
            sharedData[Utils.CoordToIndex(coord, 16) * 4] = addIndex;
        vertices.Add(new VertexData(vertexPos, 0, Color.white));
        triangles.Add(addIndex);
        normalCount.Add(0);
        return vertexPos;
    }

    public float3 AddVertexAtEdge(int3 coord, ushort vertexData, float t) {
        ushort v0 = (ushort)((vertexData >> 4) & 0x0F);
        ushort v1 = (ushort)(vertexData & 0x0F);

        float3 p0 = (float3)CornerCoord(coord, v0) * (1 << lod) * voxelSize;
        float3 p1 = (float3)CornerCoord(coord, v1) * (1 << lod) * voxelSize;

        float3 vertexPos = p1 + t * (p0 - p1);
        ushort addIndex = (ushort)vertices.Length;
        int addEdge = (vertexData >> 8) & 0x0F;
        if (v1 == 7)
            sharedData[Utils.CoordToIndex(coord, 16) * 4 + addEdge] = addIndex;
        vertices.Add(new VertexData(vertexPos, 0, Color.white));
        triangles.Add(addIndex);
        normalCount.Add(0);
        return vertexPos;
    }

    public float3 ShareVertex(int3 cellToShare, int shareEdge) {
        ushort shareIndex = sharedData[Utils.CoordToIndex(cellToShare, 16) * 4 + shareEdge];
        triangles.Add(shareIndex);
        return vertices[shareIndex].position;
    }

    public VoxelData GetData(int3 coord) {
        // asking for which voxel for lod0 then its in a regular chunk
        // if lod > 0 then this will span across multiple chunks
        int3 voxelCoord = coord * (1 << lod);

        // which chunk the voxel requested is in;
        // for lod 0 then its always chunk 0
        int3 chunkCoord = voxelCoord / 17;

        // index in voxelDataList the requested chunk is in
        int chunkIndex = Utils.CoordToIndex(chunkCoord, 1 << lod);
        // index in voxelDataList the requested voxel is in
        int voxelIndex = Utils.CoordToIndex(voxelCoord % 17);

        return voxelDataList[chunkIndex * 17 * 17 * 17 + voxelIndex];
    }

    public int3 GetShareOffsetFromCorner(ushort corner) {
        return MarchingCubeTable.ShareOffset[corner];
    }
        
    public int3 CornerCoord(int3 coord, int corner) {
        return coord + MarchingCubeTable.CornerOffset[corner];
    }

    public bool InBound(int3 coord) {
        for (int i = 0; i < 3; i++) {
            if (coord[i] < 0 || coord[i] >= 16)
                return false;
        }
        return true;
    }

    private VertexData ModifyNormal(VertexData original, float3 normal) {
        return new VertexData(original.position, normal, original.color);
    }

    public void Dispose() {
        this.voxelDataList.Dispose();
        this.vertices.Dispose();
        this.triangles.Dispose();
        this.normalCount.Dispose();
        this.sharedData.Dispose();
    }
}