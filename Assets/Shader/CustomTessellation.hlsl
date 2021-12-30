
#if defined(SHADER_API_D3D11) || defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE) || defined(SHADER_API_VULKAN) || defined(SHADER_API_METAL) || defined(SHADER_API_PSSL)
	#define UNITY_CAN_COMPILE_TESSELLATION 1
	#   define UNITY_domain                 domain
	#   define UNITY_partitioning           partitioning
	#   define UNITY_outputtopology         outputtopology
	#   define UNITY_patchconstantfunc      patchconstantfunc
	#   define UNITY_outputcontrolpoints    outputcontrolpoints
#endif

#include "GrassShaderHeader.hlsl"

// The structure definition defines which variables it contains.
// This example uses the Attributes structure as an input structure in
// the vertex shader.

// tessellation data
struct TessellationFactors
{
	float edge[3] : SV_TessFactor;
	float inside : SV_InsideTessFactor;
};

ControlPoint TessellationVertexProgram(Attributes v)
{
	ControlPoint p;

	p.positionOS = v.positionOS;
	p.normalOS = v.normalOS;
	p.tangentOS = v.tangentOS;
	p.uv = v.uv;
	p.uvLM = v.uvLM;
	p.color = v.color;

	return p;
}

// info so the GPU knows what to do (triangles) and how to set it up , clockwise, fractional division
// hull takes the original vertices and outputs more
[UNITY_domain("tri")]
[UNITY_outputcontrolpoints(3)]
[UNITY_outputtopology("triangle_cw")]
// [UNITY_partitioning("fractional_odd")]
[UNITY_partitioning("fractional_even")]
//[UNITY_partitioning("pow2")]
//[UNITY_partitioning("integer")]
[UNITY_patchconstantfunc("patchConstantFunction")]
ControlPoint hull(InputPatch<ControlPoint, 3> patch, uint id : SV_OutputControlPointID)
{
	return patch[id];
}

// fade tessellation at a distance
float CalcDistanceTessFactor(float4 vertex, float minDist, float maxDist, float tess)
{
	float3 worldPosition = TransformObjectToWorld(vertex.xyz);
	float dist = distance(worldPosition, _WorldSpaceCameraPos);
	float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
	return (f);
}

// tessellation
TessellationFactors patchConstantFunction(InputPatch<ControlPoint, 3> patch)
{
	// values for distance fading the tessellation
	float minDist = 5.0;
	float maxDist = _MaxTessDistance;

	TessellationFactors f;

	float edge0 = CalcDistanceTessFactor(patch[0].positionOS, minDist, maxDist, _Tess);
	float edge1 = CalcDistanceTessFactor(patch[1].positionOS, minDist, maxDist, _Tess);
	float edge2 = CalcDistanceTessFactor(patch[2].positionOS, minDist, maxDist, _Tess);

	// make sure there are no gaps between different tessellated distances, by averaging the edges out.
	f.edge[0] = (edge1 + edge2) / 2;
	f.edge[1] = (edge2 + edge0) / 2;
	f.edge[2] = (edge0 + edge1) / 2;
	f.inside = (edge0 + edge1 + edge2) / 3;
	return f;
}

[UNITY_domain("tri")]
Varyings domain(TessellationFactors factors, OutputPatch<ControlPoint, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
{
	Attributes v;

	#define Tesselationing(fieldName) v.fieldName = \
	patch[0].fieldName * barycentricCoordinates.x + \
	patch[1].fieldName * barycentricCoordinates.y + \
	patch[2].fieldName * barycentricCoordinates.z;

	Tesselationing(positionOS)
	Tesselationing(normalOS)
	Tesselationing(tangentOS)
	Tesselationing(uv)
	Tesselationing(uvLM)
	Tesselationing(color)

	return LitPassVertex(v);
}







