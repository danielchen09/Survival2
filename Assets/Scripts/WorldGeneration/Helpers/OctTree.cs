using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class OctTreeNode {
    public ChunkId id;
    public int lod;
    public OctTreeNode[] children;

    public OctTreeNode(ChunkId pos, int lod) {
        this.id = pos;
        this.lod = lod;
        this.children = null;
    }

    public float SideLength() {
        return (1 << lod) * (WorldSettings.chunkDimension - 1) * WorldSettings.voxelSize;
    }

    public float3 CenterPos() {
        return id.ToWorldCoord() + new float3(SideLength()) / 2;
    }
}
public class OctTree {
    public static float SUBDIVIDE_THRESHOLD = 1.2f;
    public static float SQRT3 = math.sqrt(3);
    public OctTreeNode rootNode;
    public List<OctTreeNode> activeNodes;

    public OctTree() {
        this.activeNodes = new List<OctTreeNode>();
    }

    public OctTree(int maxLod, float3 playerPos) {
        ChunkId playerChunkCoord = ChunkId.FromWorldCoord(playerPos);
        this.rootNode = new OctTreeNode(new ChunkId(playerChunkCoord.pos - (1 << (maxLod - 1))), maxLod);
        this.activeNodes = new List<OctTreeNode>();
        Subdivide(this.rootNode, playerPos);
    }

    public void Subdivide(OctTreeNode node, float3 playerPos) {
        if (node.lod == 0) {
            activeNodes.Add(node);
            return;
        }
        float dist = Utils.Magnitude(playerPos - node.CenterPos());
        if (dist <= SQRT3 * node.SideLength() / 2 * SUBDIVIDE_THRESHOLD) {
            node.children = new OctTreeNode[8];
            Utils.For3(2, (x, y, z) => {
                int3 coord = new int3(x, y, z);
                int newLod = node.lod - 1;
                OctTreeNode childNode = new OctTreeNode(new ChunkId(node.id.pos + coord * (1 << newLod)), newLod);
                node.children[Utils.CoordToIndex(coord, 2)] = childNode;
                Subdivide(childNode, playerPos);
            });
        } else {
            activeNodes.Add(node);
        }
    }
}
