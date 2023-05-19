using System.Collections.Generic;
using System.Linq;
using FernGraph;
using FernGraph.Editor;
using Unity.EditorCoroutines.Editor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FernNPRCore.StableDiffusionGraph
{
    [CustomNodeView(typeof(SDHRUpscalers))]
    public class SDHRUpscalersView : NodeView
    {
        protected override void OnInitialize()
        {
            base.OnInitialize();

            if(Target is not SDHRUpscalers upscalers) return;
            extensionContainer.Clear();
            OnAsync();
            var button = new Button(OnAsync);
            button.style.backgroundImage = SDTextureHandle.RefreshIcon;
            button.style.width = 20;
            button.style.height = 20;
            button.style.alignSelf = Align.FlexEnd;
            button.style.bottom = 0;
            button.style.right = 0;
            titleButtonContainer.Add(button);
            extensionContainer.Add(modelsContainer);
            // Setup a container to render IMGUI content in 
            var container = new IMGUIContainer(OnGUI);
            extensionContainer.Add(container);
            RefreshExpandedState();
        }

        private void OnGUI()
        {
            if(Target is not SDHRUpscalers upscalers) return;
            var modelNames = upscalers.modelNames;
            if (modelNames is not { Count: > 0 }) return;
            
            EditorGUILayout.BeginVertical();
            EditorGUI.BeginChangeCheck();
            
                    
            var index = modelNames.IndexOf(upscalers.upscaler.hr_upscaler);
            if (index == -1)
            {
                index = 0;
                upscalers.upscaler.hr_upscaler = modelNames[0];
            }

            index = EditorGUILayout.Popup("model", index, modelNames.ToArray());
            var steps = EditorGUILayout.IntSlider("steps", upscalers.upscaler.hr_second_pass_steps, 0, 150);
            var denoising = EditorGUILayout.Slider("denoising", upscalers.upscaler.denoising_strength, 0f, 4f);
            var scale = EditorGUILayout.Slider("scale", upscalers.upscaler.hr_scale, 1f, 4f);
            var resizeX = EditorGUILayout.IntSlider("resizeX", upscalers.upscaler.hr_resize_x, 0, 2048);
            var resizeY = EditorGUILayout.IntSlider("resizeY", upscalers.upscaler.hr_resize_y, 0, 2048);
            
            if (EditorGUI.EndChangeCheck())
            {
                upscalers.upscaler.hr_upscaler = modelNames[index];
                upscalers.upscaler.hr_second_pass_steps = steps;
                upscalers.upscaler.denoising_strength = denoising;
                upscalers.upscaler.hr_scale = scale;
                upscalers.upscaler.hr_resize_x = resizeX;
                upscalers.upscaler.hr_resize_y = resizeY;
            }
            EditorGUILayout.EndVertical();
            

        }

        VisualElement modelsContainer = new VisualElement();
        private void OnAsync()
        {
            var upscalers = Target as SDHRUpscalers;
            if (upscalers == null) return;
            EditorCoroutineUtility.StartCoroutine(upscalers.ListModelsAsync(), this);
        }
    }
}
