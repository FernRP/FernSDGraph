using System.Collections;
using System.Collections.Generic;
using UnityEditor.SDGraph;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;
using UnityEngine.SDGraph;

namespace UnityEditor.SDGraph
{
	[NodeCustomEditor(typeof(SDCameraRenderNode))]
	public class SDCameraRenderNodeView : SDNodeView
	{
		private SDCameraRenderNode node;
		public override void Enable()
		{
			base.Enable();
			node = nodeTarget as SDCameraRenderNode;
			OnExpandAction = null;
			//OnExpandAction += OnExpandChange;
			NotifyNodeChanged();
		}

		public override void Disable()
		{
			base.Disable();
			OnExpandAction = null;
		}

		protected override void DrawDefaultInspector(bool fromInspector = false)
		{
			base.DrawDefaultInspector(fromInspector);
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
	}
}
