using System;
using System.Collections.Generic;
using GraphProcessor;

namespace FernNPRCore.SDNodeGraph
{
    
    [Serializable, NodeMenuItem("Stable Diffusion Graph/SD ControlNet Extension")]
    public class SDControlNetExtensionNode : SDExtensionNode
    {
        
        [Input(name = "Arg0")] public ControlNetData arg0;
        [Input(name = "Arg1")] public ControlNetData arg1;
        [Input(name = "Arg2")] public ControlNetData arg2;
        public override string name => "ControlNet";

        public override object args
        {
            get
            {
                var controlNetArgs = new List<object>();
                if (arg0 != null) controlNetArgs.Add(arg0);
                if (arg1 != null) controlNetArgs.Add(arg1);
                if (arg2 != null) controlNetArgs.Add(arg2);
                return controlNetArgs.ToArray();
            }
        }
    }
}