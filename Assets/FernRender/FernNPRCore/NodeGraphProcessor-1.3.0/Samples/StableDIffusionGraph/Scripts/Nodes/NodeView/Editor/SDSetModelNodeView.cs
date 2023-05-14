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
	public class SDSetModelNodeView : BaseNodeView
	{
		public override void Enable()
		{
			SDSetModelNode comparisonNode = nodeTarget as SDSetModelNode;
			DrawDefaultInspector();

			var getModelListButton = new Button(GetModelList);
			getModelListButton.text = "Get Model List";
			extensionContainer.Add(getModelListButton);
			RefreshExpandedState();
		}

		private void GetModelList()
		{
			var setModelNode = nodeTarget as SDSetModelNode;
			if (setModelNode != null)
			{
				setModelNode.GetModelList();
			}
		}
	}
}

