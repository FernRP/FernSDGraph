using GraphProcessor;
using Newtonsoft.Json;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Linq;
using UnityEngine.SDGraph;

namespace UnityEditor.SDGraph
{
    [NodeCustomEditor(typeof(SDUpScalerNode))]
    public class SDUpScalerNodeView : SDNodeView
    {
        private SDUpScalerNode node;
        private DropdownField upScalerModelDropdown1;
        private DropdownField upScalerModelDropdown2;
        private TextField savePathTxtField;

        public override void Enable()
        {
            base.Enable();
            node = nodeTarget as SDUpScalerNode;
            if (node == null)
            {
                return;
            }

            var upScaleModelList = SDGraphResource.SdGraphDataHandle.UpscalerModel.ToList();
            var label1 = new Label("UpScale 1");
            label1.style.width = StyleKeyword.Auto;
            label1.style.marginRight = 5;
            upScalerModelDropdown1 = new DropdownField(upScaleModelList, node.upscaler1_index);
            upScalerModelDropdown1.RegisterValueChangedCallback(e =>
            {
                node.upscaler_1 = e.newValue;
                node.upscaler1_index = upScaleModelList.IndexOf(e.newValue);
            });
            upScalerModelDropdown1.style.flexGrow = 1;
            upScalerModelDropdown1.style.maxWidth = 140;
            var container1 = new VisualElement();
            container1.style.flexDirection = FlexDirection.Row;
            container1.style.alignItems = Align.Center;
            container1.Add(label1);
            container1.Add(upScalerModelDropdown1);
            var label2 = new Label("UpScale 2");
            label2.style.width = StyleKeyword.Auto;
            label2.style.marginRight = 5;
            upScalerModelDropdown2 = new DropdownField(upScaleModelList, node.upscaler2_index);
            upScalerModelDropdown2.RegisterValueChangedCallback(e =>
            {
                node.upscaler_2 = e.newValue;
                node.upscaler2_index = upScaleModelList.IndexOf(e.newValue);
            });
            upScalerModelDropdown2.style.flexGrow = 1;
            upScalerModelDropdown2.style.maxWidth = 140;
            
            var container2 = new VisualElement();
            container2.style.flexDirection = FlexDirection.Row;
            container2.style.alignItems = Align.Center;
            container2.Add(label2);
            container2.Add(upScalerModelDropdown2);
            extensionContainer.Add(container1);
            extensionContainer.Add(container2);
            
            // Save Path
            var saveToggle = new Toggle("Auto Save");
            saveToggle.labelElement.style.minWidth = 62;
            saveToggle.style.width = StyleKeyword.Auto;
            saveToggle.style.flexGrow = 1;
            saveToggle.style.marginLeft = 0;
            saveToggle.style.marginRight = 0;
            saveToggle.RegisterValueChangedCallback(OnAutoSaveToggleChange);
            extensionContainer.Add(saveToggle);
            
            var savePath = new Label("Save Path");
            savePath.style.width = StyleKeyword.Auto;
            savePath.style.marginRight = 5;
            savePathTxtField = new TextField();
            savePathTxtField.value = node.savePath;
            savePathTxtField.style.flexGrow = 1;
            savePathTxtField.style.maxWidth = 160;
            var savePathBtn = new Button(SavePathBtn);
            savePathBtn.style.backgroundImage = SDTextureHandle.OpenFolderIcon;
            savePathBtn.style.width = 20;
            savePathBtn.style.height = 20;				
            savePathBtn.style.right = 3;
             
            var containersavePath = new VisualElement();
            containersavePath.style.flexDirection = FlexDirection.Row;
            containersavePath.style.alignItems = Align.Center;
            containersavePath.Add(savePath);
            containersavePath.Add(savePathTxtField);
            containersavePath.Add(savePathBtn);
            
            extensionContainer.Add(containersavePath);

            RefreshExpandedState();
        }

        private void SavePathBtn()
        {
            string path;
            if (string.IsNullOrEmpty(node.savePath))
            {
                path = EditorUtility.SaveFilePanel("Save texture as PNG", "Assets", $"img_preview.png", "png");
            }
            else
            {
                path = EditorUtility.SaveFilePanel("Save texture as PNG", node.savePath, $"img_preview.png", "png");
            }
            savePathTxtField.value = path;
            node.savePath = path;
        }

        private void OnAutoSaveToggleChange(ChangeEvent<bool> evt)
        {
            node.isAutoSave = evt.newValue;
        }
    }
}