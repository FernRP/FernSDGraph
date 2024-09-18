using System;
using System.Collections.Generic;
using System.IO;
using GraphProcessor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace UnityEngine.SDGraph
{
    //[System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Panorama To Cubemap")]
    public class SDPanoramaToCubemapNode : SDNode
    {
        
        [Input(name = "Source")] public Texture2D inputImage;
        public Cubemap cubemap;


        public RenderTexture CreateRenderTexture(int width, int height, int depth, int antiAliasing, RenderTexture t2Create, bool create = true)
        {
            if (t2Create &&
                (t2Create.width == width) && (t2Create.height == height) && (t2Create.depth == depth) &&
                (t2Create.antiAliasing == antiAliasing) && (t2Create.IsCreated() == create))
                return t2Create;

            if (t2Create != null)
            {
                UnityEngine.Object.Destroy(t2Create);
            }

            t2Create = new RenderTexture(width, height, depth, RenderTextureFormat.ARGB32);
            //t2Create = new RenderTexture(width, height, depth, RenderTextureFormat.Default);
            t2Create.antiAliasing = antiAliasing;
            t2Create.hideFlags = HideFlags.HideAndDontSave;

            // Make sure render texture is created.
            if (create)
                t2Create.Create();

            return t2Create;
        }
        
        public void PanoramaToCubemap()
        {
            var width = inputImage.height;

            cubemap = new Cubemap(width, TextureFormat.RGB24, false);

            Color[] pixels = inputImage.GetPixels();
            
            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                Color[] facePixels = new Color[width * width];
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < width; y++)
                    {
                        // 根据面的索引和(x, y)坐标计算在panorama贴图中的对应像素索引
                        // 这里需要根据具体的panorama到cubemap的映射关系来计算
                        int panoramaIndex = CalculatePanoramaIndex(faceIndex, width, x, y);
                        facePixels[y * width + x] = pixels[panoramaIndex];
                    }
                }
                cubemap.SetPixels(facePixels, (CubemapFace)faceIndex);
                
            }
        }
        
        private int CalculatePanoramaIndexFromAngles(float phi, float theta, int width, int height)
        {
            // 将球面坐标转换为UV坐标
            float u = (phi + Mathf.PI) / (2 * Mathf.PI);
            float v = (theta + Mathf.PI / 2) / Mathf.PI;

            // 根据UV坐标计算在Equirectangular贴图中的像素索引
            int x = Mathf.RoundToInt(u * (width - 1));
            int y = Mathf.RoundToInt(v * (height - 1));

            // 确保计算出的像素索引在合法范围内
            x = Mathf.Clamp(x, 0, width - 1);
            y = Mathf.Clamp(y, 0, height - 1);

            // 计算在Equirectangular贴图中的像素索引
            return y * width + x;
        }
        
        private int CalculatePanoramaIndex(int faceIndex, int width, int x, int y)
        {
            // 计算panorama贴图中的水平和垂直方向上的角度
            float horizontalAngle = (float)x / width * 360; // 360度全景图
            float verticalAngle = (float)y / width * 180; // 垂直方向180度

            // 根据面的索引和角度计算在panorama贴图中的对应像素索引
            switch (faceIndex)
            {
                case 0: // Positive X (right)
                    // 根据水平角度计算在panorama贴图中的像素索引
                    return CalculatePanoramaIndexFromAngles(horizontalAngle, verticalAngle, inputImage.width, inputImage.height);
                case 1: // Negative X (left)
                    // 类似地计算其他面的像素索引
                    return CalculatePanoramaIndexFromAngles(horizontalAngle, verticalAngle, inputImage.width, inputImage.height);
                case 2: // Positive Y (top)
                    // 类似地计算其他面的像素索引
                    return CalculatePanoramaIndexFromAngles(horizontalAngle, verticalAngle, inputImage.width, inputImage.height);
                case 3: // Negative Y (bottom)
                    // 类似地计算其他面的像素索引
                    return CalculatePanoramaIndexFromAngles(horizontalAngle, verticalAngle, inputImage.width, inputImage.height);
                case 4: // Positive Z (front)
                    // 类似地计算其他面的像素索引
                    return CalculatePanoramaIndexFromAngles(horizontalAngle, verticalAngle, inputImage.width, inputImage.height);
                case 5: // Negative Z (back)
                    // 类似地计算其他面的像素索引
                    return CalculatePanoramaIndexFromAngles(horizontalAngle, verticalAngle, inputImage.width, inputImage.height);
                default:
                    return 0;
            }
        }
    }
}