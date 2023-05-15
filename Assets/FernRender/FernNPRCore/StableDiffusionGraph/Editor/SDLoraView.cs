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
    [CustomNodeView(typeof(SDLora))]
    public class SDLoraView : NodeView
    {
        bool foldout = false;
        PopupField<string> popup = new PopupField<string>();

        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            PortView loRAPrompt = GetInputPort("LoRAPrompt");

            var lora = Target as SDLora;
            if(lora == null) return;
            OnAsync();
            var button = new Button(OnAsync);
            button.style.backgroundImage = SDTextureHandle.RefreshIcon;
            button.style.width = 20;
            button.style.height = 20;
            button.style.alignSelf = Align.FlexEnd;
            button.style.bottom = 0;
            button.style.right = 0;
            titleButtonContainer.Add(button);
            onGuiContainer = new IMGUIContainer(OnGUI);
            extensionContainer.Add(onGuiContainer);
            
            // Add a callback to perform additional actions on value change
            popup.RegisterValueChangedCallback(evt =>
            {
                SDUtil.Log($"Selected lora: {evt.newValue}");
                lora.lora = evt.newValue;
                lora.currentIndex = lora.loraNames.IndexOf(evt.newValue);
                if (lora.loraPrompts != null && lora.loraPrompts.TryGetValue(lora.lora, out var prompt))
                {
                    lora.loraPrompt = prompt;
                    SDUtil.Log(lora.loraPrompt);
                }
                else
                {
                    SDUtil.LogWarning("Can't Get Lora Prompt");
                }
                loRAPrompt.OnUpdatePortViewElement(this);
            });
            RefreshExpandedState();
        }

        public override void OnDirty()
        {
            base.OnDirty();
            RefreshExpandedState();
        }

        IMGUIContainer onGuiContainer;
        List<string> loraWeightPresetName;

        void OnGUI()
        {
            var lora = Target as SDLora;
            if (lora == null) return;
            var styleTextArea = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true
            };
            var styleCheckbox = new GUIStyle(EditorStyles.toggle);

            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, "Config");

            if (foldout)
            {
                if (loraWeightPresetName == null || loraWeightPresetName.Count == 0)
                {
                    loraWeightPresetName = SDDataHandle.Instance.loraBlockWeightPresets.Keys.ToList();
                }
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("UseLoraBlockWeight", GUILayout.MaxWidth(120));
                lora.useLoraBlockWeight = EditorGUILayout.Toggle(
                    lora.useLoraBlockWeight,
                    styleCheckbox,
                    GUILayout.MaxWidth(150)
                );
                EditorGUILayout.EndHorizontal();
                if (lora.useLoraBlockWeight)
                {
                    lora.currentLoraBlockWeightPresetIndex = EditorGUILayout.Popup(lora.currentLoraBlockWeightPresetIndex, loraWeightPresetName.ToArray());
                    lora.loraBlockWeightPresetName = loraWeightPresetName[lora.currentLoraBlockWeightPresetIndex];
                }
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }


        private void OnAsync()
        {
            var lora = Target as SDLora;
            if(lora == null) return;
            EditorCoroutineUtility.StartCoroutine(lora.ListLoraAsync(
            () => {
                    extensionContainer.Clear();
                    // Create a VisualElement with a popup field
                    var listContainer = new VisualElement();
                    listContainer.style.flexDirection = FlexDirection.Row;
                    listContainer.style.alignItems = Align.Center;
                    listContainer.style.justifyContent = Justify.Center;

                    popup.choices = lora.loraNames;
                    popup.index = lora.currentIndex;
                    
                    listContainer.Add(popup);
                    extensionContainer.Add(listContainer);
                    if(!extensionContainer.Contains(onGuiContainer))
                        extensionContainer.Add(onGuiContainer);
                    RefreshExpandedState();
                }
            ), this);
        }
    }
}
