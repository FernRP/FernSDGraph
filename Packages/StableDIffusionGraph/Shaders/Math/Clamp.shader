Shader "Hidden/SDMix/Clamp"
{	
	Properties
	{
		// By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
		[InlineTexture]_Source_2D("Source", 2D) = "white" {}
		[InlineTexture]_Source_3D("Source", 3D) = "white" {}
		[InlineTexture]_Source_Cube("Source", Cube) = "white" {}

		// Other parameters
		_Min("Min", Float) = 0
		_Max("Max", Float) = 1

		// TODO: clamp mode per channel
		// [Enum(AllChannels, 0, PerChannel, 1)]_ClampMode("Mode", Float)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			HLSLPROGRAM
			#include "Packages/com.tateam.sdgraph/Shaders//SDMixFixed.hlsl"
            #pragma vertex CustomRenderTextureVertexShader
			#pragma fragment SDMixFragment
			#pragma target 3.0

			// The list of defines that will be active when processing the node with a certain dimension
            #pragma shader_feature CRT_2D CRT_3D CRT_CUBE

			float _Min;
			float _Max;

			// This macro will declare a version for each dimention (2D, 3D and Cube)
			TEXTURE_SAMPLER_X(_Source);

			float4 SDMixFrag (v2f_customrendertexture i) : SV_Target
			{
				float4 value = SAMPLE_X(_Source, i.localTexcoord.xyz, i.direction);
				return clamp(value, _Min, _Max);
			}
			ENDHLSL
		}
	}
}
