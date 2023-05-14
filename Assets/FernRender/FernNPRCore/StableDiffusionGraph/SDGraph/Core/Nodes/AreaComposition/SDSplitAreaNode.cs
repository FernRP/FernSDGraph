using FernGraph;
using FernGraph.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FernNPRCore.StableDiffusionGraph
{
    [Node(Path = "SD Standard")]
    [Tags("SD Node")]

    public class SDSplitAreaNode : Node
    {
        [Input] public Texture2D areaTexture;
        [Editable]
        public float threshold=.1f;
        [Output]
        public Color color;
        public List<Color> colors = new List<Color>();

        public override void OnValidate()
        {
            base.OnValidate();
            areaTexture = GetInputValue("areaTexture", this.areaTexture);
        }

        public override object OnRequestValue(Port port)
        {
            int index = port.Name.IndexOf('_') + 1;
            string numberString = port.Name.Substring(index);
            int number = int.Parse(numberString);

            return colors[number];
        }

    }
}