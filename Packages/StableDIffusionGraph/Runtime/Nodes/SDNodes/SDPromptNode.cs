using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace FernNPRCore.SDNodeGraph
{
    [System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Prompt")]
    public class SDPromptNode : SDNode
    {
        [Input(name = "Positive")] public string positive;
        [Input(name = "Negative")] public string negative;

        [Output(name = "Out")] public Prompt output;

        public override string name => "SD Prompt";

        protected override void Process()
        {
            output.positive = positive;
            output.negative = negative;
            GetPort(nameof(output), null).PushData();
        }
    }
}