using FernGraph;
using FernGraph.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace FernNPRCore.StableDiffusionGraph
{
    [Node(Path = "SD Standard")]
    [Tags("SD Node")]

    public class SDSplitAreaNode : Node
    {
        [Input] public Texture2D areaTexture;

        public override object OnRequestValue(Port port)
        {
            return null;
        }
    }
}