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
        public float strength;

        public AreaPrompt(Color areaColor, string prompt, float strength)
        {
            this.areaColor = areaColor;
            this.prompt = prompt;
            this.strength = strength;
        }
    }

    [Node(Path = "SD AreaComposition")]
    [Tags("SD Node")]
    public class SDAreaNode : Node
    {
        [Input] public string prompt;
        [Input] public float strength=0.8f;
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
            areaPrompt = new AreaPrompt(areaColor,prompt,strength);
            return areaPrompt;
        }
    }
}