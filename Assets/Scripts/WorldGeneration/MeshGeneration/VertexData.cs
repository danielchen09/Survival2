using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public struct VertexData {
    public float3 position;
    public float3 normal;
    public Color32 color;

    public VertexData(float3 position, float3 normal, Color32 color) {
        this.position = position;
        this.normal = normal;
        this.color = color;
    }

    public static readonly VertexAttributeDescriptor[] bufferMemoryLayout = {
        new VertexAttributeDescriptor(VertexAttribute.Position),
        new VertexAttributeDescriptor(VertexAttribute.Normal),
        new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4)
    };
}