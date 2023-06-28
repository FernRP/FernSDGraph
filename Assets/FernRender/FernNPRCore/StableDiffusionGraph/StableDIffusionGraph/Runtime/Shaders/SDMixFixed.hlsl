#ifndef SDMIX_FIXED
#define SDMIX_FIXED

#include "SDMixUtils.hlsl"
#include "CustomTexture.hlsl"
#include "Blending.hlsl"

float3 GetDefaultUVs(v2f_customrendertexture i)
{
#ifdef CRT_CUBE
    return i.direction;
#elif CRT_2D
    return float3(i.localTexcoord.xy, 0.5);
#else
    return i.localTexcoord.xyz;
#endif
}

#endif
