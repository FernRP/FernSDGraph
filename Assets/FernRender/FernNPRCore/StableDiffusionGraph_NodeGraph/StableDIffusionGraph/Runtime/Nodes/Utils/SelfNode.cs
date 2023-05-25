using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using FernNPRCore.SDNodeGraph;


namespace FernNPRCore.SDNodeGraph
{
    [System.Serializable, NodeMenuItem("Realtime/Self")]
    public class SelfNode : SDNode
    {
        [Input(name = "In")] public float input;

        [Output(name = "Out")] public float output;

        public override string name => "SelfNode";

        protected override void Process()
        {
            output = input * 42;
        }
    }
}