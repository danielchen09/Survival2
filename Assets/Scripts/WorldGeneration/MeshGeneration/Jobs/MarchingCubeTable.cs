using Unity.Mathematics;

public struct MarchingCubeTable {
    public static readonly int3[] CornerOffset = new int3[] {
        new int3(0, 0, 0),
        new int3(1, 0, 0),
        new int3(0, 0, 1),
        new int3(1, 0, 1),
        new int3(0, 1, 0),
        new int3(1, 1, 0),
        new int3(0, 1, 1),
        new int3(1, 1, 1)
    };

    public static readonly int3[] ShareOffset = new int3[] {
        new int3(-1, -1, -1),
        new int3(0, -1, -1),
        new int3(-1, -1, 0),
        new int3(0, -1, 0),
        new int3(-1, 0, -1),
        new int3(0, 0, -1),
        new int3(-1, 0, 0)
    };
}