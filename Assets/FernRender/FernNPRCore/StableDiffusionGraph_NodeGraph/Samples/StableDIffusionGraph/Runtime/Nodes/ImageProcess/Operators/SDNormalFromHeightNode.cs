using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;

namespace FernNPRCore.SDNodeGraph
{
    [System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Normal Form Height")]
    public class SDNormalFromHeightNode : SDShaderNode
    {
        [Input(name = "Source")] public Texture inputImage;
        [Range(0,128)]
        public float strength = 32;
        
        public override string name => "SD Normal From Height";

        public override string shaderName => "Hidden/Mixture/NormalFromHeight";


        protected override void Process()
        {
            base.Process();
            if(inputImage == null) return;
            BeforeProcessSetup();
            material.SetTexture("_Source", inputImage);
            material.SetFloat("_Strength", strength);
            output.Update();
        }
    }
}