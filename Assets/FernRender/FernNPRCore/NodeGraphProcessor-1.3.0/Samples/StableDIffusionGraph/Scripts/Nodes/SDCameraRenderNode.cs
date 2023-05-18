using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using FernNPRCore.StableDiffusionGraph;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEditor.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace FernNPRCore.SDNodeGraph
{
    [System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Camera Render")]
    public class SDCameraRenderNode : BaseNode
    {
        [Serializable]
        public enum PreviewType
        {
            Color = 0,
            Normal,
            Depth,
            InPaint
        }
        
        public override string name => "SD Camera Render";

        [Output("Color")] public RenderTexture colorTarget;
        [Output("Normal")] public RenderTexture normalTarget;
        [Output("Depth")] public RenderTexture depthTarget;
        [Output("InPaint")] public RenderTexture inpaintTarget;
        public PreviewType previewType = PreviewType.Color;
        public Camera camera;

        [ChangeEvent(true)]
        public int width = 512;
        [ChangeEvent(true)]
        public int height = 512; 

        private RenderTexture originRT;

        private UniversalAdditionalCameraData cameraUniversalData;


        protected override void Enable()
        {
            base.Enable();
            isUpdate = true;
            hasPreview = true;
            InitRenderTarget();
            SetPreviewType();
        }

        public override void OnNodeCreated()
        {
            base.OnNodeCreated();
            IsValidate();
            isUpdate = true;
            hasPreview = true;
        }

        public override void Update()
        {
            base.Update();
            if (!IsValidate()) return;
            
            InitRenderTarget();
            SetPreviewType();
            RenderColor();
            RenderNormal();
            RenderDepth();
            RenderInpaint();
        }

        private void SetPreviewType()
        {
            switch (previewType)
            {
                case PreviewType.Color:
                    previewTexture = colorTarget;
                    break;
                case PreviewType.Normal:
                    previewTexture = normalTarget;
                    break;
                case PreviewType.Depth:
                    previewTexture = depthTarget;
                    break;
                case PreviewType.InPaint:
                    previewTexture = inpaintTarget;
                    break;
            }
        }

        private bool IsValidate()
        {
            if (camera == null)
            {
                camera = Camera.main;
            }

            if (camera != null)
            {
                cameraUniversalData = camera.GetComponent<UniversalAdditionalCameraData>();
            }

            return camera != null;
        }

        private void InitRenderTarget()
        {
            if(!IsValidate()) return;
            if (colorTarget == null)
            {
                colorTarget = RenderTexture.GetTemporary(width, height, 16,
                    camera.allowHDR ? RenderTextureFormat.RGB111110Float : RenderTextureFormat.RGB111110Float);
            }

            if (normalTarget == null)
            {
                normalTarget = RenderTexture.GetTemporary(width, height, 16,
                    camera.allowHDR ? RenderTextureFormat.RGB111110Float : RenderTextureFormat.RGB111110Float);
            }

            if (depthTarget == null)
            {
                depthTarget = RenderTexture.GetTemporary(width, height, 16,
                    camera.allowHDR ? RenderTextureFormat.RGB111110Float : RenderTextureFormat.RGB111110Float);
            }

            if (inpaintTarget == null)
            {
                inpaintTarget = RenderTexture.GetTemporary(width, height, 16,
                    camera.allowHDR ? RenderTextureFormat.RGB111110Float : RenderTextureFormat.RGB111110Float);
            }
        }

        protected override void Process()
        {
        }

        public void ResetRTResolution()
        {
            if (colorTarget != null)
            {
                RenderTexture.ReleaseTemporary(colorTarget);
            }
            if (normalTarget != null)
            {
                RenderTexture.ReleaseTemporary(normalTarget);
            }
            if (depthTarget != null)
            {
                RenderTexture.ReleaseTemporary(depthTarget);
            }
            if (inpaintTarget != null)
            {
                RenderTexture.ReleaseTemporary(inpaintTarget);
            }
            
            InitRenderTarget();
        }

        protected override void Disable()
        {
            base.Disable();
            isUpdate = false;
            if (colorTarget != null)
                RenderTexture.ReleaseTemporary(colorTarget);
            if (normalTarget != null)
                RenderTexture.ReleaseTemporary(normalTarget);
            if (depthTarget != null)
                RenderTexture.ReleaseTemporary(depthTarget);
            if (inpaintTarget != null)
                RenderTexture.ReleaseTemporary(inpaintTarget);
        }

        private void RenderColor()
        {
            originRT = camera.targetTexture;
            camera.targetTexture = colorTarget;
            camera.Render();
            camera.targetTexture = originRT;
        }
        
        private void RenderNormal()
        {
            if(cameraUniversalData == null) return;
            if(SDGraphResource.sdUniversal == null) return;
            
            // temp car param
            originRT = camera.targetTexture;
            var normalBackGround = camera.backgroundColor;
            var normalClearFlags = camera.clearFlags;
            var normalRenderPipelineAsset = GraphicsSettings.renderPipelineAsset;
            var normalLayer = camera.cullingMask;
            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = Color.black;
            GraphicsSettings.renderPipelineAsset = SDGraphResource.sdUniversal;
            cameraUniversalData.SetRenderer(1);
            camera.targetTexture = normalTarget;

            camera.Render();
            
            // restore
            camera.backgroundColor = normalBackGround;
            camera.clearFlags = normalClearFlags;
            GraphicsSettings.renderPipelineAsset = normalRenderPipelineAsset;
            cameraUniversalData.SetRenderer(0); // TODO: may be no 0
            camera.targetTexture = originRT;
        }
        
        private void RenderInpaint()
        {
            if(cameraUniversalData == null) return;
            if(SDGraphResource.sdUniversal == null) return;
            
            // temp car param
            originRT = camera.targetTexture;
            var normalBackGround = camera.backgroundColor;
            var normalClearFlags = camera.clearFlags;
            var normalRenderPipelineAsset = GraphicsSettings.renderPipelineAsset;
            var normalLayer = camera.cullingMask;
            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = Color.clear;
            GraphicsSettings.renderPipelineAsset = SDGraphResource.sdUniversal;
            cameraUniversalData.SetRenderer(2);
            camera.targetTexture = depthTarget;
            
            camera.Render();
            
            // restore
            camera.backgroundColor = normalBackGround;
            camera.clearFlags = normalClearFlags;
            GraphicsSettings.renderPipelineAsset = normalRenderPipelineAsset;
            cameraUniversalData.SetRenderer(0); // TODO: may be no 0
            camera.targetTexture = originRT;
        }
        
        private void RenderDepth()
        {
            if(cameraUniversalData == null) return;
            if(SDGraphResource.sdUniversal == null) return;
            
            // temp car param
            originRT = camera.targetTexture;
            var normalBackGround = camera.backgroundColor;
            var normalClearFlags = camera.clearFlags;
            var normalRenderPipelineAsset = GraphicsSettings.renderPipelineAsset;
            var normalLayer = camera.cullingMask;
            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = Color.black;
            GraphicsSettings.renderPipelineAsset = SDGraphResource.sdUniversal;
            cameraUniversalData.SetRenderer(1);
            camera.targetTexture = depthTarget;

            camera.Render();
            
            // restore
            camera.backgroundColor = normalBackGround;
            camera.clearFlags = normalClearFlags;
            GraphicsSettings.renderPipelineAsset = normalRenderPipelineAsset;
            cameraUniversalData.SetRenderer(0); // TODO: may be no 0
            camera.targetTexture = originRT;
        }
    }
}