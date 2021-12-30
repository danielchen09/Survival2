#include "GrassShaderHeader.hlsl"

// Vertex pass
Varyings LitPassVertex(Attributes input) 
{
    Varyings output;

    output.color = input.color;

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS); // object -> screen, world space
    VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS); // transforming into tangent space

    float fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

    output.uv = TRANSFORM_TEX(input.uv, _BaseMap); // using the tiling and offset of basemap
    output.uvLM = input.uvLM.xy * unity_LightmapST.xy + unity_LightmapST.zw; // lightmap coordinate

    output.positionWSAndFogFactor = float4(vertexInput.positionWS, fogFactor);
    output.positionCS = vertexInput.positionCS;
    output.positionOS = input.positionOS;

    output.normalWS = vertexNormalInput.normalWS;
    output.tangentWS = vertexNormalInput.tangentWS;

    #ifdef _NORMALMAP
    output.bitangentWS = vertexNormalInput.bitangentWS;
    #endif

    #ifdef _MAIN_LIGHT_SHADOWS
    output.shadowCoord = GetShadowCoord(vertexInput);
    #endif

    return output;
}

// Geometry pass
[maxvertexcount(6)] // for each triangle, output at most 6 vertex
void LitPassGeom(triangle Varyings input[3], inout TriangleStream<Varyings> outStream)
{
    if (dot(input[0].normalWS, float3(0, 1, 0)) <= cos(_MaxGrowAngle * PI / 180)) {
        return;
    }
    // The output geometry will be on the middle of the input triangle
    float3 basePos = (input[0].positionWSAndFogFactor.xyz + input[1].positionWSAndFogFactor.xyz + input[2].positionWSAndFogFactor.xyz) / 3;

    float3 rotatedTangent = normalize(mul(input[0].tangentWS, Rot(Rand(input[0].positionWSAndFogFactor.xyz) * _YRotRandomness, float3(0, 1, 0))));

    // Creating a quad
    float3 quadNormal = normalize(cross(rotatedTangent, input[0].normalWS));
    float3 wind1 = float3(sin(_Time.x * _WindSpeed + basePos.x) + sin(_Time.x * _WindSpeed + basePos.z * 2) + sin(_Time.x * _WindSpeed * 0.1 + basePos.x), 0,
            cos(_Time.x * _WindSpeed + basePos.x * 2) + cos(_Time.x * _WindSpeed + basePos.z));
    wind1 *= _WindStrength;

    Varyings outputBottomLeft = input[0];
    outputBottomLeft.positionCS = TransformWorldToHClip(basePos - rotatedTangent * _Base);
    outputBottomLeft.uv = TRANSFORM_TEX(float2(0, 0), _BaseMap);
    outputBottomLeft.normalWS = quadNormal;

    Varyings outputBottomRight = input[0];
    outputBottomRight.positionCS = TransformWorldToHClip(basePos + rotatedTangent * _Base);
    outputBottomRight.uv = TRANSFORM_TEX(float2(1, 0), _BaseMap);
    outputBottomRight.normalWS = quadNormal;

    Varyings outputTopLeft = input[0];
    outputTopLeft.positionCS = TransformWorldToHClip(basePos - rotatedTangent * _Base + input[0].normalWS * _Height + wind1);
    outputTopLeft.uv = TRANSFORM_TEX(float2(0, 1), _BaseMap);
    outputTopLeft.normalWS = quadNormal;

    Varyings outputTopRight = input[0];
    outputTopRight.positionCS = TransformWorldToHClip(basePos + rotatedTangent * _Base + input[0].normalWS * _Height + wind1);
    outputTopRight.uv = TRANSFORM_TEX(float2(1, 1), _BaseMap);
    outputTopRight.normalWS = quadNormal;

    outStream.Append(outputTopLeft);
    outStream.Append(outputTopRight);
    outStream.Append(outputBottomLeft);
    outStream.RestartStrip();

    outStream.Append(outputTopRight);
    outStream.Append(outputBottomRight);
    outStream.Append(outputBottomLeft);
    outStream.RestartStrip();
}

half4 LitPassFragment(Varyings input, bool vf : SV_ISFRONTFACE) : SV_TARGET
{
    half3 normalWS = input.normalWS;
    normalWS = normalize(normalWS) * (vf ? 1 : -1);

    float3 positionWS = input.positionWSAndFogFactor.xyz;
    
    half3 color = (0, 0, 0);

    Light mainLight;
    mainLight = GetMainLight(TransformWorldToShadowCoords(positionWS));

    float3 normalLight = LightingLambert(mainLight.color, mainLight.direction, normalWS) * _LightPower;
    float3 inverseNormalLight = LightingLambert(mainLight.color, mainLight.direction, -normalWS) * _Translucency;
    color = input.color * _TopColor + normalLight + inverseNormalLight;
    color = lerp(color * _BottomColor.r, color, input.uv.y);
    color = lerp(color * _BottomColor.r, color, saturate(mainLight.shadowAttenuation + _ShadowPower));
    color = MixFog(color, input.positionWSAndFogFactor.w);

    float a = _BaseMap.Sample(sampler_BaseMap, input.uv).a;
    clip(a - _AlphaCutoff); // dont render if alpha is less than alphacutoff

    return half4(color, 1);
}

float Rand(float3 co)
{
    return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 53.539))) * 43758.5453);
}

// Rotation of degree theta around axis u
float3x3 Rot(float theta, float3 u) 
{
    float c, s;
    sincos(theta, c, s);
    return float3x3(
        c + u.x * u.x * (1 - c), u.x * u.y * (1 - c) - u.z * s, u.x * u.z * (1 - c) + u.y * s,
        u.y * u.x * (1 - c) + u.z * s, c + u.y * u.y * (1 - c), u.y * u.z * (1 - c) - u.x * s,
        u.z * u.x * (1 - c) - u.y * s, u.z * u.y * (1 - c) + u.x * s, c + u.z * u.z * (1 - c)
    );
}

float4 TransformWorldToShadowCoords(float3 positionWS) 
{
    half cascadeIndex = ComputeCascadeIndex(positionWS);
    return mul(_MainLightWorldToShadow[cascadeIndex], float4(positionWS, 1.0));
}