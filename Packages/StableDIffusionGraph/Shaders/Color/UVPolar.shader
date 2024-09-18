Shader "Hidden/SDMix/UVPolar"
{	
	Properties
	{
		_Scale("UV Scale", Vector) = (1.0,1.0,1.0,0.0)
		_Bias("UV Bias", Vector) = (0.0,0.0,0.0,0.0)
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

			#pragma shader_feature CRT_2D CRT_3D

			float4 _Scale;
			float4 _Bias;

			float4 SDMixFrag(v2f_customrendertexture IN) : SV_Target
			{
#ifdef CRT_2D
				float d = length(IN.globalTexcoord.xy - 0.5) * 2;
				float p = atan2(IN.globalTexcoord.y - 0.5, IN.globalTexcoord.x - 0.5) / 6.283185307 + 0.5;
				return float4(d * _Scale.x + _Bias.x, p * _Scale.y + _Bias.y, 0, 1);
#else
				float d = length(IN.globalTexcoord.xyz - 0.5) * 2;
				float p = atan2(IN.globalTexcoord.y - 0.5, IN.globalTexcoord.x - 0.5) / 6.283185307 + 0.5;
				float t = dot(normalize(IN.globalTexcoord.xyz - 0.5), float3(0, 1, 0)) * 0.5 + 0.5;
				return float4(d * _Scale.x + _Bias.x, p * _Scale.y + _Bias.y, t * _Scale.z + _Bias.z, 1);
#endif
			}
			ENDHLSL
		}
	}
}
