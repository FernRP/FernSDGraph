using GraphProcessor;
using UnityEngine;
using UnityEngine.SDGraph;
using UnityEngine.UIElements;

namespace UnityEditor.SDGraph
{
    [NodeCustomEditor(typeof(SDCubemapToPanoramaNode))]
    public class SDCubemapToPanoramaNodeView : SDNodeView
    {
        private SDCubemapToPanoramaNode node;
        private Button convertBtn;
        
        public override void Enable()
        {
            base.Enable();
            node = nodeTarget as SDCubemapToPanoramaNode;
            if(node == null) return;
            convertBtn = new Button(ConvertToPanorama);
            convertBtn.text = "Convert To Panorama";
            extensionContainer.Add(convertBtn);
            RefreshExpandedState();  
        }

        private void ConvertToPanorama()
        {
            node.CubemapToPanorama();
        }
    }
}