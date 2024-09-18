using System;
using UnityEngine.Serialization;

namespace UnityEngine.SDGraph
{
    [Serializable]
    public class SDGraphShaderData
    { 
        public Shader blendPS;
        public Shader blurPS;
        public Shader combinePS;
        public Shader contrastPS;
        public Shader gradientPS;
        public Shader hsvPS;
        public Shader invertPS;
        public Shader leverPS;
        public Shader remapPS;
        public Shader sharpenPS;
        public Shader directionalBlurPS;
        public Shader edgeDetectPS;
        public Shader mirrorPS;
        public Shader normalFromHeightPS;
        [FormerlySerializedAs("cubemapToEquirect")] public Shader cubemapToEquirectPS;
        public Shader previewTexturePS;
        public Shader separatePS;
    }
}