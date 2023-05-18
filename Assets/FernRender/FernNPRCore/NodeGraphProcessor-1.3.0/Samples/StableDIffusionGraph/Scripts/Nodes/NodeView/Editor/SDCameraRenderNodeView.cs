using System.Collections;
using System.Collections.Generic;
using FernNPRCore.SDNodeGraph;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;

namespace FernNPRCore.SDNodeGraph
{
	[NodeCustomEditor(typeof(SDCameraRenderNode))]
	public class SDCameraRenderNodeView : SDGraphNodeView
	{
		private SDCameraRenderNode node;
		public override void Enable()
		{
			base.Enable();
			node = nodeTarget as SDCameraRenderNode;
			OnExpandAction = null;
			OnExpandAction += OnExpandChange;
		}

		public override void Disable()
		{
			base.Disable();
			OnExpandAction = null;
		}

		private void OnExpandChange(bool isExpand)
		{
			if (!expanded)
			{
				if(mainContainer.Contains(controlsContainer))
					mainContainer.Remove(controlsContainer);
			}

			else
			{
				if(!mainContainer.Contains(controlsContainer))
					mainContainer.Add(controlsContainer);
			}
		}

		protected override void OnFieldChanged(string fieldName, object value)
		{
			base.OnFieldChanged(fieldName, value);
			if (fieldName is nameof(node.width) or nameof(node.height))
			{
				node.ResetRTResolution();
			}
		}
	}
}
