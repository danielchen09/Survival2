using Unity.Mathematics;
using UnityEngine;

public class Utils {
    public static int CoordToIndex(int3 coord) {
        return coord.z * WorldSettings.chunkDimension.x * WorldSettings.chunkDimension.y +
            coord.y * WorldSettings.chunkDimension.x +
            coord.x;
    }

    public static int CoordToIndex2D(int2 coord) {
        return coord.y * WorldSettings.chunkDimension.x + coord.x;
    }

    public static int3 IndexToCoord(int index) {
        return new int3(
            index % WorldSettings.chunkDimension.x,
            (index / WorldSettings.chunkDimension.x) % WorldSettings.chunkDimension.y,
            index / (WorldSettings.chunkDimension.x * WorldSettings.chunkDimension.z));
    }

    public static int2 IndexToCoord2D(int index) {
        return new int2(
            index % WorldSettings.chunkDimension.x,
            index / WorldSettings.chunkDimension.x);
    }

    public static float Round(float f, float m) {
        return math.round(f / m) * m;
    }

    public static float3 Round(float3 v, float m) {
        return math.round(v / m) * m; 
    }

    public static Color Round(Color c, float m) {
        return new Color(Round(c.r, m), Round(c.g, m), Round(c.b, m));
    }

    public static bool Equals(Color32 c1, Color32 c2) {
        return c1.r == c2.r && c1.g == c2.g && c1.b == c2.b && c1.a == c2.a;
    }

    public static float Magnitude(float3 v) {
        return math.sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
    }
}