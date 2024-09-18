using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SDGraph;
using GraphProcessor;
using UnityEngine.SDGraph;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.SDGraph
{
    
    [NodeCustomEditor(typeof(SDPanoramaToCubemapNode))]
    public class SDPanoramaToCubemapNodeView : SDNodeView
    {
        
        private SDPanoramaToCubemapNode node;
        private Button convertBtn;
        
        public override void Enable()
        {
            base.Enable();
            node = nodeTarget as SDPanoramaToCubemapNode;
            convertBtn = new Button(ConvertToCubemap);
            convertBtn.text = "Convert To Cubemap";
            extensionContainer.Add(convertBtn);
            RefreshExpandedState();  
        }

        private void ConvertToCubemap()
        {
            node.PanoramaToCubemap();
        }
    }
}