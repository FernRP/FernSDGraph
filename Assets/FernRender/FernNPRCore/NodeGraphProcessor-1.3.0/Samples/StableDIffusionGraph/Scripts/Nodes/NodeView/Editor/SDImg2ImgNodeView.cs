using System;
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
        
        private ProgressBar progressBar;

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

            progressBar = new ProgressBar();
            progressBar.highValue = 1;
            node.onProgressStart = null;
            node.onProgressUpdate = null;
            node.onProgressFinish = null;
            node.onProgressStart += OnProgressBarStart;
            node.onProgressUpdate += OnUpdateProgressBar;
            node.onProgressFinish += OnProgressBarFinish;
            
            RefreshExpandedState();
        }
        
        	
        private void OnUpdateProgressBar(float progress)
        {
            var label = node.cur_step == -1 ? "init" : $"{node.speed:F3}it/s";
            var total = (long)((node.step - 1) / node.speed + 1 / node.init_speed);
            total = Math.Max(total, 0);
            var re = total - (long)DateTime.Now.Subtract(node.startTime).TotalSeconds;
            re = Math.Max(re, 0);
            progressBar.title = $"{node.progress * 100:F1}% ({label})";
            progressBar.value = progress;
        }
	
        private void OnProgressBarStart()
        {
            progressBar.value = 0;
            node.progress = 0;
            progressBar.title = $"{node.progress * 100:F1}%";
            progressBar.focusable = true;
            if (!extensionContainer.Contains(progressBar))
            {
                extensionContainer.Add(progressBar);
            }
            RefreshExpandedState();
        }
	
        private void OnProgressBarFinish()
        {
            progressBar.value = 1;
            progressBar.focusable = false;
            if (extensionContainer.Contains(progressBar))
            {
                extensionContainer.Remove(progressBar);
            }
            RefreshExpandedState();
        }
    }
}