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

        private CommandBuffer cmd;

        protected override void Enable()
        {
            base.Enable();
            isUpdate = true;
            hasPreview = true;
        }

        public override void OnNodeCreated()
        {
            base.OnNodeCreated();
            isUpdate = true;
            hasPreview = true;
        }

        public override void Update()
        {
            base.Update();
            if (!IsValidate()) return;

            InitRenderTarget();
            RenderCamera(colorTarget);
            RenderCamera(normalTarget);
            RenderCamera(depthTarget);
            SetPreviewType();
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

            return camera != null;
        }

        private void InitRenderTarget()
        {
            if (colorTarget == null)
            {
                colorTarget = RenderTexture.GetTemporary(width, height, 24,
                    camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
            }

            if (normalTarget == null)
            {
                normalTarget = RenderTexture.GetTemporary(width, height, 24,
                    camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
            }

            if (depthTarget == null)
            {
                depthTarget = RenderTexture.GetTemporary(width, height, 24,
                    camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
            }

            if (inpaintTarget == null)
            {
                inpaintTarget = RenderTexture.GetTemporary(width, height, 24,
                    camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
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
                RenderTexture.ReleaseTemporary(colorTarget);
            }
            if (depthTarget != null)
            {
                RenderTexture.ReleaseTemporary(colorTarget);
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
            if (colorTarget != null)
                RenderTexture.ReleaseTemporary(colorTarget);
            if (normalTarget != null)
                RenderTexture.ReleaseTemporary(normalTarget);
            if (depthTarget != null)
                RenderTexture.ReleaseTemporary(depthTarget);
            if (inpaintTarget != null)
                RenderTexture.ReleaseTemporary(inpaintTarget);
        }

        protected void RenderCamera(RenderTexture rtTarget)
        {
            originRT = camera.targetTexture;
            camera.targetTexture = rtTarget;
            camera.Render();
            camera.targetTexture = originRT;
        }
    }
}