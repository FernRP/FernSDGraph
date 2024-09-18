using GraphProcessor;
using UnityEngine;
using UnityEngine.SDGraph;
using UnityEngine.UIElements;

namespace UnityEditor.SDGraph
{
    [NodeCustomEditor(typeof(SD360CaptureNode))]
    public class SD360CaptureNodeView : SDNodeView
    {
        private SD360CaptureNode node;
        private Button refreshProxyBtn;
        private Button addProxyBtn;
        private Button captureBtn; 

        public override void Enable()
        {
            base.Enable();
            node = nodeTarget as SD360CaptureNode;
            if(node == null) return;
            addProxyBtn = new Button(AddCapturePoint);
            addProxyBtn.text = "Add Capture Camera";
            captureBtn = new Button(Capture360);
            captureBtn.text = "Capture";
            extensionContainer.Add(addProxyBtn);
            extensionContainer.Add(captureBtn);
            RefreshExpandedState();  
        }

        private void AddCapturePoint()
        {
            var proxy = SDEditorUtils.CreateSD360Capture();
            node.currentCapturePoint = proxy;
        }

        private void Capture360()
        {
            node.StartCapture();
        }
    }
}