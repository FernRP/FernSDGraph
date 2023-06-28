Shader "Hidden/SDGraph/Separate"
{
    Properties
    {
        // By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
        [InlineTexture]_Source_2D("Source", 2D) = "white" {}
        [InlineTexture]_Source_3D("Source", 3D) = "white" {}
        [InlineTexture]_Source_Cube("Source", Cube) = "white" {}

        // Other parameters
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #include "../../../Runtime/Shaders/SDMixFixed.hlsl"
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment SDMixFragment
            #pragma target 3.0
            // #pragma enable_d3d11_debug_symbols

            // The list of defines that will be active when processing the node with a certain dimension
            #pragma shader_feature CRT_2D CRT_3D CRT_CUBE

            // This macro will declare a version for each dimention (2D, 3D and Cube)
            TEXTURE_SAMPLER_X(_Source);
            uniform float4 _Channel;

            float4 SDMixFrag(v2f_customrendertexture i) : SV_Target
            {
                // The SAMPLE_X macro handles sampling for 2D, 3D and cube textures
                float4 c = SAMPLE_X(_Source, i.localTexcoord.xyz, i.direction) * _Channel;
                return c;
            }
            ENDHLSL
        }
    }
}