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

        protected override void OnInitialize()
        {
            base.OnInitialize();

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
                    if (lora.loraNames != null && lora.loraNames.Count > 0)
                    {
                        extensionContainer.Clear();
                        // Create a VisualElement with a popup field
                        var listContainer = new VisualElement();
                        listContainer.style.flexDirection = FlexDirection.Row;
                        listContainer.style.alignItems = Align.Center;
                        listContainer.style.justifyContent = Justify.Center;

                        var popup = new PopupField<string>(lora.loraNames, lora.currentIndex);

                        // Add a callback to perform additional actions on value change
                        popup.RegisterValueChangedCallback(evt =>
                        {
                            SDUtil.Log($"Selected lora: {evt.newValue}");
                            lora.lora = evt.newValue;
                            lora.currentIndex = lora.loraNames.IndexOf(evt.newValue);
                            if (lora.loraPrompts != null && lora.loraPrompts.ContainsKey(lora.lora))
                            {
                                lora.loraPrompt = lora.loraPrompts[lora.lora];
                            }
                        });

                        listContainer.Add(popup);
                        extensionContainer.Add(listContainer);
                        if(!extensionContainer.Contains(onGuiContainer))
                            extensionContainer.Add(onGuiContainer);
                        RefreshExpandedState();
                    }
                }
                ), this);
        }
    }
}
