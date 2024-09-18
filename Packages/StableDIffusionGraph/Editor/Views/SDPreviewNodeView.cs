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
    [NodeCustomEditor(typeof(SDPreviewNode))]
    public class SDPreviewNodeView : SDNodeView
    {
        private SDPreviewRealNode node;
        private Image previewImage;

        public override void Enable()
        {
            base.Enable();
            NotifyNodeChanged();
        }
    }
}