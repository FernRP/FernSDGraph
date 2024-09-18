using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GraphProcessor;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace UnityEngine.SDGraph
{
    [System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Skybox To Cubemap")]
    public class SDSkyBoxToCubemapNode : LinearSDProcessorNode
    {
        
        public int cubeWidth = 512;

        [Output("right")] public Texture2D RightImage;
        [Output("left")] public Texture2D LeftImage;
        [Output("top")] public Texture2D TopImage;
        [Output("bottom")] public Texture2D BottomImage;
        [Output("front")] public Texture2D FrontImage;
        [Output("back")] public Texture2D BackImage;

        protected override void Enable()
        {
            base.Enable();
            var with = cubeWidth;
            RightImage = new Texture2D(with, with);
            LeftImage = new Texture2D(with, with);
            TopImage = new Texture2D(with, with);
            BottomImage = new Texture2D(with, with);
            FrontImage = new Texture2D(with, with);
            BackImage = new Texture2D(with, with);
        }

        protected override IEnumerator Execute()
        {
            yield return RenderSkybox(null);
        }
        
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

        public IEnumerator RenderSkybox(Action callback)
        {
            RenderSkybox();

            yield return null; 
        }

        public Cubemap cubemap ;
        
        public void RenderSkybox()
        {


            var cameraGo = GameObject.Find("Camera Skybox Capture");
            if (cameraGo == null)
            {
                cameraGo = new GameObject();
                cameraGo.name = "Camera Skybox Capture";
                cameraGo.AddComponent<Camera>(); 
            }

            var camera = cameraGo.GetComponent<Camera>();
            camera.enabled = false;
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.cullingMask = 0;

            var width = cubeWidth;
            
            cubemap = new Cubemap(width, DefaultFormat.LDR, 0);
            camera.RenderToCubemap(cubemap);
            cubemap.Apply();
            
            if(RightImage == null || RightImage.width != cubeWidth) RightImage = new Texture2D(width, width);
            if(LeftImage == null || RightImage.width != cubeWidth) LeftImage = new Texture2D(width, width);
            if(TopImage == null || RightImage.width != cubeWidth) TopImage = new Texture2D(width, width);
            if(BottomImage == null || RightImage.width != cubeWidth) BottomImage = new Texture2D(width, width);
            if(FrontImage == null || RightImage.width != cubeWidth) FrontImage = new Texture2D(width, width);
            if(BackImage == null || RightImage.width != cubeWidth) BackImage = new Texture2D(width, width);

            RightImage.SetPixels(cubemap.GetPixels(CubemapFace.PositiveX));
            RightImage.Apply();
            LeftImage.SetPixels(cubemap.GetPixels(CubemapFace.NegativeX));
            LeftImage.Apply();
            TopImage.SetPixels(cubemap.GetPixels(CubemapFace.PositiveY));
            TopImage.Apply();
            BottomImage.SetPixels(cubemap.GetPixels(CubemapFace.NegativeY));
            BottomImage.Apply();
            FrontImage.SetPixels(cubemap.GetPixels(CubemapFace.PositiveZ));
            FrontImage.Apply();
            BackImage.SetPixels(cubemap.GetPixels(CubemapFace.NegativeZ));
            BackImage.Apply();
            
            Object.DestroyImmediate(cameraGo);
        }
    }
}