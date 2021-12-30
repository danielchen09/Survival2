Shader "Custom/GrassShader"
{
    Properties
    {
        [MainTexture] _BaseMap("Grass Texture", 2D) = "white" {}
        
        [Header(Shape and Color)]
        [Space]
        _Height ("Height", Float) = 1.0
        _Base ("Base", Float) = 1.0
        _TopColor ("Top Color", Color) = (0.5, 0.5, 0.5, 1)
        _BottomColor ("Bottom Color", Color) = (0.5, 0.5, 0.5, 1)
        
        [Header(Orientation)]
        [Space]
        _YRotRandomness ("Random Rotation", Float) = 90
        _MaxGrowAngle("Grow Angle", Range(0, 360)) = 45

        [Header(Wind)]
        [Space]
        _WindSpeed ("Wind Speed", Float) = 1
        _WindStrength ("Wind Strength", Float) = 1

        [Header(Lighting)]
        [Space]
        _LightPower("Light Power", Float) = 0.05
        _Translucency("Translucency", Float) = 0.02
        _AlphaCutoff("Alpha cutoff", Float) = 0.1
        _ShadowPower("Shadow Power", Float) = 0.35

        [Header(Tessellation)]
        [Space]
        _Tess("Tessellation", Range(1, 20)) = 2
        _MaxTessDistance("Max Tesselation Distance", Float) = 6
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100

        
        Pass
        {
            Name "Depth Pass"
            Tags {"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0
            Cull Off

            HLSLPROGRAM

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma multi_compile _ WRITE_NORMAL_BUFFER
            #pragma multi_compile _ WRITE_MSAA_DEPTH
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma multi_compile_instancing

            #pragma require geometry

            #pragma geometry LitPassGeom
            #pragma vertex LitPassVertex
            #pragma fragment DepthPassFragment

            #define SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "GrassPass.hlsl"

            half4 DepthPassFragment(Varyings input) : SV_TARGET
            {
                return 0;
            }

            ENDHLSL
        }

        Pass
        {
            Name "Geometry Pass"
            Tags {"LightMode" = "UniversalForward"}

            ZWrite On
            Cull Off // for two sided planes

            HLSLPROGRAM

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 4.0

            #pragma shader_feature _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _GLOSSYREFLECTIONS_OFF
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _SPECULAR_SETUP
            #pragma shader_feature _RECEIVE_SHADOWS_ON
            //#pragma shader_feature _ _RENDERING_CUTOUT _RENDERING_FADE
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS

            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

            // Allow for use of light map, fog
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog
            
            // tells shader geometry pass should be used
            #pragma require tessellation
            #pragma require geometry

            // modify vertices information
            #pragma vertex TessellationVertexProgram
            // add extra vertices per vertex
            #pragma geometry LitPassGeom
            // writing to the screen
            #pragma fragment LitPassFragment

            #pragma hull hull
            #pragma domain domain

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            
            #include "GrassPass.hlsl"
            #include "CustomTessellation.hlsl"

            ENDHLSL
        }
    }
}
