#ifndef GRASS_SHADER_HEADER
#define GRASS_SHADER_HEADER
struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 uv           : TEXCOORD0;
    float2 uvLM         : TEXCOORD1;
    float4 color : COLOR; 
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct ControlPoint
{
    float4 positionOS   : INTERNALTESSPOS;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 uv           : TEXCOORD0;
    float2 uvLM         : TEXCOORD1;
    float4 color : COLOR; 
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float3 normalOS : NORMAL;
    float2 uv                       : TEXCOORD0;
    float2 uvLM                     : TEXCOORD1;
    float4 positionWSAndFogFactor   : TEXCOORD2; // xyz: positionWS, w: vertex fog factor
    half3  normalWS                 : TEXCOORD3;
    half3 tangentWS                 : TEXCOORD4;
    float4 positionOS : TEXCOORD5;

    float4 color : COLOR;

    #if _NORMALMAP
    half3 bitangentWS               : TEXCOORD5;
    #endif

    #ifdef _MAIN_LIGHT_SHADOWS
    float4 shadowCoord              : TEXCOORD6; // compute shadow coord per-vertex for the main light
    #endif

    float4 positionCS               : SV_POSITION;
};

// Properties
float _Height;
float _Base;
float4 _TopColor;
float4 _BottomColor;

float _YRotRandomness;
float _MaxGrowAngle;

float _WindSpeed;
float _WindStrength;

float _LightPower;
float _Translucency;
float _AlphaCutoff;
float4 _ShadowPower;

float _Tess;
float _MaxTessDistance;

Varyings LitPassVertex(Attributes input);
[maxvertexcount(6)]
void LitPassGeom(triangle Varyings input[3], inout TriangleStream<Varyings> outStream);
half4 LitPassFragment(Varyings input, bool vf : SV_ISFRONTFACE) : SV_TARGET;

float3x3 Rot(float theta, float3 u);
float Rand(float3 co);
float4 TransformWorldToShadowCoords(float3 positionWS);
#endif