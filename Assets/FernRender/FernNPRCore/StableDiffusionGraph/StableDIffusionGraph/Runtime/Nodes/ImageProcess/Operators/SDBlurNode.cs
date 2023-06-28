using System.Collections;
using System.Collections.Generic;
using FernNPRCore.SDNodeGraph;
using GraphProcessor;
using UnityEngine;
using UnityEngine.Rendering;

namespace FernNPRCore.SDNodeGraph
{
    [System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Blur")]
    public class SDBlurNode : SDShaderNode
    {
        [Input(name = "Source")] public Texture inputImage;
        public override string name => "SD Blur";

        public override string shaderName => "Hidden/SDMix/Blur";

        [Range(0, 64)] public float radius = 0;

        protected override void Process(CommandBuffer cmd)
        {
            base.Process();
            BeforeProcessSetup();
            if(inputImage != null)
                SDUtil.SetTextureWithDimension(material, "_Source", inputImage);
            material.SetFloat("_Radius", radius);
            output.Update();
        }
    }
}
