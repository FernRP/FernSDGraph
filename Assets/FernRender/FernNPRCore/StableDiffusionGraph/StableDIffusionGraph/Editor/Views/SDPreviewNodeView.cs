using System.Collections;
using System.Collections.Generic;
using FernNPRCore.SDNodeGraph;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;
using NodeGraphProcessor.Examples;
using Unity.VisualScripting;

namespace FernNPRCore.SDNodeGraph
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