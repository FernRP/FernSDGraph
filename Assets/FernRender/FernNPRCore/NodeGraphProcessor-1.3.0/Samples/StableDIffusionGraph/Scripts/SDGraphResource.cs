using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                    Debug.Log("123");
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
    }
}
