using FernGraph;
using FernGraph.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace FernNPRCore.StableDiffusionGraph
{
    [Node(Path = "SD AreaComposition")]
    [Tags("SD Node")]
    public class SDCombineAreaNode : Node
    {
        public override object OnRequestValue(Port port)
        {
            return null;
        }
    }
}