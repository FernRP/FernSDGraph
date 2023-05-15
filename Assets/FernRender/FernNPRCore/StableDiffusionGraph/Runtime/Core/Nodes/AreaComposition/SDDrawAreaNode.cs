using FernGraph;
using FernGraph.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FernNPRCore.StableDiffusionGraph
{
    [Node(Path = "SD AreaComposition")]
    [Tags("SD Node")]
    public class SDDrawAreaNode : Node
    {
        public override object OnRequestValue(Port port)
        {
            return null;
        }

    }
}