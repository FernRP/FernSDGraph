Shader "FernRender/URP/FERNNPRHair"
{
    Properties
    {
        [Main(Surface, _, off, off)]
        _group ("Surface", float) = 0
        [Space()]
        [Tex(Surface, _BaseColor)] _BaseMap ("Base Map", 2D) = "white" { }
        [HideInInspector] _BaseColor ("Base Color", color) = (0.5, 0.5, 0.5, 1)
        [SubToggle(Surface, _NORMALMAP)] _BumpMapKeyword("Use Normal Map", Float) = 0.0
        [Tex(Surface_NORMALMAP)] _BumpMap ("Normal Map", 2D) = "bump" { }
        [Sub(Surface_NORMALMAP)] _BumpScale("Scale", Float) = 1.0
        [Tex(Surface)] _LightMap ("PBR Light Map", 2D) = "white" { }
        [Channel(Surface)] _PBRMetallicChannel("Metallic Channel", Vector) = (1,0,0,0)
        [Sub(Surface)] _Metallic("Metallic", Range(0, 1.0)) = 0.0
        [Channel(Surface)] _PBRSmothnessChannel("Smoothness Channel", Vector) = (0,0,0,1)
        [Sub(Surface)] _Smoothness("Smoothness", Range(0, 1.0)) = 0.5 
        [Channel(Surface)] _PBROcclusionChannel("Occlusion Channel", Vector) = (0,1,0,0)
        [Sub(Surface)] _OcclusionStrength("Occlusion Strength", Range(0, 1.0)) = 0.0
        
        [Main(ShadingMap, _, off, off)]
        _groupShadingMask ("Shading Map", float) = 0
        [Space()]
        [Tex(ShadingMap)] _ShadingMap01 ("Shading Mask Map 1", 2D) = "white" { }
        
        [Main(Diffuse, _, off, off)]
        _group1 ("DiffuseSettings", float) = 1
        [Space()]
        [KWEnum(Diffuse, CelShading, _CELLSHADING, RampShading, _RAMPSHADING, PBRShading, _LAMBERTIAN)] _enum_diffuse ("Shading Mode", float) = 2
        [SubToggle(Diffuse)] _UseHalfLambert ("Use HalfLambert (More Flatter)", float) = 0
        [SubToggle(Diffuse)] _UseRadianceOcclusion ("Radiance Occlusion", float) = 0
        [Sub(Diffuse_LAMBERTIAN._CELLSHADING)] [HDR] _HighColor ("Hight Color", Color) = (1,1,1,1)
        [Sub(Diffuse_LAMBERTIAN._CELLSHADING)] _DarkColor ("Dark Color", Color) = (0,0,0,1)
        [Sub(Diffuse_CELLSHADING)] _CELLThreshold ("Cell Threshold", Range(0.01,1)) = 0.5
        [Sub(Diffuse_CELLSHADING)] _CELLSmoothing ("Cell Smoothing", Range(0.001,1)) = 0.001
        [Sub(Diffuse_RAMPSHADING)] _DiffuseRampMap ("Ramp Map", 2D) = "white" {}
        [Sub(Diffuse_RAMPSHADING)] _RampMapUOffset ("Ramp Map U Offset", Range(-1,1)) = 0
        [Sub(Diffuse_RAMPSHADING)] _RampMapVOffset ("Ramp Map V Offset", Range(0,1)) = 0.5
        
        [Main(Specular, _, off, off)]
        _groupSpecular ("SpecularSettings", float) = 1
        [Space()]
        [KWEnum(Specular, None, _, Anisotropy, _KAJIYAHAIR, AngleRing, _ANGLERING, PBR_GGX, _GGX, Stylized, _STYLIZED)] _enum_specular ("Shading Mode", float) = 0
        [SubToggle(Specular._GGX._STYLIZED._BLINNPHONG._KAJIYAHAIR._ANGLERING, _SPECULARMASK)] _SpecularMask("Use Specular Mask", Float) = 0.0
        [Channel(Specular._SPECULARMASK)] _SpecularIntensityChannel("Specular Intensity Channel", Vector) = (1,0,0,0)
        [Sub(Specular._GGX._STYLIZED._BLINNPHONG._KAJIYAHAIR)][HDR] _SpecularColor ("Specular Color", Color) = (1,1,1,1)
        [Sub(Specular._STYLIZED)] _StylizedSpecularSize ("Stylized Specular Size", Range(0,1)) = 0.1
        [Sub(Specular._STYLIZED)] _StylizedSpecularSoftness ("Stylized Specular Softness", Range(0.001,1)) = 0.05
        [Sub(Specular._STYLIZED)] _StylizedSpecularAlbedoWeight ("Specular Color Albedo Weight", Range(0,1)) = 0
        [Sub(Specular._BLINNPHONG)] _Shininess ("BlinnPhong Shininess", Range(0,1)) = 1
        [Tex(Specular._KAJIYAHAIR)] _AnisoShiftMap ("Aniso Shift Map", 2D) = "white" {}
        [Sub(Specular._KAJIYAHAIR)] _AnisoShiftScale ("Aniso Shift Scale", Range(1, 50)) = 10
        [Sub(Specular._KAJIYAHAIR)] _AnisoSpecularColor("Aniso Specular Color Layer1", Color) = (1,1,1,1)
        [Sub(Specular._KAJIYAHAIR)] _AnisoSpread1("Aniso Specular Spread Layer1", Range(-1,1)) = 0.0
        [Sub(Specular._KAJIYAHAIR)] _AnsioSpeularShift("Aniso Specular Shift Layer1", Range(-3,3)) = 1.0
        [Sub(Specular._KAJIYAHAIR)] _AnsioSpeularStrength("Aniso Specular Strength Layer1", Range(0, 64)) = 1.0
        [Sub(Specular._KAJIYAHAIR)] _AnsioSpeularExponent("Aniso Specular Exponent Layer1", Range(1,1024)) = 1.0
        [Sub(Specular._KAJIYAHAIR)] _AnisoSecondarySpecularColor("Aniso Specular Color Layer2", Color) = (0.5,0.5,0.5,1)
        [Sub(Specular._KAJIYAHAIR)] _AnisoSpread2("Aniso Specular Spread Layer2", Range(-1,1)) = 0.0
        [Sub(Specular._KAJIYAHAIR)] _AnsioSecondarySpeularShift("Aniso Specular Shift Layer2", Range(-3,3)) = 1.0
        [Sub(Specular._KAJIYAHAIR)] _AnsioSecondarySpeularStrength("Aniso Specular Strength Layer2", Range(0, 64)) = 1.0
        [Sub(Specular._KAJIYAHAIR)] _AnsioSecondarySpeularExponent("Aniso Specular Exponent Layer2",Range(1,1024)) = 1.0
        [Sub(Specular._ANGLERING)] [HDR]_AngleRingBrightColor ("Specular Bright Color", Color) = (1,1,1,1)
        [Sub(Specular._ANGLERING)] _AngleRingShadowColor ("Specular Shadow Color", Color) = (1,1,1,1)
        [Sub(Specular._ANGLERING)] _AngleRingWidth ("Specular Width", Range(0,1)) = 0
        [Sub(Specular._ANGLERING)] _AngleRingIntensity ("Angle Ring Intensity", Range(0,1)) = 0
        [Sub(Specular._ANGLERING)] _AngleRingThreshold ("Shadow Threshold", Range(0,1)) = 0
        [Sub(Specular._ANGLERING)] _AngleRingSoftness ("Shadow Softness", Range(0,1)) = 0
        
        [Main(Environment, _, off, off)]
        _groupEnvironment ("EnvironmentSettings", float) = 1
        [Space()]
        [KWEnum(Environment, None, _, RenderSetting, _RENDERENVSETTING, CustomCube, _CUSTOMENVCUBE)] _enum_env ("Environment Source", float) = 1
        
        [Main(EmssionSetting, _, off, off)]
        _groupEmission ("Emission Setting", float) = 0
        [Space()]
        [SubToggle(EmssionSetting, _USEEMISSIONTEX)] _UseEmissionTex("Use Emission Tex", Float) = 0.0
        [Tex(EmssionSetting._USEEMISSIONTEX)] _EmissionTex ("Emission Tex", 2D) = "white" { }
        [Channel(EmssionSetting)] _EmissionChannel("Emission Channel", Vector) = (0,0,1,0)
        [Sub(EmssionSetting)] [HDR]_EmissionColor("Emission Color", Color) = (0,0,0,0)
        [Sub(EmssionSetting)] _EmissionColorAlbedoWeight("Emission Color Albedo Weight", Range(0, 1)) = 0
        
        [Main(Rim, _, off, off)]
        _groupRim ("RimSettings", float) = 1
        [Space()]
        [KWEnum(Rim, None, _, FresnelRim, _FRESNELRIM)] _enum_rim ("Rim Mode", float) = 0
        [Sub(Rim._FRESNELRIM)] _RimDirectionLightContribution("Directional Light Contribution", Range(0,1)) = 1.0
        [Sub(Rim._FRESNELRIM)][HDR] _RimColor("Rim Color",Color) = (1,1,1,1)
        [Sub(Rim._FRESNELRIM)] _RimThreshold("Rim Threshold",Range(0,1)) = 0.2
        [Sub(Rim._FRESNELRIM)] _RimSoftness("Rim Softness",Range(0.001,1)) = 0.01
        
        [Main(ShadowSetting, _, off, off)]
        _groupShadowSetting ("Shading Map", float) = 0
        [Space()]
        [SubToggleOff(ShadowSetting, _RECEIVE_SHADOWS_OFF)] _RECEIVE_SHADOWS_OFF("RECEIVE_SHADOWS", Float) = 1
        
        [Main(AdditionalLightSetting, _, off, off)]
        _groupAdditionLight ("AdditionalLightSetting", float) = 1
        [Space()]
        [SubToggle(AdditionalLightSetting)] _Is_Filter_LightColor("Is Filter LightColor", Float) = 1
        [Sub(AdditionalLightSetting)] _LightIntensityClamp("Additional Light Intensity Clamp", Range(0, 8)) = 1
        
        [Main(Outline, _, off, off)]
        _groupOutline ("OutlineSettings", float) = 1
        [Space()]
        [SubToggle(Outline, _OUTLINE)] _Outline("Use Outline", Float) = 0.0
        [Sub(Outline._OUTLINE)] _OutlineColor ("Outline Color", Color) = (0,0,0,0)
        [Sub(Outline._OUTLINE)] _OutlineWidth ("Outline Width", Range(0, 10)) = 1
        [KWEnum(Outline, None, _, UV8.RG, _SMOOTHEDNORMAL)] _enum_outline_smoothed("Smoothed Normal", float) = 0
        [KWEnum(Outline, None, _, VertexColor.A, _OUTLINEWIDTHWITHVERTEXTCOLORA, UV8.A, _OUTLINEWIDTHWITHUV8A)] _enum_outline_width("Override Outline Width", float) = 0
        [KWEnum(Outline, None, _, BaseMap, _OUTLINECOLORBLENDBASEMAP, VertexColor, _OUTLINECOLORBLENDVERTEXCOLOR)] _enum_outline_color("Blend Outline Color", float) = 0

        // AI Core has no release
        [Main(AISetting, _, off, off)]
        _groupAI ("AISetting", float) = 1
        [Space()]
        [SubToggle(AISetting)] _Is_SDInPaint("Is InPaint", Float) = 0
        [SubToggle(AISetting)] _ClearShading("Clear Shading", Float) = 0
        
        //Effect is in Developing
//        [Title(_, Effect)]
//        [Main(DissolveSetting, _, off, off)]
//        _groupDissolveSetting ("Dissolve Setting", float) = 0
//        [Space()]
//        [SubToggle(DissolveSetting, _USEDISSOLVEEFFECT)] _UseDissolveEffect("Use Dissolve Effect", Float) = 0.0
//        [Tex(DissolveSetting._USEDISSOLVEEFFECT)] _DissolveNoiseTex ("Dissolve Noise Tex", 2D) = "white" { }
//        [Sub(DissolveSetting)] _DissolveThreshold ("Dissolve Threshold", Range(0, 1)) = 0

        // RenderSetting
        [Title(_, RenderSetting)]
        [Main(RenderSetting, _, off, off)]
        _groupSurface ("RenderSetting", float) = 1
        [Surface(RenderSetting)] _Surface("Surface Type", Float) = 0.0
        [SubEnum(RenderSetting, UnityEngine.Rendering.CullMode)] _Cull("Cull Mode", Float) = 2.0
        [SubEnum(RenderSetting, UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Alpha", Float) = 1.0
        [SubEnum(RenderSetting, UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Alpha", Float) = 0.0
        [SubEnum(RenderSetting, Off, 0, On, 1)] _ZWrite("Z Write", Float) = 1.0
        [SubEnum(RenderSetting, Off, 0, On, 1)] _DepthPrePass("Depth PrePass", Float) = 0
        [SubEnum(RenderSetting, Off, 0, On, 1)] _CasterShadow("Caster Shadow", Float) = 1
        [Sub(RenderSetting)]_Cutoff("Alpha Clipping", Range(0.0, 1.0)) = 0.5
        [Sub(RenderSetting)]_ZOffset("Z Offset", Range(-1.0, 1.0)) = 0
        [Queue(RenderSetting)] _QueueOffset("Queue offset", Range(-50, 50)) = 0.0
    }

    SubShader
    {
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}
        LOD 300
        
        Pass
        {
            Name "FernDepthPrePass"
            Tags{"LightMode" = "SRPDefaultUnlit"} // Hard Code Now

            Blend Off
            ZWrite on
            Cull off
            ColorMask 0

            HLSLPROGRAM
            #pragma only_renderers gles gles3 glcore d3d11
            #pragma target 3.0

            // -------------------------------------
            // Fern Keywords
            #pragma shader_feature_local_vertex _PERSPECTIVEREMOVE

            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment_DepthPrePass

            #include "NPRStandardInput.hlsl"
            #include "NPRStandardForwardPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Cull[_Cull]

            HLSLPROGRAM
            #pragma only_renderers gles gles3 glcore d3d11
            #pragma target 3.0

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _LAMBERTIAN _CELLSHADING _RAMPSHADING
            #pragma shader_feature_local _ _KAJIYAHAIR _ANGLERING _GGX _STYLIZED 
            #pragma shader_feature_local _SPECULARMASK
            #pragma shader_feature_local _ _FRESNELRIM
            #pragma shader_feature_local _ _RENDERENVSETTING _CUSTOMENVCUBE

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF
            #pragma shader_feature_local _DEPTHSHADOW
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _LIGHT_LAYERS
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _CLUSTERED_RENDERING

            // -------------------------------------
            // Fern Keywords
            #pragma shader_feature_local_vertex _PERSPECTIVEREMOVE

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma multi_compile_fragment _ DEBUG_DISPLAY

            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            #include "NPRStandardInput.hlsl"
            #include "NPRStandardForwardPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma only_renderers gles gles3 glcore d3d11
            #pragma target 3.0

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

             // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON

            // -------------------------------------
            // Universal Pipeline keywords

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "NPRStandardInput.hlsl"
            #include "../ShaderLibrary/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma only_renderers gles gles3 glcore d3d11
            #pragma target 3.0

            // -------------------------------------
            // Fern Keywords
            #pragma shader_feature_local_vertex _PERSPECTIVEREMOVE

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            #include "NPRStandardInput.hlsl"
            #include "../ShaderLibrary/DepthOnlyPass.hlsl"
            ENDHLSL
        }
        
         Pass
        {
            Name "DepthShadowOnly"
            Tags{"LightMode" = "DepthShadowOnly"}

            ZWrite On
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma only_renderers gles gles3 glcore d3d11
            #pragma target 3.0

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            // -------------------------------------
            // Fern Keywords
            #pragma shader_feature_local_vertex _PERSPECTIVEREMOVE

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            #include "NPRStandardInput.hlsl"
            #include "../ShaderLibrary/DepthOnlyPass.hlsl"
            ENDHLSL
        }


        // This pass is used when drawing to a _CameraNormalsTexture texture
        Pass
        {
            Name "DepthNormals"
            Tags{"LightMode" = "DepthNormals"}

            ZWrite On
            Cull[_Cull]

            HLSLPROGRAM
            #pragma only_renderers gles gles3 glcore d3d11
            #pragma target 3.0

            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _PARALLAXMAP
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // -------------------------------------
            // Fern Keywords
            #pragma shader_feature_local_vertex _PERSPECTIVEREMOVE

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #include "NPRStandardInput.hlsl"
            #include "NPRDepthNormalsPass.hlsl"
            ENDHLSL
        }
        
        // Normal Outline
        Pass
        {
            Name "OutLine"
            Tags {"LightMode" = "Outline" }
            Cull Front
            ZWrite [_ZWrite]
            ZTest LEqual
            Blend [_SrcBlend] [_DstBlend]
            Offset 1, 1

            HLSLPROGRAM
            
            #pragma shader_feature_local _OUTLINE
            
            // -------------------------------------
            // Fern Keywords
            #pragma shader_feature_local_vertex _PERSPECTIVEREMOVE
            #pragma shader_feature_local_vertex _SMOOTHEDNORMAL
            #pragma shader_feature_local_vertex _OUTLINEWIDTHWITHVERTEXTCOLORA _OUTLINEWIDTHWITHUV8A
            #pragma shader_feature_local_fragment _OUTLINECOLORBLENDBASEMAP _OUTLINECOLORBLENDVERTEXCOLOR
            
            #pragma vertex NormalOutLineVertex
            #pragma fragment NormalOutlineFragment

            #include "NPRStandardInput.hlsl"
            #include "../ShaderLibrary/NormalOutline.hlsl"
            ENDHLSL
        }
        
                
        Pass
        {
            Name "InPaint"
            Tags{"LightMode" = "InPaint"}

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Cull[_Cull]

            HLSLPROGRAM
            #pragma only_renderers gles gles3 glcore d3d11
            #pragma target 3.0

            // -------------------------------------
            // Material Keywords
            
            // -------------------------------------
            // Universal Pipeline keywords

            // -------------------------------------
            // Unity defined keywords

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma vertex LitPassVertex
            #pragma fragment InPaintPassFragment

            #include "NPRStandardInput.hlsl"
            #include "NPRStandardForwardPass.hlsl"

            void InPaintPassFragment(
                Varyings input
                , out half4 outColor : SV_Target0
            #ifdef _WRITE_RENDERING_LAYERS
                , out float4 outRenderingLayers : SV_Target1
            #endif
            )
            {
                outColor = lerp(0, 1, _Is_SDInPaint);
            }

            ENDHLSL
        }

    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "LWGUI.LWGUI"
}