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
	[NodeCustomEditor(typeof(SDSetModelNode))]
	public class SDSetModelNodeView : SDNodeView
	{
		private string[] modelNames;
		private Button getModelListButton;
		private bool isDebug = true;

		public override void Enable()
		{
			base.Enable();
			var setModelNode = nodeTarget as SDSetModelNode;

			getModelListButton = new Button(GetModelList);
			getModelListButton.text = "Refresh Model List";
			extensionContainer.Add(getModelListButton);
			if (setModelNode!=null && setModelNode.modelNames!=null)
			{
				extensionContainer.Clear();
				// Create a VisualElement with a popup field
				var listContainer = new VisualElement();
				listContainer.style.flexDirection = FlexDirection.Row;
				listContainer.style.alignItems = Align.Center;
				listContainer.style.justifyContent = Justify.Center;

				List<string> stringList = new List<string>();
				stringList.AddRange(setModelNode.modelNames); 
				var popup = new PopupField<string>(stringList, setModelNode.currentIndex);

				// Add a callback to perform additional actions on value change
				popup.RegisterValueChangedCallback(evt =>
				{
					SDUtil.Log("Selected item: " + evt.newValue, isDebug);
					setModelNode.Model = evt.newValue;
					setModelNode.currentIndex = stringList.IndexOf(evt.newValue);
				});
				listContainer.Add(popup);
				extensionContainer.Add(getModelListButton);
				extensionContainer.Add(listContainer);
			}
			RefreshExpandedState();
		}

		private void GetModelList()
		{
			var setModelNode = nodeTarget as SDSetModelNode;
			if (setModelNode != null)
			{
				setModelNode.GetModelList(() =>
				{
					modelNames = setModelNode.modelNames;
					if (modelNames != null && modelNames.Length > 0)
					{
						extensionContainer.Clear();
						// Create a VisualElement with a popup field
						var listContainer = new VisualElement();
						listContainer.style.flexDirection = FlexDirection.Row;
						listContainer.style.alignItems = Align.Center;
						listContainer.style.justifyContent = Justify.Center;

						List<string> stringList = new List<string>();
						stringList.AddRange(setModelNode.modelNames);
						var popup = new PopupField<string>(stringList, setModelNode.currentIndex);

						// Add a callback to perform additional actions on value change
						popup.RegisterValueChangedCallback(evt =>
						{
							SDUtil.Log("Selected item: " + evt.newValue, isDebug);
							setModelNode.Model = evt.newValue;
							setModelNode.currentIndex = stringList.IndexOf(evt.newValue);
						});

						listContainer.Add(popup);
						extensionContainer.Add(getModelListButton);
						extensionContainer.Add(listContainer);
						RefreshExpandedState();
					}
				});
			}
		}
	}
}

