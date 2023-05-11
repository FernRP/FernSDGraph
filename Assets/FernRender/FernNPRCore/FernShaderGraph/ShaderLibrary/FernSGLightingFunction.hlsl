#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

// @Cyanilux | https://github.com/Cyanilux/URP_ShaderGraphCustomLighting
// Note this version of the package assumes v12+ due to usage of "Branch on Input Connection" node
// For older versions, see branches on github repo!

//------------------------------------------------------------------------------------------------------
// Main Light
//------------------------------------------------------------------------------------------------------

/*
- Obtains the Direction, Color and Distance Atten for the Main Light.
- (DistanceAtten is either 0 or 1 for directional light, depending if the light is in the culling mask or not)
- If you want shadow attenutation, see MainLightShadows_float, or use MainLightFull_float instead
*/
void MainLight_float (out float3 Direction, out float3 Color, out float DistanceAtten){
	#ifdef SHADERGRAPH_PREVIEW
		Direction = normalize(float3(1,1,-0.4));
		Color = float4(1,1,1,1);
		DistanceAtten = 1;
	#else
		Light mainLight = GetMainLight();
		Direction = mainLight.direction;
		Color = mainLight.color;
		DistanceAtten = mainLight.distanceAttenuation;
	#endif
}

//------------------------------------------------------------------------------------------------------
// Main Light Layer Test
//------------------------------------------------------------------------------------------------------

#ifndef SHADERGRAPH_PREVIEW
	#if UNITY_VERSION < 202220
	/*
	GetMeshRenderingLayer() is only available in 2022.2+
	Previous versions need to use GetMeshRenderingLightLayer()
	*/
	uint GetMeshRenderingLayer(){
		return GetMeshRenderingLightLayer();
	}
	#endif
#endif
		
/*
- Tests whether the Main Light Layer Mask appears in the Rendering Layers from renderer
- (Used to support Light Layers, pass your shading from Main Light into this)
- To work in an Unlit Graph, requires keywords :
	- Boolean Keyword, Global Multi-Compile "_LIGHT_LAYERS"
*/
void MainLightLayer_float(float3 Shading, out float3 Out){
	#ifdef SHADERGRAPH_PREVIEW
		Out = Shading;
	#else
		Out = 0;
		uint meshRenderingLayers = GetMeshRenderingLayer();
		#ifdef _LIGHT_LAYERS
			if (IsMatchingLightLayer(GetMainLight().layerMask, meshRenderingLayers))
		#endif
		{
			Out = Shading;
		}
	#endif
}

/*
- Obtains the Light Cookie assigned to the Main Light
- (For usage, You'd want to Multiply the result with your Light Colour)
- To work in an Unlit Graph, requires keywords :
	- Boolean Keyword, Global Multi-Compile "_LIGHT_COOKIES"
*/
void MainLightCookie_float(float3 WorldPos, out float3 Cookie){
	Cookie = 1;
	#if defined(_LIGHT_COOKIES)
        Cookie = SampleMainLightCookie(WorldPos);
    #endif
}

//------------------------------------------------------------------------------------------------------
// Main Light Shadows
//------------------------------------------------------------------------------------------------------

/*
- This undef (un-define) is required to prevent the "invalid subscript 'shadowCoord'" error,
  which occurs when _MAIN_LIGHT_SHADOWS is used with 1/No Shadow Cascades with the Unlit Graph.
- It's not required for the PBR/Lit graph, so I'm using the SHADERPASS_FORWARD to ignore it for that pass
*/
#ifndef SHADERGRAPH_PREVIEW
	#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
	#if (SHADERPASS != SHADERPASS_FORWARD)
		#undef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
	#endif
#endif

/*
- Samples the Shadowmap for the Main Light, based on the World Position passed in. (Position node)
- For shadows to work in the Unlit Graph, the following keywords must be defined in the blackboard :
	- Enum Keyword, Global Multi-Compile "_MAIN_LIGHT", with entries :
		- "SHADOWS"
		- "SHADOWS_CASCADE"
		- "SHADOWS_SCREEN"
	- Boolean Keyword, Global Multi-Compile "_SHADOWS_SOFT"
- For a PBR/Lit Graph, these keywords are already handled for you.
*/
void MainLightShadows_float (float3 WorldPos, half4 Shadowmask, out float ShadowAtten){
	#ifdef SHADERGRAPH_PREVIEW
		ShadowAtten = 1;
	#else
		#if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
		float4 shadowCoord = ComputeScreenPos(TransformWorldToHClip(WorldPos));
		#else
		float4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
		#endif
		ShadowAtten = MainLightShadow(shadowCoord, WorldPos, Shadowmask, _MainLightOcclusionProbes);
	#endif
}

void MainLightShadows_float (float3 WorldPos, out float ShadowAtten){
	MainLightShadows_float(WorldPos, half4(1,1,1,1), ShadowAtten);
}

//------------------------------------------------------------------------------------------------------
// Shadowmask (v10+)
//------------------------------------------------------------------------------------------------------

/*
- Used to support "Shadowmask" mode in Lighting window.
- Should be sampled once in graph, then input into the Main Light Shadows and/or Additional Light subgraphs/functions.
- To work in an Unlit Graph, likely requires keywords :
	- Boolean Keyword, Global Multi-Compile "SHADOWS_SHADOWMASK" 
	- Boolean Keyword, Global Multi-Compile "LIGHTMAP_SHADOW_MIXING"
	- (also LIGHTMAP_ON, but I believe Shader Graph is already defining this one)
*/
void Shadowmask_half (float2 lightmapUV, out half4 Shadowmask){
	#ifdef SHADERGRAPH_PREVIEW
		Shadowmask = half4(1,1,1,1);
	#else
		OUTPUT_LIGHTMAP_UV(lightmapUV, unity_LightmapST, lightmapUV);
		Shadowmask = SAMPLE_SHADOWMASK(lightmapUV);
	#endif
}

//------------------------------------------------------------------------------------------------------
// Ambient Lighting
//------------------------------------------------------------------------------------------------------

/*
- Uses "SampleSH", the spherical harmonic stuff that ambient lighting / light probes uses.
- Will likely be used in the fragment, so will be per-pixel.
- Alternatively could use the Baked GI node, as it'll also handle this for you.
- Could also use the Ambient node, would be cheaper but the result won't automatically adapt based on the Environmental Lighting Source (Lighting tab).
*/
void AmbientSampleSH_float (float3 WorldNormal, out float3 Ambient){
	#ifdef SHADERGRAPH_PREVIEW
		Ambient = float3(0.1, 0.1, 0.1); // Default ambient colour for previews
	#else
		Ambient = SampleSH(WorldNormal);
	#endif
}

//------------------------------------------------------------------------------------------------------
// Subtractive Baked GI
//------------------------------------------------------------------------------------------------------
/*
- Used to support "Subtractive" mode in Lighting window.
- To work in an Unlit Graph, likely requires keywords :
	- Boolean Keyword, Global Multi-Compile "LIGHTMAP_SHADOW_MIXING"
	- (also LIGHTMAP_ON, but I believe Shader Graph is already defining this one)
*/
void SubtractiveGI_float (float ShadowAtten, float3 normalWS, float3 bakedGI, out half3 result){
	#ifdef SHADERGRAPH_PREVIEW
		result = half3(1,1,1);
	#else
		Light mainLight = GetMainLight();
		mainLight.shadowAttenuation = ShadowAtten;
		MixRealtimeAndBakedGI(mainLight, normalWS, bakedGI);
		result = bakedGI;
	#endif
}

//------------------------------------------------------------------------------------------------------
// Mix Fog
//------------------------------------------------------------------------------------------------------
/*
- Adds fog to the colour, based on the Fog settings in the Lighting tab.
- Note : Not required for v12, can use Lerp instead. See "Mix Fog" SubGraph
*/
void MixFog_float (float3 Colour, float Fog, out float3 Out){
	#ifdef SHADERGRAPH_PREVIEW
		Out = Colour;
	#else
		Out = MixFog(Colour, Fog);
	#endif
}

//------------------------------------------------------------------------------------------------------
// Screen Depth Rim
//------------------------------------------------------------------------------------------------------


//------------------------------------------------------------------------------------------------------
// Position Extenstion
//------------------------------------------------------------------------------------------------------

void PositionWS2CS_float(float3 positionWS, out float4 positionCS)
{
	positionCS = TransformWorldToHClip(positionWS);
}

void PositionWS2CS_half(half3 positionWS, out half4 positionCS)
{
	positionCS = TransformWorldToHClip(positionWS);
}

#endif // CUSTOM_LIGHTING_INCLUDED
