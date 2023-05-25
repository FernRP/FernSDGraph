using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;

namespace FernNPRCore.SDNodeGraph
{
    [System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Blend")]
    public class SDBlendNode : SDShaderNode
    {
        public enum Blend
        {
            [InspectorName("Normal (Copy)")]
            Normal = 0,
            [InspectorName("Min (Darken)")]
            Min = 1,
            [InspectorName("Max (Lighten)")]
            Max = 2,
            [InspectorName("Additive (Linear Dodge)")]
            Additive = 13,
            Subtract = 22,
            Burn = 3,
            Difference = 5,
            Dodge = 6,
            Divide = 7,
            Exclusion = 8,
            HardLight = 9,
            HardMix = 10,
            LinearBurn = 12,
            LinearLight = 14,
            LinearLightAddSub = 15,
            Multiply = 16,
            Negation = 17,
            Overlay = 18,
            PinLight = 19,
            Screen = 20,
            SoftLight = 21,
            VividLight = 23,
            Transparent = 24,
        }
        
        public enum PerChannel
        {
            PerChannel = 0,
            R = 1,
            G = 2,
            B = 3,
            A = 4
        }
        
        [Input(name = "Source")] public Texture inputImage;
        [Input(name = "Target")] public Texture targetImage;
        [Input(name = "Mask")] public Texture maskImage;
        
        public Blend blendMode = Blend.Normal;
        [Range(0,1)]
        public float opacity = 0.5f;
        public PerChannel maskMode = PerChannel.PerChannel;
        public bool clamp = true;
        
        public override string name => "SD Blend";

        public override string shaderName => "Hidden/Mixture/Blend";

        protected override void Process()
        {
            base.Process();
            BeforeProcessSetup();
            if(inputImage != null) 
                SDUtil.SetTextureWithDimension(material, "_Source", inputImage);
            if(targetImage != null)
                SDUtil.SetTextureWithDimension(material, "_Target", targetImage);
            if(maskImage != null)
                SDUtil.SetTextureWithDimension(material, "_Mask", maskImage);
            material.SetFloat("_BlendMode", (float)blendMode);
            material.SetFloat("_MaskMode", (float)maskMode);
            material.SetFloat("_Opacity", opacity);
            material.SetFloat("_RemoveNegative", clamp?1:0);
            output.Update();
        }
    }
}