using Unity.Mathematics;

public class Utils {
    public static int CoordToIndex(int3 coord) {
        return coord.z * WorldSettings.chunkDimension.x * WorldSettings.chunkDimension.y +
            coord.y * WorldSettings.chunkDimension.x +
            coord.x;
    }

    public static int3 IndexToCoord(int index) {
        return new int3(
            index % WorldSettings.chunkDimension.x,
            (index / WorldSettings.chunkDimension.x) % WorldSettings.chunkDimension.y,
            index / (WorldSettings.chunkDimension.x * WorldSettings.chunkDimension.z));
    }

    public static float Magnitude(float3 v) {
        return math.sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
    }
}