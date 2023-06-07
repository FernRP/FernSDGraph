using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;
using UnityEngine.Rendering;

namespace FernNPRCore.SDNodeGraph
{
    [System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Invert")]
    public class SDInvertNode : SDShaderNode
    {
        [Input(name = "Source")] public Texture inputImage;
        public bool hue = true;
        public bool saturation = true;
        public bool value = true;
        public bool alpha = true;
        
        public override string name => "SD Invert";

        public override string shaderName => "Hidden/Mixture/Invert";

        protected override void Process(CommandBuffer cmd)
        {
            base.Process();
            BeforeProcessSetup();
            if(inputImage != null)
                SDUtil.SetTextureWithDimension(material, "_Source", inputImage);
            material.SetFloat("_Hue", hue ? 1 : 0);
            material.SetFloat("_Saturation",  saturation ? 1 : 0);
            material.SetFloat("_Value",  value ? 1 : 0);
            material.SetFloat("_Alpha", alpha ? 1 : 0);
            output.Update();
        }
    }
}