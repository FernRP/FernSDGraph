using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;
using System.Linq;

namespace FernNPRCore.SDNodeGraph
{
    [NodeCustomEditor(typeof(StringNode))]
    public class StringNodeView : SDNodeView
    {
        private TextField textArea;
        private StringNode node;
        Button togglePreviewButton = null;

        public override void Enable()
        {
            base.Enable();

            node = nodeTarget as StringNode;

            togglePreviewButton = new Button(() =>
            {
                node.isShowString = !node.isShowString;
                UpdatePreviewCollapseState();
            });
            togglePreviewButton.ClearClassList();
            togglePreviewButton.AddToClassList("PreviewToggleButton");
            controlsContainer.Add(togglePreviewButton);

            textArea = new TextField(-1, true, false, '*') { value = node.textFiledValue };
            textArea.Children().First().style.unityTextAlign = TextAnchor.UpperLeft;
            textArea.style.whiteSpace = WhiteSpace.Normal;
            textArea.style.height = float.NaN;
            textArea.RegisterValueChangedCallback(v =>
            {
                owner.RegisterCompleteObjectUndo("Edit string node");
                node.textFiledValue = v.newValue;
            });
            controlsContainer.Add(textArea);
        }

        void UpdatePreviewCollapseState()
        {
            if (!node.isShowString)
            {
                if (controlsContainer.Contains(textArea))
                {
                    controlsContainer.Remove(textArea);
                }

                togglePreviewButton.RemoveFromClassList("Collapsed");
            }
            else
            {
                if (!controlsContainer.Contains(textArea))
                {
                    controlsContainer.Add(textArea);
                }

                togglePreviewButton.AddToClassList("Collapsed");
            }
        }

        public override void Disable()
        {
            base.Disable();
            OnExpandAction = null;
        }
    }
}