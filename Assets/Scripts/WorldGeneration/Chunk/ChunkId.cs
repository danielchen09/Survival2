using Unity.Mathematics;

public struct ChunkId {
    public int3 pos;

    public int this[int index] {
        get => pos[index];
        set => pos[index] = value;
    }

    public ChunkId(int3 id) {
        this.pos = id;
    }

    public ChunkId(int x, int y, int z) {
        this.pos = new int3(x, y, z);
    }

    public int3 ToVoxelCoord() {
        return pos * (WorldSettings.chunkDimension - new int3(1, 1, 1));
    }

    public float3 ToWorldCoord() {
        return (float3)ToVoxelCoord() * WorldSettings.voxelSize;
    }

    public float3 CenterWorldCoord() {
        return (float3)ToWorldCoord() * WorldSettings.voxelSize + WorldSettings.voxelSize * (float3)WorldSettings.chunkDimension / 2;
    }

    public static ChunkId FromWorldCoord(float3 coord) {
        return new ChunkId((int3)math.floor(coord / (float3)(WorldSettings.chunkDimension - 1) / WorldSettings.voxelSize));
    }

    public ChunkId Offset(int3 offset) {
        return new ChunkId(pos + offset);
    }

    public override bool Equals(object obj) {
        ChunkId other = (ChunkId)obj;
        return pos.Equals(other.pos);
    }

    public override int GetHashCode() {
        return pos.GetHashCode();
    }
}