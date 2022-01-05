using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public struct WorldSettings {
    // chunk and voxel data
    public static readonly int3 chunkDimension = new int3(17, 17, 17); // how many vertices are in a chunk
    public static int chunkDataLength {
        get => chunkDimension.x * chunkDimension.y * chunkDimension.z;
    }
    public static readonly float voxelSize = 1f; // distance between vertices
    public static readonly int WorldHeightInChunks = 8;
    public static float WorldHeight {
        get => WorldHeightInChunks * (chunkDimension.y - 1) * voxelSize;
    }

    public static readonly int WorldDepthInChunks = 8;
    public static float WorldDepth {
        get => WorldDepthInChunks * (chunkDimension.y - 1) * voxelSize;
    }

    // noise generation
    public static readonly int octaves = 5;
    public static readonly float frequency = 0.01f;
    public static readonly float lacunarity = 2f;
    public static readonly float persistence = 0.5f;

    // mesh generation
    public static readonly float isoLevel = 0;

    // player settings
    public static readonly int RenderDistanceInChunks = 5;
    public static float3 RenderDistance {
        get => RenderDistanceInChunks * (float3)chunkDimension * voxelSize;
    }
    public static readonly int ModifyVoxelRadius = 1;
    public static readonly float ReachDistance = 100f;
    public static readonly float VoxelModifier = 0.2f;
}