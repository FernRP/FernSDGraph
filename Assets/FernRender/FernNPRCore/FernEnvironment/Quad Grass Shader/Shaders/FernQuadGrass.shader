Shader "FernRender/Environment/Quad Grass"
{
	Properties
	{
		[Main(Surface, _, off, off)] 
		_group_Surface("Surface", float) = 0
		[Space()]
		[Tex(Surface, _BaseColor)][MainTexture] _BaseMap("Albedo", 2D) = "white" {}
		[HideInInspector][MainColor] _BaseColor("Color", Color) = (0.49, 0.89, 0.12, 1.0)
		[SubToggle(Surface)] _AlphaToCoverage("Alpha to coverage", Float) = 0.0
		[Sub(Surface)]_HueVariation("Hue Variation (Alpha = Intensity)", Color) = (1, 0.63, 0, 0.15)
		[Sub(Surface)]_ColorMapStrength("Colormap Strength", Range(0.0, 1.0)) = 0.0
		[Sub(Surface)][HDR] _EmissionColor("Emission Color", Color) = (0,0,0)
		[Sub(Surface)] _VertexDarkening("Random Darkening", Range(0, 1)) = 0.1

		_ColorMapHeight("Colormap Height", Range(0.0, 1.0)) = 1.0
		_ScalemapInfluence("Scale influence", vector) = (0,1,0,0)
		
		[Main(PBR, _, off, off)] 
		_group_PBR("Lighting", float) = 0
		[Space()]
		[SubEnum(PBR, Unlit,0,Simple,1,Advanced,2)]_LightingMode("Lighting Mode", Float) = 2.0
		[Sub(PBR)]_OcclusionStrength("Ambient Occlusion", Range(0.0, 1.0)) = 0.25
		[Sub(PBR)]_Smoothness("Smoothness", Range(0.0, 1.0)) = 0.0
		
		
		[Main(Translucency, _, off, off)] 
		_group_TRANSLUCENCY("Translucency", float) = 0
		[Space()]
		[Sub(Translucency)]_TranslucencyDirect("Translucency (Direct)", Range(0.0, 1.0)) = 1
		[Sub(Translucency)]_TranslucencyIndirect("Translucency (Indirect)", Range(0.0, 1.0)) = 0.0
		[Sub(Translucency)]_TranslucencyFalloff("Translucency Falloff", Range(1.0, 8.0)) = 4.0
		[Sub(Translucency)]_TranslucencyOffset("Translucency Offset", Range(0.0, 1.0)) = 0.0
		
		
		[Main(Normal, _, off, off)] 
		_group_NORMAL("Normal", float) = 0
		[Space()]
		[Sub(Normal)]_UpNormal("Up Normal Weight",Range(0.0, 1.0)) = 1.0
		[Sub(Normal)]_NormalSpherify("Normal Spherifying",Range(0.0, 1.0)) = 0.0
		[Sub(Normal)]_NormalSpherifyMask("Normal Spherifying (tip mask)",Range(0.0, 1.0)) = 0.0
		[Sub(Normal)]_NormalFlattenDepthNormals("Normal Spherifying (DepthNormals pass)",Range(0.0, 1.0)) = 0.0
		[Sub(Normal)]_BumpScale("Normal Map Strength",Range(0.0, 1.0)) = 1.0
		[Tex(Normal)]_BumpMap("Normal Map", 2D) = "bump" {}
		[Sub(Normal)]_BendPushStrength("Push Strength (XZ)", Range(0.0, 1.0)) = 1.0
		
		[Main(Blend, _, off, off)] 
		_group_BLEND("Blend", float) = 0
		[Space()]
		[SubEnum(Blend, Per Vertex,0,Uniform,1)] _BendMode("Bend Mode", Float) = 0.0
		[Sub(Blend)]_BendFlattenStrength("Flatten Strength (Y)", Range(0.0, 1.0)) = 1.0
		[Sub(Blend)]_BendTint("Bending tint", Color) = (0.8, 0.8, 0.8, 1.0)
		[Sub(Blend)]_PerspectiveCorrection("Perspective Correction", Range(0.0, 1.0)) = 1.0

		[Main(Wind, _, off, off)] 
		_group_WIND("Wind", float) = 0
		[Space()]
		[Sub(Wind)]_WindAmbientStrength("Ambient Strength", Range(0.0, 1.0)) = 0.2
		[Sub(Wind)]_WindSpeed("Ambient Speed", Float) = 3.0
		[Sub(Wind)]_WindDirection("Direction", vector) = (1,0,0,0)
		[Sub(Wind)]_WindVertexRand("Vertex randomization", Range(0.0, 1.0)) = 0.6
		[Sub(Wind)]_WindObjectRand("Object randomization", Range(0.0, 1.0)) = 0.5
		[Sub(Wind)]_WindRandStrength("Random per-object strength", Range(0.0, 1.0)) = 0.5
		[Sub(Wind)]_WindSwinging("Swinging", Range(0.0, 1.0)) = 0.15
		[Sub(Wind)]_WindGustStrength("Gusting strength", Range(0.0, 1.0)) = 0.2
		[Sub(Wind)]_WindGustFreq("Gusting frequency", Range(0.0, 10.0)) = 4
		[Sub(Wind)]_WindGustSpeed("Gusting Speed", Float) = 4
		[Tex(Wind)]_WindMap("Wind map", 2D) = "black" {}
		[Sub(Wind)]_WindGustTint("Max Gusting tint", Range(0.0, 3.0)) = 1

		[Main(Fade, _, off, off)] 
		_group_FADE("Fade", float) = 0
		[Space()]
		[Sub(Fade)]_FadeNear("Near", vector) = (0.25, 0.5, 0, 0)
		[Sub(Fade)]_FadeFar("Far", vector) = (50, 100, 0, 0)
		[Sub(Fade)]_FadeAngleThreshold("Angle fading threshold", Range(0.0, 90.0)) = 15
		
		[Main(KeywordState, _, off, off)] 
		_group_KEYWORD("KeywordState", float) = 0 
		[Space()]
		[SubToggle(KeywordState, _SCALEMAP)] _Scalemap("Scale grass by scalemap", Float) = 0.0
		[SubToggle(KeywordState, _BILLBOARD)] _Billboard("Billboard", Float) = 0.0
		[SubToggleOff(KeywordState, _RECEIVE_SHADOWS_OFF)] _ReceiveShadows("Receive Shadows", Float) = 1.0
		[SubToggleOff(KeywordState, _SPECULARHIGHLIGHTS_OFF)] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[SubToggleOff(KeywordState, _ENVIRONMENTREFLECTIONS_OFF)] _EnvironmentReflections("Environment Reflections", Float) = 1.0
		[SubToggle(KeywordState, _FADING)] _FadingOn("Distance/Angle Fading", Float) = 0.0
		[Sub(KeywordState)]_LODDebugColor ("LOD Debug color", Color) = (1,1,1,1)
		
        [Main(RenderSetting, _, off, off)]
        _groupSurface ("RenderSetting", float) = 1
        [Surface(RenderSetting)] _Surface("Surface Type", Float) = 0.0
        [SubEnum(RenderSetting, UnityEngine.Rendering.CullMode)] _Cull("Cull Mode", Float) = 2.0
        [SubEnum(RenderSetting, UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Alpha", Float) = 1.0
        [SubEnum(RenderSetting, UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Alpha", Float) = 0.0
        [SubEnum(RenderSetting, Off, 0, On, 1)] _ZWrite("Z Write", Float) = 1.0
        [SubEnum(RenderSetting, Off, 0, On, 1)] _CasterShadow("Caster Shadow", Float) = 1
        [Sub(RenderSetting)]_Cutoff("Alpha Clipping", Range(0.0, 1.0)) = 0.5
        [Sub(RenderSetting)]_ZOffset("Z Offset", Range(-1.0, 1.0)) = 0
        [Queue(RenderSetting)] _QueueOffset("Queue offset", Range(-50, 50)) = 0.0
		
		//Required for DOTS Instancing (aka Hybrid Renderer v2)
		[HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
		[HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
		[HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
	}

	SubShader
	{
		Tags{
			"RenderType" = "Opaque"
			"Queue" = "AlphaTest"
			"RenderPipeline" = "UniversalPipeline"
			"IgnoreProjector" = "True"
			"NatureRendererInstancing" = "True"
		}
		
		HLSLINCLUDE
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
		#define REQUIRES_WORLD_SPACE_POS_INTERPOLATOR

		//Hard coded features
		#define _ALPHATEST_ON

		//Uncomment to compile out these calculations
		//#define DISABLE_WIND
		//#define DISABLE_BENDING

		//Change this to 4.5 to add support for the Hybrid Renderer. Doing so removes WebGL support!
		#pragma target 3.5

		// GPU Instancing
		#pragma multi_compile_instancing
		#pragma multi_compile _ DOTS_INSTANCING_ON //Hybrid Renderer v2

		/* start CurvedWorld */
		//#define CURVEDWORLD_BEND_TYPE_CLASSICRUNNER_X_POSITIVE
		//#define CURVEDWORLD_BEND_ID_1
		//#pragma shader_feature_local CURVEDWORLD_DISABLED_ON
		//#pragma shader_feature_local CURVEDWORLD_NORMAL_TRANSFORMATION_ON
		//#include "Assets/Amazing Assets/Curved World/Shaders/Core/CurvedWorldTransform.cginc"
		/* end CurvedWorld */

		//* start VegetationStudio */
		#include "Libraries/VS_InstancedIndirect.cginc"
		#pragma instancing_options assumeuniformscaling renderinglayer procedural:setup
		/* end VegetationStudio */

		/* include GPUInstancer */
//		#include "GPUInstancer/Shaders/Include/GPUInstancerInclude.cginc"
		/* start GPUInstancer */
//		#pragma instancing_options procedural:setupGPUI
		/* end GPUInstancer */

		/* include NatureRenderer */
//		#include "Assets/Visual Design Cafe/Nature Shaders/Common/Nodes/Integrations/Nature Renderer.cginc"
		/* start NatureRenderer */
//		#pragma instancing_options assumeuniformscaling procedural:SetupNatureRenderer
		/* end NatureRenderer */
		
		ENDHLSL

		// ------------------------------------------------------------------
		//  Forward pass. Shades all light in a single pass. GI + emission + Fog
		Pass
		{
			Name "ForwardLit"
			Tags{ "LightMode" = "UniversalForward" }

			AlphaToMask [_AlphaToCoverage]
			Blend [_SrcBlend] [_DstBlend], One Zero
			Cull [_Cull]
			ZWrite [_ZWrite]

			HLSLPROGRAM

			//In place for projects that use a custom RP or modified URP and require specific behaviour for vegetation
			#define VEGETATION_SHADER

			// -------------------------------------
			// Material Keywords
			#pragma multi_compile _ LOD_FADE_CROSSFADE //Note: Cannot use _fragment suffix, unity_LODFade conflicts with unity_RenderingLayer in cBuffer somehow
			#pragma shader_feature_local_vertex _SCALEMAP
			#pragma shader_feature_local_vertex _BILLBOARD
			#pragma shader_feature_local_fragment _FADING
			#pragma shader_feature_local _NORMALMAP
			#pragma shader_feature_local _ _SIMPLE_LIGHTING _ADVANCED_LIGHTING
			#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
			#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
			
			// -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS	
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

			#if !_SIMPLE_LIGHTING && !_ADVANCED_LIGHTING
			#define _UNLIT
			#undef _NORMALMAP
			#endif
			
			// -------------------------------------
			// Unity defined keywords
			//#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile_fog

			//VVV Stripped on older URP versions VVV

			//URP 10+
			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK 
			#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION 
			
			//URP 12+
			//#pragma shader_feature_local_fragment _DISABLE_DECALS
			#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3 
            #pragma multi_compile_fragment _ _LIGHT_LAYERS 
			#pragma multi_compile_fragment _ _LIGHT_COOKIES 
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE //Deprecated?
			#pragma multi_compile _ _CLUSTERED_RENDERING 
			#pragma multi_compile _ DYNAMICLIGHTMAP_ON
			#pragma multi_compile_fragment _ DEBUG_DISPLAY

			//URP 14+
			#pragma multi_compile_fragment _ _FORWARD_PLUS
			
			//Constants
			#define SHADERPASS_FORWARD
			
			#pragma vertex LitPassVertex
			#pragma fragment LightingPassFragment
			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

			#include "Libraries/Input.hlsl"
			#include "Libraries/Common.hlsl"
			#include "Libraries/Color.hlsl"
			#include "Libraries/Lighting.hlsl"

			#include "FernQuadGrassLightingPass.hlsl"
			ENDHLSL
		}

		Pass
		{
			Name "ShadowCaster"
			Tags{"LightMode" = "ShadowCaster"}

			ZWrite [_ZWrite]
			ZTest LEqual
			Cull[_Cull]

			HLSLPROGRAM

			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma shader_feature_local_vertex _SCALEMAP
			#pragma shader_feature_local_vertex _BILLBOARD
			#pragma shader_feature_local_fragment _FADING

			#define SHADERPASS_SHADOWCASTER
			
			#pragma vertex ShadowPassVertex
			#pragma fragment ShadowPassFragment

			#include "Libraries/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			
			#include "Libraries/Common.hlsl"

			#include "ShadowPass.hlsl"
			ENDHLSL
		}
		
		//Deferred rendering
		Pass
        {
            Name "GBuffer"
            Tags{"LightMode" = "UniversalGBuffer"}

            //AlphaToMask [_AlphaToCoverage] //Not supported in deferred
			Blend [_SrcBlend] [_DstBlend], One Zero
			Cull [_Cull]
			ZWrite [_ZWrite]

			HLSLPROGRAM

			//In place for projects that use a custom RP or modified URP and require specific behaviour for vegetation
			#define VEGETATION_SHADER

			// -------------------------------------
			// Material Keywords
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma shader_feature_local_vertex _SCALEMAP
			#pragma shader_feature_local_vertex _BILLBOARD
			#pragma shader_feature_local_fragment _FADING
			#pragma shader_feature_local _NORMALMAP
			#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
			#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
		
			//URP12+
			#pragma shader_feature_local_fragment _DISABLE_DECALS

			//Disable features
			#undef _ALPHAPREMULTIPLY_ON
			#undef _EMISSION
			#undef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
			#undef _OCCLUSIONMAP
			#undef _METALLICSPECGLOSSMAP

			// -------------------------------------
			// Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			//Note: has to be on a new line in older versions
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

			//URP 10+
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION

			// -------------------------------------
			// Unity defined keywords
			//#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile_fog
			//Accurate G-buffer normals option in renderer settings
			#pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT

			//URP12+
			#pragma multi_compile _ DYNAMICLIGHTMAP_ON //Realtime GI
			#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3 //Decal support
            #pragma multi_compile_fragment _ _LIGHT_LAYERS
            #pragma multi_compile_fragment _ _RENDER_PASS_ENABLED //Stencil support

			//Constants
			#define SHADERPASS_DEFERRED

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

			#include "Libraries/Input.hlsl"
			#include "Libraries/Common.hlsl"
			
			#include "Libraries/Color.hlsl"
			#include "Libraries/Lighting.hlsl"

			#pragma vertex LitPassVertex
			#pragma fragment LightingPassFragment
			
			#include "FernQuadGrassLightingPass.hlsl"

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

			#define SHADERPASS_DEPTHONLY

			// -------------------------------------
			// Material Keywords
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma shader_feature_local_vertex _SCALEMAP
			#pragma shader_feature_local_vertex _BILLBOARD
			#pragma shader_feature_local_fragment _FADING

			#include "Libraries/Input.hlsl"
			#include "Libraries/Common.hlsl"

			#pragma vertex DepthOnlyVertex
			#pragma fragment DepthOnlyFragment
			
			#include "DepthPass.hlsl"
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

			#define SHADERPASS_DEPTH_ONLY
			#define SHADERPASS_DEPTHNORMALS
			
			#pragma vertex DepthOnlyVertex	
			//Only URP 10.0.0+ does this amounts to the actual fragment shader, otherwise a dummy is used
			#pragma fragment DepthNormalsFragment

			// -------------------------------------
			// Material Keywords
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma shader_feature_local_vertex _SCALEMAP
			#pragma shader_feature_local_vertex _BILLBOARD
			#pragma shader_feature_local_fragment _FADING

			#include "Libraries/Input.hlsl"            			
			#include "Libraries/Common.hlsl"

			#include "DepthPass.hlsl"
			ENDHLSL
		}

		// Used for Baking GI. This pass is stripped from build.
		//Disabled, breaks SRP batcher, shader doesn't have the exact same properties as the Lit shader
		//UsePass "Universal Render Pipeline/Lit/Meta"

	}//Subshader

	FallBack "Hidden/Universal Render Pipeline/FallbackError"
	 CustomEditor "LWGUI.LWGUI"

}//Shader
