using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;
using UnityEngine.SDGraph;
using Unity.EditorCoroutines.Editor;

namespace UnityEditor.SDGraph
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
			OnAsync();
			controlsContainer.Add(button);
		}

		private void UpdateModelList()
		{
			extensionContainer.Clear();
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
			
			RefreshExpandedState();
		}
		
		private void UpdateMoudleList()
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
			
			RefreshExpandedState();
		}
		
		private void OnAsync()
        {
            if(node == null) return;
            node.modelList.Clear();
           
	        EditorCoroutineUtility.StartCoroutine(node.ControlNetModelListAsync(UpdateModelList), owner);
            
            node.moudleList.Clear();
            EditorCoroutineUtility.StartCoroutine(node.ControlNetMoudleList(UpdateMoudleList), owner);

        }
		
	}
}

