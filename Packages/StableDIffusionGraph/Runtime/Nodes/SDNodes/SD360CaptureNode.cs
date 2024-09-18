using System;
using System.Collections.Generic;
using System.IO;
using GraphProcessor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace UnityEngine.SDGraph
{
    [System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD 360 Capture")]
    public class SD360CaptureNode : SDNode
    {
        [Serializable]
        public enum ProjectionType
        {
            Equirectangular,
            Cubemap
        }
        
        [Serializable]
  public enum CubemapLayoutType
  {
    /// <summary>
    ///
    /// The horizontal cross layout:
    ///
    /// +------------+------------+------------+------------+
    /// |            |            |            |            |
    /// |            |            |            |            |
    /// |            |  +Y (Top)  |            |            |
    /// |            |            |            |            |
    /// |            |            |            |            |
    /// +------------+------------+------------+------------+
    /// |            |            |            |            |
    /// |            |            |            |            |
    /// | -X (Left)  | +Z (Front) | +X (Right) | -Z (Back)  |
    /// |            |            |            |            |
    /// |            |            |            |            |
    /// +------------+------------+------------+------------+
    /// |            |            |            |            |
    /// |            |            |            |            |
    /// |            | -Y (Bottom)|            |            |
    /// |            |            |            |            |
    /// |            |            |            |            |
    /// +------------+------------+------------+------------+
    ///
    /// </summary>
    HorizontalCross,
    /// <summary>
    ///
    /// The 6 sided layout:
    ///
    /// +------------+ +------------+ +------------+
    /// |            | |            | |            |
    /// |            | |            | |            |
    /// | +X (Right) | | -X (Left)  | |  +Y (Top)  |
    /// |            | |            | |            |
    /// |            | |            | |            |
    /// +------------+ +------------+ +------------+
    /// |            | |            | |            |
    /// |            | |            | |            |
    /// | -Y (Bottom)| | +Z (Front) | | -Z (Back)  |
    /// |            | |            | |            |
    /// |            | |            | |            |
    /// +------------+ +------------+ +------------+
    ///
    /// </summary>
    SixSided,
    /// <summary>
    ///
    /// The compact layout:
    ///
    /// +------------+------------+------------+
    /// |            |            |            |
    /// |            |            |            |
    /// | +X (Right) | -X (Left)  |  +Y (Top)  |
    /// |            |            |            |
    /// |            |            |            |
    /// +------------+------------+------------+
    /// |            |            |            |
    /// |            |            |            |
    /// | -Y (Bottom)| +Z (Front) | -Z (Back)  |
    /// |            |            |            |
    /// |            |            |            |
    /// +------------+------------+------------+
    ///
    /// </summary>
    Compact
  }
        
        [Input("Color"), ShowAsDrawer] public bool isRenderColor;
        [Output("Color")] public CustomRenderTexture colorTarget;
        
        public override string name => "SD 360 Capture";
        
        // public ProjectionType projectionType = ProjectionType.Equirectangular;
        // public CubemapLayoutType cubemapLayout = CubemapLayoutType.Compact;
        [HideInInspector]
        public SDCaptureProxy currentCapturePoint;
        private Material equirectMaterial;
        
        private RenderTexture equirectTexture;

        /// <summary>
        /// Camera for capture skybox.
        /// </summary>
        private Camera captureCamera;
        
        public override Texture previewTexture
        {
            get
            {
                return colorTarget;
            }
        }

        public override void OnNodeCreated()
        {
            base.OnNodeCreated();
            hasPreview = true;
        }
        
        protected override void Enable()
        {
            hasPreview = true;
            hasSettings = true;
            equirectMaterial = CoreUtils.CreateEngineMaterial(SDGraphResource.SdGraphDataHandle.shaderData.cubemapToEquirectPS);
            base.Enable();
            InitRenderTarget();
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
        
        private void InitRenderTarget()
        {
            UpdateTempRenderTexture(ref colorTarget,false, false, CustomRenderTextureUpdateMode.OnDemand, true);
        }
        
        public override void Update()
        {
            base.Update();
            if (!IsValidate()) return;
            InitRenderTarget();
        }

        private SDCaptureProxy[] cameras;
        
        private bool IsValidate()
        {
            cameras = GameObject.FindObjectsOfType<SDCaptureProxy>();
            if (cameras.Length > 1)
            {
                SDUtil.LogWarning("There has More than a Capture Camera");
                return false;
            }

            if (equirectMaterial == null)
            {
                equirectMaterial = CoreUtils.CreateEngineMaterial(SDGraphResource.SdGraphDataHandle.shaderData.cubemapToEquirectPS);
            }
            
            return true;
        }

        public void StartCapture()
        {
            if(!IsValidate()) return;
            if (currentCapturePoint == null)
            {                    
                currentCapturePoint = GameObject.FindObjectOfType<SDCaptureProxy>();
            }

            if (currentCapturePoint == null)
            {
                SDUtil.LogWarning("There are no Capture Camera in the scene");
                return;
            }
            currentCapturePoint.TryGetComponent<Camera>(out captureCamera);
            if (captureCamera == null)
            {
                captureCamera = currentCapturePoint.gameObject.AddComponent<Camera>();
                captureCamera.enabled = false;
            }

            int width = settings.GetResolvedWidth(graph);
            int height = settings.GetResolvedHeight(graph);
           
            // Create equirectangular render texture.
            RenderTexture equirectTexture = CreateRenderTexture(height/2, height/2, 24, 1, null, false);
            equirectTexture.dimension = UnityEngine.Rendering.TextureDimension.Cube;
            
            captureCamera.RenderToCubemap(equirectTexture);
            captureCamera.Render();
            
            equirectMaterial.SetMatrix("_CubeTransform", Matrix4x4.identity);
            
            // Convert to equirectangular projection.
            Graphics.Blit(equirectTexture, colorTarget, equirectMaterial);
            
              // Object.DestroyImmediate(captureCamera.GetComponent<UniversalAdditionalCameraData>());
              //Object.DestroyImmediate(captureCamera);
            
        }
    }
}