Shader "Lux URP/Fast Outline Double Pass"
{
    Properties
    {
        [HeaderHelpLuxURP_URL(gpukpasbzt01)]

        [Header(Stencil Pass)]
        [Space(8)]
        [Enum(UnityEngine.Rendering.CompareFunction)] _SPZTest ("ZTest", Int) = 4
        [Enum(UnityEngine.Rendering.CullMode)] _SPCull ("Culling", Float) = 2

        [Header(Outline Pass)]
        [Space(8)]
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Int) = 4
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Culling", Float) = 2

        [Header(Shared Stencil Settings)]
        [Space(8)]
        [IntRange] _StencilRef ("Stencil Reference", Range (0, 255)) = 0
        [IntRange] _ReadMask ("     Read Mask", Range (0, 255)) = 255
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilCompare ("Stencil Comparison", Int) = 6


        [Header(Outline)]
        [Space(8)]
        _OutlineColor ("Color", Color) = (1,1,1,1)
        _Border ("Width", Float) = 3
        _UseScale ("Use Scale", Integer) = 0

        [Space(5)]
        [Toggle(_APPLYFOG)] _ApplyFog("Enable Fog", Float) = 0.0
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType"="Opaque"
            "IgnoreProjector" = "True"
            "Queue"= "Transparent+60" // +59 smalltest to get drawn on top of transparents
        }


        //  First pass which only prepares the stencil buffer

        Pass
        {
            Tags
            {
                //"Queue"= "Transparent+59"
            }

            Name "Unlit"
            Stencil
            {
                Ref [_StencilRef]
                ReadMask [_ReadMask]
                Comp Always
                Pass Replace
            }

            Cull [_SPCull]
            ZTest [_SPZTest]
            //  Make sure we do not get overridden
            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Universal Pipeline keywords

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"


            #pragma vertex vert
            #pragma fragment frag

            // Lighting include is needed because of GI
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            //#include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"

            #if defined(LOD_FADE_CROSSFADE)
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                half _Border;
                half _UseScale;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };


            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                #if defined(_APPLYFOG)
                    half fogCoord : TEXCOORD0;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return o;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                #ifdef LOD_FADE_CROSSFADE
                    LODFadeCrossFade(input.positionCS);
                #endif

                return 0;
            }
            ENDHLSL
        }

        //  Second pass which draws the outline

        Pass
        {

            Name "ForwardLit"
            Tags
            {
                "LightMode" = "UniversalForwardOnly"
                //"Queue"= "Transparent+60"
            }

            Stencil
            {
                Ref [_StencilRef]
                ReadMask [_ReadMask]
                Comp [_StencilCompare]
                Pass Keep
            }

            Blend SrcAlpha OneMinusSrcAlpha
            Cull [_Cull]
            ZTest [_ZTest]
            //  Make sure we do not get overridden
            ZWrite On

            HLSLPROGRAM
            #pragma target 2.0

            #pragma shader_feature_local _APPLYFOG

            // -------------------------------------
            // Universal Pipeline keywords

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fog
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"


            #pragma vertex vert
            #pragma fragment frag

            // Lighting include is needed because of GI
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            //#include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"

            #if defined(LOD_FADE_CROSSFADE)
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                half _Border;
                half _UseScale;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };


            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                #if defined(_APPLYFOG)
                    half fogCoord   : TEXCOORD0;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                // Transform vertex position from object space to world space
                float3 worldPos = _UseScale == 0
                          ? TransformObjectToWorld(input.positionOS.xyz)
                          : TransformObjectToWorld(input.positionOS.xyz * _Border);
        
                // Transform vertex position to clip space
                output.positionCS = _UseScale == 0 ? TransformObjectToHClip(input.positionOS.xyz) 
                          : TransformObjectToHClip(input.positionOS.xyz * _Border);
        
                // Calculate distance from camera to vertex
                float3 cameraPos = GetCameraPositionWS();
                float distance = length(worldPos - cameraPos);
        
                // Adjust _Border based on distance to maintain consistent size
                float adjustedBorder = _Border / distance;
        
                // Apply outline extrusion if _Border > 0
                if (adjustedBorder > 0.0h && _UseScale == 0)
                {
                    float3 normal = mul(GetWorldToHClipMatrix(),mul(GetObjectToWorldMatrix(), float4(input.normalOS, 0.0))).xyz;
                    float2 offset = normalize(normal.xy);
                    float2 ndc = _ScreenParams.xy * 0.5;
                    output.positionCS.xy += ((offset * adjustedBorder) / ndc * output.positionCS.w);
                }
        
                // Compute fog factor if enabled
                #if defined(_APPLYFOG)
        output.fogCoord = ComputeFogFactor(output.positionCS.z);
                #endif
        
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Use the base color defined in the material
                half4 color = _OutlineColor;

                // Return the color as the output for this pixel
                return color;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}