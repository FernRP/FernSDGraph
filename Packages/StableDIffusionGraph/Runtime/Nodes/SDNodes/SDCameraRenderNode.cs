using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEditor.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace FernNPRCore.SDNodeGraph
{
    [System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Camera Render")]
    public class SDCameraRenderNode : SDNode
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

        [Output("Color")] public CustomRenderTexture colorTarget;
        [Output("Normal")] public CustomRenderTexture normalTarget;
        [Output("Depth")] public CustomRenderTexture depthTarget;
        [Output("InPaint")] public CustomRenderTexture inpaintTarget; 
        [Input("Color"), ShowAsDrawer] public bool isRenderColor;
        [Input("Normal"), ShowAsDrawer] public bool isRenderNormal;
        [Input("Depth"), ShowAsDrawer] public bool isRenderDepth;
        [Input("InPaint"), ShowAsDrawer] public bool isRenderInPaint;
        
        public PreviewType previewType = PreviewType.Color;

        public override Texture previewTexture
        {
            get
            {
                switch (previewType)
                {
                    case PreviewType.Color:
                        return colorTarget;
                    case PreviewType.Normal:
                        return normalTarget;
                    case PreviewType.Depth:
                        return depthTarget;
                    case PreviewType.InPaint:
                        return inpaintTarget;
                }

                return null;
            }
        }

        private Camera camera;

        private RenderTexture originRT;

        private UniversalAdditionalCameraData cameraUniversalData;
        

        protected override void Enable()
        {
            hasPreview = true;
            hasSettings = true;
            base.Enable();
            isUpdate = true;
            InitRenderTarget();
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
            RenderColor();
            RenderNormal();
            RenderDepth();
            RenderInpaint();
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
            
            UpdateTempRenderTexture(ref colorTarget,false, false, CustomRenderTextureUpdateMode.OnDemand, true);
            UpdateTempRenderTexture(ref normalTarget);
            UpdateTempRenderTexture(ref depthTarget, false, false, CustomRenderTextureUpdateMode.OnDemand, true);
            UpdateTempRenderTexture(ref inpaintTarget);
        }

        public void ResetRTResolution()
        {
            InitRenderTarget();
        }

        protected override void Disable()
        {
            base.Disable();
            isUpdate = false;
            if (colorTarget != null)
                colorTarget.Release();
            if (normalTarget != null)
                normalTarget.Release();
            if (depthTarget != null)
                depthTarget.Release();
            if (inpaintTarget != null)
                inpaintTarget.Release();
        }

        private void RenderColor()
        {
            if(!isRenderColor) return;
            originRT = camera.targetTexture;
            camera.targetTexture = colorTarget;
            camera.Render();
            camera.targetTexture = originRT;
            colorTarget.Update();
        }
        
        private void RenderNormal()
        {
            if(!isRenderNormal) return;
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
            normalTarget.Update();
        }
        
        private void RenderInpaint()
        {
            if(!isRenderInPaint) return;
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
            cameraUniversalData.SetRenderer(0);
            camera.targetTexture = depthTarget;
            
            camera.Render();
            
            // restore
            camera.backgroundColor = normalBackGround;
            camera.clearFlags = normalClearFlags;
            GraphicsSettings.renderPipelineAsset = normalRenderPipelineAsset;
            cameraUniversalData.SetRenderer(0); // TODO: may be no 0
            camera.targetTexture = originRT;
            inpaintTarget.Update();
        }
        
        private void RenderDepth()
        {
            if(!isRenderDepth) return;
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
            cameraUniversalData.SetRenderer(2);
            camera.targetTexture = depthTarget;

            camera.Render();
            
            // restore
            camera.backgroundColor = normalBackGround;
            camera.clearFlags = normalClearFlags;
            GraphicsSettings.renderPipelineAsset = normalRenderPipelineAsset;
            cameraUniversalData.SetRenderer(0); // TODO: may be no 0
            camera.targetTexture = originRT;
            depthTarget.Update();
        }
    }
}