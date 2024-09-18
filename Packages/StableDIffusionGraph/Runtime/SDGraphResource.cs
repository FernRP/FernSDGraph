using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.VersionControl;
#endif
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEngine.SDGraph
{
    public static class SDGraphResource
    {
        private static string SDGraphDataHandleGUID = "dea2d18e10d08fb469d6cd38adb531fc";
        private static SDGraphDataHandle sdGraphDataHandle;

        public static SDGraphDataHandle SdGraphDataHandle
        {
            get
            {
                #if UNITY_EDITOR
                    if (sdGraphDataHandle == null)
                    {
                        sdGraphDataHandle =
                            AssetDatabase.LoadAssetAtPath<SDGraphDataHandle>(
                                AssetDatabase.GUIDToAssetPath(SDGraphDataHandleGUID));
                    }
                   
                #else
                    if (sdGraphDataHandle == null)
                    {
                        sdGraphDataHandle = Resources.Load<SDGraphDataHandle>("SDGraph/SDGraphDataHandle");
                    }
                #endif
               

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
            Resources.Load<RenderPipelineAsset>("SDGraph/URPData/SDUniversalRenderPipeline");

        public static RenderPipelineAsset sdUniversal
        {
            get
            {
                if (m_sdUniversal == null)
                {
                    m_sdUniversal = Resources.Load<RenderPipelineAsset>("SDGraph/URPData/SDUniversalRenderPipeline");
                }

                return m_sdUniversal;
            }
        }
    }
}
