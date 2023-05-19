using System.Collections;
using System.Collections.Generic;
using FernNPRCore.SDNodeGraph;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;

namespace FernNPRCore.SDNodeGraph
{
    [NodeCustomEditor(typeof(SDImg2ImgNode))]
    public class SDImg2ImgNodeView : SDGraphNodeView
    {
        private SDImg2ImgNode node;

        private DropdownField samplerMethodDropdown;
        private LongField longLastField;

        public override void Enable()
        {
            base.Enable();
            node = nodeTarget as SDImg2ImgNode;

            if (node == null) return;

            List<string> samplerMethodList = new List<string>();
            samplerMethodList.AddRange(SDGraphResource.SdGraphDataHandle.samplers);

            samplerMethodDropdown = new DropdownField(samplerMethodList, 0);
            samplerMethodDropdown.RegisterValueChangedCallback(e => { node.samplerMethod = e.newValue; });
            samplerMethodDropdown.style.flexGrow = 1;
            samplerMethodDropdown.style.maxWidth = 140;

            var label = new Label("Method");
            label.style.width = StyleKeyword.Auto;
            label.style.marginRight = 5;

            var containerSampleMethod = new VisualElement();
            containerSampleMethod.style.flexDirection = FlexDirection.Row;
            containerSampleMethod.style.alignItems = Align.Center;
            containerSampleMethod.Add(label);
            containerSampleMethod.Add(samplerMethodDropdown);

            extensionContainer.Add(containerSampleMethod);

            // last seed
            var labelLastSeed = new Label("Last Seed");
            labelLastSeed.style.width = StyleKeyword.Auto;
            labelLastSeed.style.marginRight = 5;

            longLastField = new LongField();
            longLastField.value = node.outSeed;
            longLastField.style.flexGrow = 1;
            longLastField.style.maxWidth = 140;
            var containerLastSeed = new VisualElement();
            containerLastSeed.style.flexDirection = FlexDirection.Row;
            containerLastSeed.style.alignItems = Align.Center;
            containerLastSeed.Add(labelLastSeed);
            containerLastSeed.Add(longLastField);
            extensionContainer.Add(containerLastSeed);

            RefreshExpandedState();

            NotifyNodeChanged();
        }
    }
}