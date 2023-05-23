using System.Collections;
using System.Collections.Generic;
using FernNPRCore.SDNodeGraph;
using GraphProcessor;
using UnityEngine;

namespace FernNPRCore.SDNodeGraph
{
    [System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Blur")]
    public class SDBlurNode : SDShaderNode
    {
        [Input(name = "Source")] public Texture inputImage;
        public override string name => "SD Blur";

        public override string shaderName => "Hidden/Mixture/Blur";

        [Range(0, 64)] public float radius = 0;

        protected override void Process()
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
