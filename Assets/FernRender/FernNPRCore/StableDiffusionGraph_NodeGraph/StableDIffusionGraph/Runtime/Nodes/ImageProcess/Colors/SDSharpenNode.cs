using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;
using UnityEngine.Rendering;

namespace FernNPRCore.SDNodeGraph
{
    [System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Sharpen")]
    public class SDSharpenNode : SDShaderNode
    {
        [Input(name = "Source")] public Texture inputImage;
        [Range(0.01f, 8)]
        public float strength = 1;
        
        public override string name => "SD Sharpen";

        public override string shaderName => "Hidden/Mixture/Sharpen";

        protected override void Process(CommandBuffer cmd)
        {
            base.Process();
            BeforeProcessSetup();
            if(inputImage != null)
                SDUtil.SetTextureWithDimension(material, "_Source", inputImage);
            material.SetFloat("_Strength",strength);
            output.Update();
        }
    }
}