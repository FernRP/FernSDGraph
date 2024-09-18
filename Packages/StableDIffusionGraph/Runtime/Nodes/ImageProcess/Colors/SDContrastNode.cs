using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEngine.SDGraph
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

        protected override void Enable()
        {
            shader = SDGraphResource.SdGraphDataHandle.shaderData.contrastPS;
            base.Enable();
        }

        protected override void Process(CommandBuffer cmd)
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