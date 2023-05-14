using FernGraph;
using FernGraph.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FernNPRCore.StableDiffusionGraph
{
    public class AreaPrompt
    {
        public Color areaColor;
        public string prompt;
    }

    [Node(Path = "SD Standard")]
    [Tags("SD Node")]
    public class SDAreaNode : Node
    {
        [Input] public string prompt;
        [Input] public Color areaColor;
        [Editable]
        public Color previewColor;
        [Output] public AreaPrompt areaPrompt;

        public override void OnValidate()
        {
            base.OnValidate();
            areaColor = GetInputValue("areaColor", this.areaColor);
            previewColor = areaColor;
        }
        public override object OnRequestValue(Port port)
        {
            areaPrompt = new AreaPrompt();
            areaPrompt.areaColor = areaColor;
            areaPrompt.prompt = prompt;
            return areaPrompt;
        }
    }
}