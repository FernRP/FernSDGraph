using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;

namespace FernNPRCore.SDNodeGraph
{
	[NodeCustomEditor(typeof(SDControlNetNode))]
	public class SDControlNetNodeView : SDNodeView
	{
		private SDControlNetNode node;
		public override void Enable()
		{
			base.Enable();
			node = nodeTarget as SDControlNetNode;
			
			var button = new Button(OnAsync);
			button.text = "Refresh ControlNet Model";
			controlsContainer.Add(button);
		}
		
		private void OnAsync()
        {
            if(node == null) return;
            extensionContainer.Clear();

            if (node.modelList != null && node.modelList.Count > 0)
            {
                // Create a VisualElement with a popup field
                var listContainer = new VisualElement();
                listContainer.style.flexDirection = FlexDirection.Row;
                listContainer.style.alignItems = Align.Center;
                listContainer.style.justifyContent = Justify.Center;
            
                var popup = new PopupField<string>(node.modelList, node.currentModelListIndex);
            
                // Add a callback to perform additional actions on value change
                popup.RegisterValueChangedCallback(evt =>
                {
	                node.model = evt.newValue;
	                SDUtil.Log("ControlNet Choose: " + node.model);
	                node.currentModelListIndex = node.modelList.IndexOf(evt.newValue);
                });

                listContainer.Add(popup);
                
                extensionContainer.Add(listContainer);
            }
            
            if (node.moudleList != null && node.moudleList.Count > 0)
            {
                // Create a VisualElement with a popup field
                var listContainer = new VisualElement();
                listContainer.style.flexDirection = FlexDirection.Row;
                listContainer.style.alignItems = Align.Center;
                listContainer.style.justifyContent = Justify.FlexStart;
            
                var popup = new PopupField<string>(node.moudleList, node.currentMoudleListIndex);
            
                // Add a callback to perform additional actions on value change
                popup.RegisterValueChangedCallback(evt =>
                {
	                node.module = evt.newValue;
	                node.currentMoudleListIndex = node.moudleList.IndexOf(evt.newValue);
                });

                listContainer.Add(popup);
                
                extensionContainer.Add(listContainer);
            }
            
            RefreshExpandedState();
        }
		
	}
}

