using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FernNPRCore.SDNodeGraph
{
    public static class SDGraphResource
    {
        private static SDGraphDataHandle sdGraphDataHandle = Resources.Load<SDGraphDataHandle>("SDGraphDataHandle");

        public static SDGraphDataHandle SdGraphDataHandle
        {
            get
            {
                if (sdGraphDataHandle == null)
                {
                    sdGraphDataHandle = Resources.Load<SDGraphDataHandle>("SDGraphDataHandle");
                }

                return sdGraphDataHandle;
            }
        }
        
        static Material _texture2DPreviewMaterial;
        public static Material texture2DPreviewMaterial
        {
            get
            {
                if (_texture2DPreviewMaterial == null)
                {
                    _texture2DPreviewMaterial = new Material(Shader.Find("Hidden/SDGraphTexture2DPreview"));
                }

                return _texture2DPreviewMaterial;
            }
        }

        private static RenderPipelineAsset m_sdUniversal =
            Resources.Load<RenderPipelineAsset>("SDGraphUniversalData/SDUniversalRenderPipeline");

        public static RenderPipelineAsset sdUniversal
        {
            get
            {
                if (m_sdUniversal == null)
                {
                    m_sdUniversal = Resources.Load<RenderPipelineAsset>("SDGraphUniversalData/SDUniversalRenderPipeline");
                }

                return m_sdUniversal;
            }
        }
    }
}
