using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace FernNPRCore.SDNodeGraph
{
    public static class TextureUtils
    {
        const int CurveTextureResolution = 512;
        static Color[] pixels = new Color[CurveTextureResolution];
        static Dictionary<TextureDimension, Texture> blackTextures = new Dictionary<TextureDimension, Texture>();
        static Dictionary<TextureDimension, Texture> whiteTextures = new Dictionary<TextureDimension, Texture>();
        
        // Do not change change these names, it would break all graphs that are using default texture values
        static readonly string blackDefaultTextureName = "SDMix Black";
        static readonly string whiteDefaultTextureName = "SDMix white";
        
        public static void UpdateTextureFromCurve(AnimationCurve curve, ref Texture2D curveTexture)
        {
            if (curveTexture == null)
            {
                curveTexture = new Texture2D(CurveTextureResolution, 1, TextureFormat.RFloat, false, true);
                curveTexture.wrapMode = TextureWrapMode.Clamp;
                curveTexture.filterMode = FilterMode.Bilinear;
                curveTexture.hideFlags = HideFlags.HideAndDontSave;
            }

            for (int i = 0; i < CurveTextureResolution; i++)
            {
                float t = (float)i / (CurveTextureResolution - 1);
                pixels[i] = new Color(curve.Evaluate(t), 0, 0, 1);
            }
            curveTexture.SetPixels(pixels);
            curveTexture.Apply(false);
        }
        
        public static Texture GetBlackTexture(TextureDimension dim, int sliceCount = 0)
        {
            Texture blackTexture;

            if (dim == TextureDimension.Any || dim == TextureDimension.Unknown || dim == TextureDimension.None)
                throw new Exception($"Unable to create white texture for type {dim}");

            if (blackTextures.TryGetValue(dim, out blackTexture))
            {
                // We don't cache texture arrays
                if (dim != TextureDimension.Tex2DArray && dim != TextureDimension.Tex2DArray)
                    return blackTexture;
            }

            blackTexture = CreateColorRenderTexture(dim, Color.black);
            blackTexture.name = blackDefaultTextureName;
            blackTextures[dim] = blackTexture;

            return blackTexture;
        }
        
        public static RenderTexture CreateColorRenderTexture(TextureDimension dim, Color color)
        {
            RenderTexture rt = new RenderTexture(1, 1, 0, GraphicsFormat.R8G8B8A8_UNorm, 1)
            {
                volumeDepth = 1,
                dimension = dim,
                enableRandomWrite = true,
                hideFlags = HideFlags.HideAndDontSave
            };
            rt.Create();

            var cmd = CommandBufferPool.Get();
            for (int i = 0; i < GetSliceCount(rt); i++)
            {
                cmd.SetRenderTarget(rt, 0, (CubemapFace)i, i);
                cmd.ClearRenderTarget(false, true, color);
            }

            Graphics.ExecuteCommandBuffer(cmd);

            return rt;
        }
        
        public static int GetSliceCount(Texture tex)
        {
            if (tex == null)
                return 0;

            switch (tex)
            {
                case Texture2D _:
                    return 1;
                case Texture2DArray t:
                    return t.depth;
                case Texture3D t:
                    return t.depth;
                case CubemapArray t:
                    return t.cubemapCount;
                case Cubemap _:
                    return 1;
                case RenderTexture rt:
                    if (rt.dimension == TextureDimension.Tex2D || rt.dimension == TextureDimension.Cube)
                        return 1;
                    else if (rt.dimension == TextureDimension.Tex3D || rt.dimension == TextureDimension.Tex2DArray || rt.dimension == TextureDimension.CubeArray)
                        return rt.volumeDepth;
                    else
                        return 0;
                default:
                    return 0;
            }
        }
    }
}