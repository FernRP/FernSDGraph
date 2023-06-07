using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;
using UnityEngine.Rendering;

namespace FernNPRCore.SDNodeGraph
{
    [System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD HSV")]
    public class SDHSVNode : SDShaderNode
    {
        [Input(name = "Source")] public Texture inputImage;
        [Input(name = "HSV Offset")] public Texture hsvOffsetImage;
        
        [Range(0.0f, 1.0f)]
        public float hue = 0.5f;
        [Range(0,1)]
        public float saturation = 0.5f;
        [Range(0,1)]
        public float value = 0.5f;
        public float maxValue = 0.5f;
        
        public override string name => "Hue Saturation Value";

        public override string shaderName => "Hidden/Mixture/HSV";

        protected override void Process(CommandBuffer cmd)
        {
            base.Process();
            BeforeProcessSetup();
            if(inputImage != null)
                SDUtil.SetTextureWithDimension(material, "_Source", inputImage);
            if(hsvOffsetImage != null)
                SDUtil.SetTextureWithDimension(material, "_HSVOffset", hsvOffsetImage);
            material.SetFloat("_Hue",hue);
            material.SetFloat("_Saturation", saturation);
            material.SetFloat("_Value", value);
            material.SetFloat("_MaxValue", maxValue);
            output.Update();
        }
    }
}