using Codice.CM.Client.Differences.Graphic;
using FernGraph;
using FernGraph.Editor;
using System.Collections.Generic;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.UIElements;

namespace FernNPRCore.StableDiffusionGraph
{
    [CustomNodeView(typeof(SDAreaNode))]

    public class SDAreaNodeView : NodeView
    {
        protected override void OnInitialize()
        {
            base.OnInitialize();

            var button = new Button(Refresh);
            button.style.backgroundImage = SDTextureHandle.RefreshIcon;
            button.style.width = 20;
            button.style.height = 20;
            button.style.alignSelf = Align.Auto;
            button.style.bottom = 5;
            titleContainer.Add(button);
        }

        void Refresh()
        {
            var config = Target as SDAreaNode;
            config.areaColor = config.GetInputValue("areaColor", config.areaColor);

            config.previewColor = config.areaColor;
        }
    }
}