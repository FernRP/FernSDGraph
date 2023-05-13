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
	[NodeCustomEditor(typeof(SDStartNode))]
	public class SDStartNodeView : BaseNodeView
	{
		public override void Enable()
		{
			SDStartNode comparisonNode = nodeTarget as SDStartNode;
			DrawDefaultInspector();
			
		}
	}
}

