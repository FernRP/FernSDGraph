using System;
using System.Collections.Generic;
using FernGraph;
using FernGraph.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FernNPRCore.StableDiffusionGraph
{
    [CustomNodeView(typeof(SDSamplerNode))]
    public class SDSamplerNodeView : NodeView
    {
        private LongField longField;
        private LongField longLastField;
        protected override void OnInitialize()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("SDGraphRes/SDNodeView"));
            AddToClassList("sdNodeView");
            PortView inView = GetInputPort("SDFlowIn");
            PortView outView = GetOutputPort("SDFlowOut");
            if (inView != null) inView.AddToClassList("SDFlowInPortView");
            if (outView != null) outView.AddToClassList("SDFlowOutPortView");
            
            var samplerNode = Target as SDSamplerNode;
            if(samplerNode == null) return;

            samplerNode.OnUpdateSeedField = null;
            samplerNode.OnUpdateSeedField += OnUpadteSeed;

            List<string> samplerMethodList = new List<string>();
            samplerMethodList.AddRange(SDDataHandle.Instance.samplers);

            var samplerMethodDropdown = new DropdownField(samplerMethodList, 0);
            samplerMethodDropdown.RegisterValueChangedCallback(e =>
            {
                samplerNode.SamplerMethod = e.newValue;
            });
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
            
            // seed
            var labelSeed = new Label("Seed    ");
            labelSeed.style.width = StyleKeyword.Auto;
            labelSeed.style.marginRight = 5;
            
            longField = new LongField();
            longField.value = -1;
            longField.RegisterValueChangedCallback((e) =>
            {
                samplerNode.Seed = e.newValue;
            });
            
            longField.style.flexGrow = 1;
            longField.style.maxWidth = 140;
            var containerSeed = new VisualElement();
            containerSeed.style.flexDirection = FlexDirection.Row;
            containerSeed.style.alignItems = Align.Center;
            containerSeed.Add(labelSeed);
            containerSeed.Add(longField);
            extensionContainer.Add(containerSeed);
            
            // last seed
            var labelLastSeed = new Label("Last Seed");
            labelLastSeed.style.width = StyleKeyword.Auto;
            labelLastSeed.style.marginRight = 5;
            
            longLastField = new LongField();
            longLastField.value = samplerNode.outSeed;
            longLastField.style.flexGrow = 1;
            longLastField.style.maxWidth = 140;
            var containerLastSeed = new VisualElement();
            containerLastSeed.style.flexDirection = FlexDirection.Row;
            containerLastSeed.style.alignItems = Align.Center;
            containerLastSeed.Add(labelLastSeed);
            containerLastSeed.Add(longLastField);
            extensionContainer.Add(containerLastSeed);
            
            var container = new IMGUIContainer(OnGUI);
            extensionContainer.Add(container);
            
            RefreshExpandedState();
        }

        void OnGUI()
        {
            if(Target is not SDSamplerNode { isExecuting: true } samplerNode) return;
            var controlRect = EditorGUILayout.GetControlRect();
            controlRect.height = 40;
            var label = samplerNode.cur_step == -1 ? "init" : $"{samplerNode.speed:F3}it/s";

            var total = (long)((samplerNode.Step - 1) / samplerNode.speed + 1 / samplerNode.init_speed);
            total = Math.Max(total, 0);
            var re = total - (long)DateTime.Now.Subtract(samplerNode.startTime).TotalSeconds;
            re = Math.Max(re, 0);
            EditorGUI.ProgressBar(controlRect, samplerNode.progress, $"{samplerNode.progress * 100:F1}% ({label})\n{re.Seconds_To_HMS()}/{total.Seconds_To_HMS()}");
            GUILayout.Space(20);
        }
        private void OnUpadteSeed(long seed, long outSeed)
        {
            var samplerNode = Target as SDSamplerNode;
            if(samplerNode == null) return;
            samplerNode.Seed = seed;
            samplerNode.outSeed = outSeed;
            longField.value = seed;
            longLastField.value = outSeed;
        }
    }
}
