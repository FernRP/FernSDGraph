using GraphProcessor;
using Newtonsoft.Json;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Linq;

namespace FernNPRCore.SDNodeGraph
{
    [NodeCustomEditor(typeof(SDUpScalerNode))]
    public class SDUpScalerNodeView : SDNodeView
    {
        private SDUpScalerNode node;
        private DropdownField upScalerModelDropdown1;
        private DropdownField upScalerModelDropdown2;

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
            
            RefreshExpandedState();
        }

        private void OnAutoSaveToggleChange(ChangeEvent<bool> evt)
        {
            node.isAutoSave = evt.newValue;
        }
    }
}