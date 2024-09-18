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
    [System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Cubemap To Panorama")]
    public class SDCubemapToPanoramaNode : LinearSDProcessorNode
    {
        [Input("right")] public Texture2D RightImage;
        [Input("left")] public Texture2D LeftImage;
        [Input("top")] public Texture2D TopImage;
        [Input("bottom")] public Texture2D BottomImage;
        [Input("front")] public Texture2D FrontImage;
        [Input("back")] public Texture2D BackImage;

        [Output("Color")] public CustomRenderTexture colorTarget;
        
        public override Texture previewTexture
        {
            get
            {
                return colorTarget;
            }
        }
        
        public Cubemap cubemap;
        private Material equirectMaterial;

        protected override void Enable()
        {
            hasPreview = true;
            equirectMaterial = CoreUtils.CreateEngineMaterial(SDGraphResource.SdGraphDataHandle.shaderData.cubemapToEquirectPS);
            InitRenderTarget();
        }

        private void InitRenderTarget()
        {
            if (RightImage != null && LeftImage != null && TopImage != null && BottomImage != null &&
                FrontImage != null && BackImage != null)
            {
                UpdateTempRenderTexture(ref colorTarget,false, false, CustomRenderTextureUpdateMode.OnDemand, true, 
                    GraphicsFormat.R8G8B8A8_UNorm, RightImage.width * 2, RightImage.width);
            }
        }

        protected override void Process(CommandBuffer cmd)
        {
            base.Process(cmd);
            InitRenderTarget();
        }

        public void CubemapToPanorama()
        {
            if (RightImage != null && LeftImage != null && TopImage != null && BottomImage != null && FrontImage != null && BackImage != null)
            {
                InitRenderTarget();
                
                cubemap = new Cubemap(RightImage.width, DefaultFormat.LDR, 0);

                cubemap.SetPixels(RightImage.GetPixels(), CubemapFace.PositiveX);
                cubemap.SetPixels(LeftImage.GetPixels(), CubemapFace.NegativeX);
                cubemap.SetPixels(TopImage.GetPixels(), CubemapFace.PositiveY);
                cubemap.SetPixels(BottomImage.GetPixels(), CubemapFace.NegativeY);
                cubemap.SetPixels(FrontImage.GetPixels(), CubemapFace.PositiveZ);
                cubemap.SetPixels(BackImage.GetPixels(), CubemapFace.NegativeZ);
                
                cubemap.Apply();
                
                if (equirectMaterial == null)
                {
                    equirectMaterial = CoreUtils.CreateEngineMaterial(SDGraphResource.SdGraphDataHandle.shaderData.cubemapToEquirectPS);
                }
                    
                Graphics.Blit(cubemap, colorTarget, equirectMaterial);

            }
        }

        public override void Process()
        {
            base.Process();
            CubemapToPanorama();
        }
    }
}