using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FernNPRCore.SDNodeGraph
{
    public static class TextureUtils
    {
        const int CurveTextureResolution = 512;
        static Color[] pixels = new Color[CurveTextureResolution];
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
    }
}