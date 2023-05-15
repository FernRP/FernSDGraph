#include "FernSGNPRLighting.hlsl"

void InitializeInputData(Varyings input, SurfaceDescription surfaceDescription, out InputData inputData)
{
    inputData = (InputData)0;

    inputData.positionWS = input.positionWS;

    #ifdef _NORMALMAP
        // IMPORTANT! If we ever support Flip on double sided materials ensure bitangent and tangent are NOT flipped.
        float crossSign = (input.tangentWS.w > 0.0 ? 1.0 : -1.0) * GetOddNegativeScale();
        float3 bitangent = crossSign * cross(input.normalWS.xyz, input.tangentWS.xyz);

        inputData.tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);
        #if _NORMAL_DROPOFF_TS
            inputData.normalWS = TransformTangentToWorld(surfaceDescription.NormalTS, inputData.tangentToWorld);
        #elif _NORMAL_DROPOFF_OS
            inputData.normalWS = TransformObjectToWorldNormal(surfaceDescription.NormalOS);
        #elif _NORMAL_DROPOFF_WS
            inputData.normalWS = surfaceDescription.NormalWS;
        #endif
    #else
        inputData.normalWS = input.normalWS;
    #endif
    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        inputData.shadowCoord = input.shadowCoord;
    #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
        inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
    #else
        inputData.shadowCoord = float4(0, 0, 0, 0);
    #endif

    inputData.fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactorAndVertexLight.x);
    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
#if defined(DYNAMICLIGHTMAP_ON)
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.dynamicLightmapUV.xy, input.sh, inputData.normalWS);
#else
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.sh, inputData.normalWS);
#endif
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);

    #if defined(DEBUG_DISPLAY)
    #if defined(DYNAMICLIGHTMAP_ON)
    inputData.dynamicLightmapUV = input.dynamicLightmapUV.xy;
    #endif
    #if defined(LIGHTMAP_ON)
    inputData.staticLightmapUV = input.staticLightmapUV;
    #else
    inputData.vertexSH = input.sh;
    #endif
    #endif
}

inline half GeometryAA(half3 normalWS, half smoothness, FernSGAddSurfaceData addSurfData)
{
    half dx = dot(ddx(normalWS),ddx(normalWS));
    half dy = dot(ddy(normalWS),ddy(normalWS));
    half roughness = 1 - smoothness;
    half roughnessAA = roughness * roughness + min(LerpWhiteTo(32, addSurfData.geometryAAVariant) * (dx + dy), addSurfData.geometryAAStrength * addSurfData.geometryAAStrength);
    roughnessAA = saturate(roughnessAA);
    half smoothnessAA = smoothness * (1 - roughnessAA);
    return smoothnessAA;
}


LightingData InitializeLightingData(Light mainLight, Varyings input, half3 normalWS, half3 viewDirectionWS)
{
    LightingData lightData;
    lightData.lightColor = mainLight.color;
    #if EYE
    lightData.NdotL = dot(addInputData.irisNormalWS, mainLight.direction.xyz);
    #else
    lightData.NdotL = dot(normalWS, mainLight.direction.xyz);
    #endif
    lightData.NdotLClamp = saturate(lightData.NdotL);
    lightData.HalfLambert = lightData.NdotL * 0.5 + 0.5;
    half3 halfDir = SafeNormalize(mainLight.direction + viewDirectionWS);
    lightData.LdotHClamp = saturate(dot(mainLight.direction.xyz, halfDir.xyz));
    lightData.NdotHClamp = saturate(dot(normalWS.xyz, halfDir.xyz));
    lightData.NdotVClamp = saturate(dot(normalWS.xyz, viewDirectionWS.xyz));
    lightData.HalfDir = halfDir;
    lightData.lightDir = mainLight.direction;
    #if defined(_RECEIVE_SHADOWS_OFF)
    lightData.ShadowAttenuation = 1;
    #elif _DEPTHSHADOW
    lightData.ShadowAttenuation = DepthShadow(_DepthShadowOffset, _DepthOffsetShadowReverseX, _DepthShadowThresoldOffset, _DepthShadowSoftness, input.positionCS.xy, mainLight.direction, addInputData);
    #else
    lightData.ShadowAttenuation = mainLight.shadowAttenuation * mainLight.distanceAttenuation;
    #endif

    return lightData;
}

///////////////////////////////////////////////////////////////////////////////
//                         Shading Function                                  //
///////////////////////////////////////////////////////////////////////////////

half3 NPRDiffuseLighting(BRDFData brdfData, half3 rampColor, LightingData lightingData, FernSGAddSurfaceData addSurfData, half radiance)
{
    half3 diffuse = 0;

    #if _LAMBERTIAN
        diffuse = lerp(addSurfData.darkColor.rgb, addSurfData.lightenColor.rgb, radiance);
    #elif _RAMPSHADING
        diffuse = rampColor.rgb;
    #elif _CELLSHADING
        diffuse = CellShadingDiffuse(radiance, addSurfData.cellThreshold, addSurfData.cellSoftness, addSurfData.lightenColor.rgb, addSurfData.darkColor.rgb);
    #endif
    
    diffuse *= brdfData.diffuse;
    return diffuse;
}

/**
 * \brief NPR Specular
 * \param brdfData 
 * \param surfData 
 * \param input 
 * \param inputData 
 * \param albedo 
 * \param addSurfData 
 * \param radiance 
 * \param lightData 
 * \return 
 */
half3 NPRSpecularLighting(BRDFData brdfData, NPRSurfaceData surfData, Varyings input, InputData inputData, half3 albedo, FernSGAddSurfaceData addSurfData, half radiance, LightingData lightData)
{
    half3 specular = 0;
    #if _STYLIZED
        specular = StylizedSpecular(albedo, lightData.NdotHClamp, addSurfData.StylizedSpecularSize, addSurfData.StylizedSpecularSoftness);
    #elif _BLINNPHONG
        specular = BlinnPhongSpecular((1 - brdfData.perceptualRoughness), lightData.NdotHClamp);
    #elif _KAJIYAHAIR
        half2 anisoUV = input.uv.xy * _AnisoShiftScale;
        AnisoSpecularData anisoSpecularData;
        InitAnisoSpecularData(anisoSpecularData);
        specular = AnisotropyDoubleSpecular(brdfData, anisoUV, input.tangentWS, inputData, lightData, anisoSpecularData,
            TEXTURE2D_ARGS(_AnisoShiftMap, sampler_AnisoShiftMap));
    #elif _ANGLERING
        AngleRingSpecularData angleRingSpecularData;
        InitAngleRingSpecularData(1, angleRingSpecularData);
        specular = AngleRingSpecular(angleRingSpecularData, inputData, radiance, lightData);
    #elif _GGX
        specular = GGXDirectBRDFSpecular(brdfData, lightData.LdotHClamp, lightData.NdotHClamp);
    #endif
    specular *= addSurfData.specularColor.rgb * radiance * brdfData.specular;
    return specular;
}

/**
 * \brief Main Lighting, consists of NPR and PBR Lighting Equation
 * \param brdfData 
 * \param brdfDataClearCoat 
 * \param input 
 * \param inputData 
 * \param surfData 
 * \param radiance 
 * \param lightData 
 * \return 
 */
half3 NPRMainLightDirectLighting(BRDFData brdfData, BRDFData brdfDataClearCoat, Varyings input, InputData inputData,
                                 NPRSurfaceData surfData, half radiance, FernSGAddSurfaceData addSurfData, LightingData lightData)
{
    half3 diffuse = NPRDiffuseLighting(brdfData, addSurfData.rampColor, lightData, addSurfData, radiance);
    half3 specular = NPRSpecularLighting(brdfData, surfData, input, inputData, surfData.albedo, addSurfData, radiance, lightData);
    half3 brdf = (diffuse + specular) * lightData.lightColor;
    return brdf;
}

/**
 * \brief AdditionLighting, Lighting Equation base on MainLight, TODO: if cell-shading should use other lighting equation
 * \param brdfData 
 * \param brdfDataClearCoat 
 * \param input 
 * \param inputData 
 * \param surfData 
 * \param addInputData 
 * \param shadowMask 
 * \param meshRenderingLayers 
 * \param aoFactor 
 * \return 
 */
half3 NPRAdditionLightDirectLighting(BRDFData brdfData, BRDFData brdfDataClearCoat, Varyings input, InputData inputData,
                                     NPRSurfaceData surfData,half4 shadowMask, half meshRenderingLayers, FernSGAddSurfaceData addSurfData,
                                     AmbientOcclusionFactor aoFactor)
{
    half3 additionLightColor = 0;
    #if defined(_ADDITIONAL_LIGHTS)
    uint pixelLightCount = GetAdditionalLightsCount();

    #if USE_FORWARD_PLUS
    for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK

        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);

    #ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
    #endif
        {
            LightingData lightingData = InitializeLightingData(light, input, inputData.normalWS, inputData.viewDirectionWS);
            half radiance = LightingRadiance(lightingData);
            // Additional Light Filter Referenced from https://github.com/unity3d-jp/UnityChanToonShaderVer2_Project
            float pureIntencity = max(0.001,(0.299 * lightingData.lightColor.r + 0.587 * lightingData.lightColor.g + 0.114 * lightingData.lightColor.b));
            //lightingData.lightColor = max(0, lerp(lightingData.lightColor, lerp(0, min(lightingData.lightColor, lightingData.lightColor / pureIntencity * _LightIntensityClamp), 1), _Is_Filter_LightColor));
            half3 addLightColor = NPRMainLightDirectLighting(brdfData, brdfDataClearCoat, input, inputData, surfData, radiance, addSurfData, lightingData);
            additionLightColor += addLightColor;
        }
    }
    #endif

    #if USE_CLUSTERED_LIGHTING
    for (uint lightIndex = 0; lightIndex < min(_AdditionalLightsDirectionalCount, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);
    #ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
    #endif
        {
            LightingData lightingData = InitializeLightingData(light, input, inputData.normalWS, inputData.viewDirectionWS);
            half radiance = LightingRadiance(lightingData);
            // Additional Light Filter Referenced from https://github.com/unity3d-jp/UnityChanToonShaderVer2_Project
            float pureIntencity = max(0.001,(0.299 * lightingData.lightColor.r + 0.587 * lightingData.lightColor.g + 0.114 * lightingData.lightColor.b));
            //lightingData.lightColor = max(0, lerp(lightingData.lightColor, lerp(0, min(lightingData.lightColor, lightingData.lightColor / pureIntencity * _LightIntensityClamp), 1), _Is_Filter_LightColor));
            half3 addLightColor = NPRMainLightDirectLighting(brdfData, brdfDataClearCoat, input, inputData, surfData, radiance, addSurfData, lightingData);
            additionLightColor += addLightColor;
        }
    }
    #endif

    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);
    #ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
    #endif
        {
            LightingData lightingData = InitializeLightingData(light, input, inputData.normalWS, inputData.viewDirectionWS);
            half radiance = LightingRadiance(lightingData);
            // Additional Light Filter Referenced from https://github.com/unity3d-jp/UnityChanToonShaderVer2_Project
            float pureIntencity = max(0.001,(0.299 * lightingData.lightColor.r + 0.587 * lightingData.lightColor.g + 0.114 * lightingData.lightColor.b));
           // TODO: Add SG defined:
           // lightingData.lightColor = max(0, lerp(lightingData.lightColor, lerp(0, min(lightingData.lightColor, lightingData.lightColor / pureIntencity * _LightIntensityClamp), 1), _Is_Filter_LightColor));
            half3 addLightColor = NPRMainLightDirectLighting(brdfData, brdfDataClearCoat, input, inputData, surfData, radiance, addSurfData, lightingData);
            additionLightColor += addLightColor;
        }
    LIGHT_LOOP_END
    #endif

    // vertex lighting only lambert diffuse for now...
    #if defined(_ADDITIONAL_LIGHTS_VERTEX)
        additionLightColor += inputData.vertexLighting * brdfData.diffuse;
    #endif

    return additionLightColor;
}

half3 NPRIndirectLighting(BRDFData brdfData, InputData inputData, Varyings input, FernSGAddSurfaceData addSurfData, half occlusion)
{
    half3 indirectDiffuse = inputData.bakedGI * occlusion;
    half3 reflectVector = reflect(-inputData.viewDirectionWS, inputData.normalWS);
    reflectVector = RotateAroundYInDegrees(reflectVector, addSurfData.envRotate);
    half NoV = saturate(dot(inputData.normalWS, inputData.viewDirectionWS));
    half fresnelTerm = Pow4(1.0 - NoV);
    half3 indirectSpecular = 0;
    #if _ENVDEFAULT
        indirectSpecular = NPRGlossyEnvironmentReflection(reflectVector, inputData.positionWS, inputData.normalizedScreenSpaceUV, brdfData.perceptualRoughness, occlusion) * addSurfData.envSpecularIntensity;
    #elif _ENVCUSTOM
        indirectSpecular = NPRGlossyEnvironmentReflection_Custom(addSurfData.envCustomReflection, occlusion) * addSurfData.envSpecularIntensity;
    #else
        indirectSpecular = _GlossyEnvironmentColor.rgb * occlusion;
    #endif
    
    half3 indirectColor = EnvironmentBRDF(brdfData, indirectDiffuse, indirectSpecular, fresnelTerm);

    #if _MATCAP
        half3 matCap = SamplerMatCap(_MatCapColor, input.uv.zw, inputData.normalWS, inputData.normalizedScreenSpaceUV, TEXTURE2D_ARGS(_MatCapTex, sampler_MatCapTex));
        indirectColor += lerp(matCap, matCap * brdfData.diffuse, _MatCapAlbedoWeight);
    #endif

    return indirectColor;
}

// ====================== Vert/Fragment ============================
PackedVaryings vert(Attributes input)
{
    Varyings output = (Varyings)0;
    output = BuildVaryings(input);
    PackedVaryings packedOutput = (PackedVaryings)0;
    packedOutput = PackVaryings(output);
    return packedOutput;
}

void frag(
    PackedVaryings packedInput
    , half facing : VFACE
    , out half4 outColor : SV_Target0
#ifdef _WRITE_RENDERING_LAYERS
    , out float4 outRenderingLayers : SV_Target1
#endif
)
{
    Varyings unpacked = UnpackVaryings(packedInput);
    UNITY_SETUP_INSTANCE_ID(unpacked);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(unpacked);
    SurfaceDescription surfaceDescription = BuildSurfaceDescription(unpacked);

#if defined(_SURFACE_TYPE_TRANSPARENT)
    bool isTransparent = true;
#else
    bool isTransparent = false;
#endif

#if defined(_ALPHATEST_ON)
    half alpha = AlphaDiscard(surfaceDescription.Alpha, surfaceDescription.AlphaClipThreshold);
#elif defined(_SURFACE_TYPE_TRANSPARENT)
    half alpha = surfaceDescription.Alpha;
#else
    half alpha = half(1.0);
#endif

    #if defined(LOD_FADE_CROSSFADE) && USE_UNITY_CROSSFADE
        LODFadeCrossFade(unpacked.positionCS);
    #endif

    InputData inputData;
    InitializeInputData(unpacked, surfaceDescription, inputData);
    // TODO: Mip debug modes would require this, open question how to do this on ShaderGraph.
    //SETUP_DEBUG_TEXTURE_DATA(inputData, unpacked.texCoord1.xy, _MainTex);

    #ifdef _SPECULAR_SETUP
        float3 specular = surfaceDescription.Specular;
        float metallic = 1;
    #else
        float3 specular = 0;
        float metallic = surfaceDescription.Metallic;
    #endif

    half3 normalTS = half3(0, 0, 0);
    #if defined(_NORMALMAP) && defined(_NORMAL_DROPOFF_TS)
        normalTS = surfaceDescription.NormalTS;
    #endif

    NPRSurfaceData surfData = (NPRSurfaceData)0;
    surfData.albedo              = surfaceDescription.BaseColor;
    surfData.metallic            = saturate(metallic);
    surfData.specular            = specular;
    surfData.smoothness          = saturate(surfaceDescription.Smoothness),
    surfData.occlusion           = surfaceDescription.Occlusion,
    surfData.emission            = surfaceDescription.Emission,
    surfData.alpha               = saturate(alpha);
    surfData.normalTS            = normalTS;
    surfData.clearCoatMask       = 0;
    surfData.clearCoatSmoothness = 1;

    #ifdef _CLEARCOAT
        surfData.clearCoatMask       = saturate(surfaceDescription.CoatMask);
        surfData.clearCoatSmoothness = saturate(surfaceDescription.CoatSmoothness);
    #endif

    // Init AddSurfaceData
    FernSGAddSurfaceData addSurfData = (FernSGAddSurfaceData)0;
    addSurfData.specularColor = surfaceDescription.SpecularColor;
    #if _RAMPSHADING
        addSurfData.rampColor = surfaceDescription.RampColor;
    #endif
    #if _STYLIZED
        addSurfData.StylizedSpecularSize = surfaceDescription.StylizedSpecularSize;
        addSurfData.StylizedSpecularSoftness = surfaceDescription.StylizedSpecularSoftness;
    #endif
    #if _CELLSHADING
        addSurfData.cellThreshold = surfaceDescription.CellThreshold;
        addSurfData.cellSoftness = surfaceDescription.CellSmoothness;
    #endif
    #if _SPECULARAA
        addSurfData.geometryAAVariant = surfaceDescription.GeometryAAVariant;
        addSurfData.geometryAAStrength = surfaceDescription.GeometryAAStrength;
        surfData.smoothness = GeometryAA(inputData.normalWS, surfData.smoothness, addSurfData);
    #endif
    #if _ENVCUSTOM ||_ENVDEFAULT
    addSurfData.envSpecularIntensity = surfaceDescription.EnvSpecularIntensity;
    #endif
    #if _ENVCUSTOM
        addSurfData.envCustomReflection = surfaceDescription.EnvReflection;
    #endif
    #if _ENVROTATE && _ENVDEFAULT
        addSurfData.envRotate = surfaceDescription.EnvRotate;
    #endif

    #if !_RAMPSHADING
        addSurfData.darkColor = surfaceDescription.DarkColor;
        addSurfData.lightenColor = surfaceDescription.LightenColor;
    #endif

    surfData.albedo = AlphaModulate(surfData.albedo, surfData.alpha);

#ifdef _DBUFFER
    ApplyDecalToSurfaceData(unpacked.positionCS, surface, inputData);
#endif

    half4 shadowMask = CalculateShadowMask(inputData);

    uint meshRenderingLayers = GetMeshRenderingLayer();
    Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, shadowMask);
    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI);
    #if defined(_SCREEN_SPACE_OCCLUSION)
        AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(inputData.normalizedScreenSpaceUV);
        mainLight.color *= aoFactor.directAmbientOcclusion;
        surfData.occlusion = min(surfData.occlusion, aoFactor.indirectAmbientOcclusion);
        #else
        AmbientOcclusionFactor aoFactor;
        aoFactor.indirectAmbientOcclusion = 1;
        aoFactor.directAmbientOcclusion = 1;
    #endif

    BRDFData brdfData, clearCoatbrdfData;
    InitializeNPRBRDFData(surfData, brdfData, clearCoatbrdfData);
    
    LightingData lightingData = InitializeLightingData(mainLight, unpacked, inputData.normalWS, inputData.viewDirectionWS);
    half radiance = LightingRadiance(lightingData);
    
    half4 color = 0;
    color.rgb = NPRMainLightDirectLighting(brdfData, clearCoatbrdfData, unpacked, inputData, surfData, radiance, addSurfData, lightingData);
    color.rgb += NPRAdditionLightDirectLighting(brdfData, clearCoatbrdfData, unpacked, inputData, surfData, shadowMask, meshRenderingLayers, addSurfData, aoFactor);
    color.rgb += NPRIndirectLighting(brdfData, inputData, unpacked, addSurfData, surfData.occlusion);
    color.rgb += surfData.emission;
    color.rgb = MixFog(color.rgb, inputData.fogCoord);

    color.a = OutputAlpha(color.a, isTransparent);

    outColor = color;

#ifdef _WRITE_RENDERING_LAYERS
    uint renderingLayers = GetMeshRenderingLayer();
    outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
#endif
}
