using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;
using UnityEngine.Rendering;

namespace FernNPRCore.SDNodeGraph
{
    [System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Combine")]
    public class SDCombineNode : SDShaderNode
    {
        
        static string[] displayedOptions = { "Input.Red", "Input.Green", "Input.Blue", "Input.Alpha", "Black", "Gray", "White", "Custom" };
        static int[] optionValues = { 0, 1, 2, 3, 4, 5, 6, 7 };
        
        public enum CombineChannel
        {
            Red = 0,
            Green,
            Blue,
            Alpha,
            Black,
            Gray,
            White,
            Custom,
        }
        
        [Input(name = "Source 1")] public Texture inputImage;
        [Input(name = "Source 2")] public Texture source2Image;
        [Input(name = "Source 3")] public Texture source3Image;
        [Input(name = "Source 4")] public Texture source4Image;

        public CombineChannel source1 = CombineChannel.Red;
        public CombineChannel source2 = CombineChannel.Green;
        public CombineChannel source3 = CombineChannel.Blue;
        public CombineChannel source4 = CombineChannel.Alpha;
        
        public override string name => "SD Combine";

        public override string shaderName => "Hidden/Mixture/Combine";

        protected override void Process(CommandBuffer cmd)
        {
            base.Process();
            BeforeProcessSetup();
            if(inputImage != null)
                SDUtil.SetTextureWithDimension(material, "_SourceR", inputImage);
            if(source2Image != null)
                SDUtil.SetTextureWithDimension(material, "_SourceG", source2Image);
            if(source3Image != null)
                SDUtil.SetTextureWithDimension(material, "_SourceB", source3Image);
            if(source4Image != null)
                SDUtil.SetTextureWithDimension(material, "_SourceA", source4Image);
            material.SetFloat("_CombineModeR", (float)source1);
            material.SetFloat("_CombineModeG", (float)source2);
            material.SetFloat("_CombineModeB", (float)source3);
            material.SetFloat("_CombineModeA", (float)source4);
            output.Update();
        }
    }
}