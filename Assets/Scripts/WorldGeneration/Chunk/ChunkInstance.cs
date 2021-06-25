using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ChunkInstance : MonoBehaviour {
    public Chunk chunk = null;

    private void OnDrawGizmos() {
        if (chunk != null) {
            Color color = Color.green + chunk.lod / (float)(ChunkController.MAX_LOD - 1) * (Color.red - Color.green);
            Gizmos.color = new Color(color.r, color.g, color.b, 0.2f);
            float sizeLength = 16 * (1 << chunk.lod) * WorldSettings.voxelSize;
            float3 chunkPos = chunk.id.pos * 16;
            Gizmos.DrawCube(chunkPos + sizeLength / 2, Vector3.one * sizeLength);
        }
    }
}
