using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;

namespace FernNPRCore.SDNodeGraph
{
    [System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Remap")]
    public class SDRemapNode : SDShaderNode
    {
        
        public enum SDRemapMode
        {
            Brightness_Gradient = 0,
            Red_Channel_Curve = 5,
            Green_Channel_Curve = 6,
            Blue_Channel_Curve = 7,
            Alpha_Channel_Curve = 1,
            All_Channels = 8,
            Brightness_Curve = 2,
            Saturation_Curve = 3,
            Hue_Curve = 4,
        }
        
        [Input("Map")] public Texture mapImage;

        public SDRemapMode mode = SDRemapMode.Brightness_Gradient;
        
        public override string name => "SD Remap";

        public override string shaderName => "Hidden/Mixture/Remap";

        protected override void Process()
        {
            base.Process();
            BeforeProcessSetup();
            if(inputImage != null)
                SDUtil.SetTextureWithDimension(material, "_Input", inputImage);
            if(mapImage != null)
                material.SetTexture("_Map", mapImage);
            material.SetFloat("_Mode", (float)mode);
            output.Update();
        }
    }
}