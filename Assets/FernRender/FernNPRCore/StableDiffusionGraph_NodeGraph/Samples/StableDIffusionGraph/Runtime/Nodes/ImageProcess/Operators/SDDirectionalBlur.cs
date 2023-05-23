using System.Collections;
using System.Collections.Generic;
using FernNPRCore.SDNodeGraph;
using GraphProcessor;
using UnityEngine;

namespace FernNPRCore.SDNodeGraph
{
    [System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Directional Blur")]
    public class SDDirectionalBlur : SDShaderNode
    {
        [Input(name = "Source")] public Texture inputImage;
        public override string name => "SD Directional Blur";

        public override string shaderName => "Hidden/Mixture/DirectionalBlur";

        [Range(0, 100)] public float radius = 0;
        public Vector4 direction = new Vector2(0.707f, 0.707f);

        protected override void Process()
        {
            base.Process();
            BeforeProcessSetup();
            if(inputImage != null)
                SDUtil.SetTextureWithDimension(material, "_Source", inputImage);
            material.SetFloat("_Radius", radius);
            material.SetVector("_Direction", direction);
            output.Update();
        }
    }
}
