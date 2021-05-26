using Unity.Mathematics;

public struct ChunkId {
    public int3 id;

    public int this[int index] {
        get => id[index];
        set => id[index] = value;
    }

    public ChunkId(int3 id) {
        this.id = id;
    }

    public ChunkId(int x, int y, int z) {
        this.id = new int3(x, y, z);
    }

    public int3 ToVoxelCoord() {
        return id * (WorldSettings.chunkDimension - new int3(1, 1, 1));
    }

    public float3 ToWorldCoord() {
        return (float3)ToVoxelCoord() * WorldSettings.voxelSize;
    }

    public float3 CenterWorldCoord() {
        return (float3)ToWorldCoord() * WorldSettings.voxelSize + WorldSettings.voxelSize * (float3)WorldSettings.chunkDimension / 2;
    }

    public static ChunkId FromWorldCoord(float3 coord) {
        return new ChunkId((int3)math.floor(coord / (float3)WorldSettings.chunkDimension / WorldSettings.voxelSize));
    }

    public override bool Equals(object obj) {
        ChunkId other = (ChunkId)obj;
        return id.Equals(other.id);
    }

    public override int GetHashCode() {
        return id.GetHashCode();
    }
}