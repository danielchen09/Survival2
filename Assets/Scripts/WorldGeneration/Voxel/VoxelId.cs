using System.Collections.Generic;
using Unity.Mathematics;

public struct VoxelId {
    public int3 id;

    public VoxelId(int3 id) {
        this.id = id;
    }

    public List<ChunkId> OverlappingChunks(ChunkId originalChunk) {
        List<ChunkId> chunks = new List<ChunkId>();

        if (!IsBoundary())
            return chunks;

        for (int b = 1; b <= 0b111; b++) {
            int changed = 0;
            ChunkId newChunk = new ChunkId(originalChunk.id);
            for (int i = 0; i < 3; i++) {
                if ((b & (1 << i)) > 0) {
                    if (id[i] == 0) {
                        newChunk[i] -= 1;
                        changed |= (1 << i);
                    } else if (id[i] == WorldSettings.chunkDimension[i] - 1) {
                        newChunk[i] += 1;
                        changed |= (1 << i);
                    }
                }
            }
            if (changed == b)
                chunks.Add(newChunk);
        }

        return chunks;
    }

    public VoxelId Translate(ChunkId originalChunk, ChunkId newChunk) {
        int3 offset = newChunk.id - originalChunk.id;
        return new VoxelId(id - offset * (WorldSettings.chunkDimension - new int3(1, 1, 1)));
    }

    public float3 ToWorldCoord(ChunkId chunk) {
        return (float3)id * WorldSettings.voxelSize + chunk.ToWorldCoord();
    }

    public bool IsBoundary() {
        for (int i = 0; i < 3; i++) {
            if (id[i] == 0 || id[i] == WorldSettings.chunkDimension[i] - 1)
                return true;
        }
        return false;
    }

    public int3 ChunkOffset() {
        return (int3)math.floor((float3)id / (float3)WorldSettings.chunkDimension);
    }

    public List<VoxelId> GetNeighbors(int radius) {
        List<VoxelId> neighbors = new List<VoxelId>();
        for (int x = -radius; x <= radius; x++) {
            for (int y = -radius; y <= radius; y++) {
                for (int z = -radius; z <= radius; z++) {
                    VoxelId neighbor = new VoxelId(new int3(x, y, z) + id);
                    if (Utils.Magnitude(neighbor.id - id) <= radius)
                        neighbors.Add(neighbor);
                }
            }
        }
        return neighbors;
    }

    public bool InChunk() {
        for (int i = 0; i < 3; i++) {
            if (id[i] < 0 || id[i] >= WorldSettings.chunkDimension[i])
                return false;
        }
        return true;
    }

    public static VoxelId FromWorldCoord(float3 coord, ChunkId chunk) {
        int3 globalVoxelId = (int3)math.round(coord / WorldSettings.voxelSize);
        return new VoxelId(globalVoxelId - chunk.ToVoxelCoord());
    }

    public override bool Equals(object obj) {
        VoxelId other = (VoxelId)obj;
        return other.id.Equals(other.id);
    }

    public override int GetHashCode() {
        return id.GetHashCode();
    }
}