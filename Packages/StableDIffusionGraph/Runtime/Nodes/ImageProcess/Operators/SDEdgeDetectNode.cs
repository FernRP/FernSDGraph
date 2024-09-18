using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEngine.SDGraph
{
    [System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Edge Detection")]
    public class SDEdgeDetectNode : SDShaderNode
    {

        public enum EdgeMode
        {
            Edge = 0, 
            ColorEdge = 1
        }
        [Input(name = "Source")] public Texture inputImage;
        [Range(0.01f,2)]
        public float step = 0.5f;
        [Range(1,8)]
        public float pow = 1f;
        public EdgeMode mode = EdgeMode.Edge;
        
        public override string name => "SD Edge Detection";

        protected override void Enable()
        {
            shader = SDGraphResource.SdGraphDataHandle.shaderData.edgeDetectPS;
            base.Enable();
        }

        protected override void Process(CommandBuffer cmd)
        {
            base.Process();
            if(inputImage == null) return;
            BeforeProcessSetup();
            SDUtil.SetTextureWithDimension(material, "_Source", inputImage);
            material.SetFloat("_Step", step);
            material.SetFloat("_Mode", (float)mode);
            material.SetFloat("_Pow", (float)pow);
            output.Update();
        }
    }
}