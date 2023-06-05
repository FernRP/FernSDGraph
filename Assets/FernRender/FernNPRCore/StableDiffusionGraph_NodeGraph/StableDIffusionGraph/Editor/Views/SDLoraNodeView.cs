using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FernNPRCore.SDNodeGraph;
using GraphProcessor;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FernNPRCore.SDNodeGraph
{
    
    [NodeCustomEditor(typeof(SDLoraNode))]
    public class SDLoraNodeView : SDNodeView
    {
        private SDLoraNode node;
        private TextField textArea;
        bool foldout = false;
        PopupField<string> popup = new PopupField<string>();
        VisualElement listContainer;

        public override void Enable()
        {
            base.Enable();
            
            node = nodeTarget as SDLoraNode;
            
            listContainer = new VisualElement();

            textArea = new TextField(-1, true, false, '*') { value = node.loraPrompt };
            textArea.Children().First().style.unityTextAlign = TextAnchor.UpperLeft;
            textArea.style.whiteSpace = WhiteSpace.Normal;
            textArea.style.width = 200;
            textArea.style.height = float.NaN;
            textArea.RegisterValueChangedCallback(v => {
                owner.RegisterCompleteObjectUndo("Edit lora prompt");
                node.loraPrompt = v.newValue;
            });
            extensionContainer.Add(textArea);
            
            var button = new Button(OnAsync);
            button.text = "Refresh Lora Models";
            
            extensionContainer.Add(button);
            
            onGuiContainer = new IMGUIContainer(OnGUI);
            
            // Add a callback to perform additional actions on value change
            popup.RegisterValueChangedCallback(evt =>
            {
                SDUtil.Log($"Selected lora: {evt.newValue}");
                node.lora = evt.newValue;
                node.currentIndex = node.loraNames.IndexOf(evt.newValue);
                if (node.loraPrompts != null && node.loraPrompts.TryGetValue(node.lora, out var prompt))
                {
                    node.loraPrompt = prompt;
                    SDUtil.Log(node.loraPrompt);
                }
                else
                {
                    SDUtil.LogWarning("Can't Get Lora Prompt");
                }
            });
            
            RefreshExpandedState();
        }
        
        private void OnAsync()
        {
            if(node == null) return;
            EditorCoroutineUtility.StartCoroutine(node.ListLoraAsync(
                () => {
                    listContainer.Clear();
                    if (extensionContainer.Contains(listContainer))
                    {
                        extensionContainer.Remove(listContainer);
                    }
                    // Create a VisualElement with a popup field
                    listContainer.style.flexDirection = FlexDirection.Row;
                    listContainer.style.alignItems = Align.Center;
                    listContainer.style.justifyContent = Justify.Center;

                    popup.choices = node.loraNames;
                    popup.index = node.currentIndex;
                    
                    listContainer.Add(popup);
                    extensionContainer.Add(listContainer);
                    if(!extensionContainer.Contains(onGuiContainer))
                        extensionContainer.Add(onGuiContainer);
                    RefreshExpandedState();
                }
            ), this);
        }
        
        IMGUIContainer onGuiContainer;
        List<string> loraWeightPresetName;
        
        void OnGUI()
        {
            if (node == null) return;
            var styleCheckbox = new GUIStyle(EditorStyles.toggle);

            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, "Config");

            if (foldout)
            {
                if (loraWeightPresetName == null || loraWeightPresetName.Count == 0)
                {
                    loraWeightPresetName = SDGraphResource.SdGraphDataHandle.loraBlockWeightPresets.Keys.ToList();
                    
                }
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Use Lora Bloc kWeight", GUILayout.MaxWidth(120));
                node.useLoraBlockWeight = EditorGUILayout.Toggle(
                    node.useLoraBlockWeight,
                    styleCheckbox,
                    GUILayout.MaxWidth(150)
                );
                EditorGUILayout.EndHorizontal();
                if (node.useLoraBlockWeight)
                {
                    node.currentLoraBlockWeightPresetIndex = EditorGUILayout.Popup(node.currentLoraBlockWeightPresetIndex, loraWeightPresetName.ToArray());
                    node.loraBlockWeightPresetName = loraWeightPresetName[node.currentLoraBlockWeightPresetIndex];
                }
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}
