using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;

namespace FernNPRCore.SDNodeGraph
{
    [System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Sample Gradient")]
    public class SDSampleGradientNode : SDNode
    {
        
        [Input("x"),Range(0.0f,1.0f)]
        public float x=0.0f;
        
        [Output("Color")]
        new public Color	color;
        
        public Gradient gradient = new Gradient();
    
        public override string name => "SD Sample Gradient";

        protected override void Process()
        {
            base.Process();
            color = gradient.Evaluate(x);
        }
    }
}