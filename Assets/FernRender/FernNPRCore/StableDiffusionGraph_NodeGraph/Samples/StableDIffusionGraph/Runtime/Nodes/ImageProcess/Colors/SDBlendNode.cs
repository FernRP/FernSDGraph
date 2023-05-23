using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;

namespace FernNPRCore.SDNodeGraph
{
    [System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Contrast")]
    public class SDContrastNode : SDShaderNode
    {
        [Input(name = "Source")] public Texture inputImage;
        [Range(-1,1)]
        public float saturation = 0;
        [Range(-1,1)]
        public float luminosity = 0;
        
        public override string name => "SD Contrast";

        public override string shaderName => "Hidden/Mixture/Contrast";

        protected override void Process()
        {
            base.Process();
            if(inputImage == null) return;
            BeforeProcessSetup();
            SDUtil.SetTextureWithDimension(material, "_Source", inputImage);
            material.SetFloat("_Saturation", saturation);
            material.SetFloat("_Luminosity", luminosity);
            output.Update();
        }
    }
}