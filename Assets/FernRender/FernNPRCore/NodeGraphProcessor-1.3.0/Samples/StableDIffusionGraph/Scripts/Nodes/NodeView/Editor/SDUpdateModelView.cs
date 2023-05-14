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
	[NodeCustomEditor(typeof(SDUpdateModelNode))]
	public class SDUpdateModelView : BaseNodeView
	{
		public override void Enable()
		{
			SDSetModelNode comparisonNode = nodeTarget as SDSetModelNode;
			DrawDefaultInspector();
			
			extensionContainer.Add(new Button());
			RefreshExpandedState();
		}
	}
}

