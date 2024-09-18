using System.Collections;
using System.Collections.Generic;
using UnityEditor.SDGraph;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;
using NodeGraphProcessor.Examples;
using UnityEngine.SDGraph;
using Unity.VisualScripting;

namespace UnityEditor.SDGraph
{
    [NodeCustomEditor(typeof(SDSkyBoxToCubemapNode))]
    public class SDSkyBoxToCubemapNodeView : SDNodeView
    {
        private SDSkyBoxToCubemapNode node;
        private Button renderCubemapBtn;

        public override void Enable()
        {
            base.Enable();
            node = nodeTarget as SDSkyBoxToCubemapNode;
            if(node == null) return;
            renderCubemapBtn = new Button(RenderSkyToCubemap);
            renderCubemapBtn.text = "Render Sky To Cubemap";
            extensionContainer.Add(renderCubemapBtn);
            RefreshExpandedState();  
        }

        private void RenderSkyToCubemap()
        {
            node.RenderSkybox();
        }
    }
}