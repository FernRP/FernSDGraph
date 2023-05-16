using System.Collections;
using System.Collections.Generic;
using FernNPRCore.SDNodeGraph;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;

[NodeCustomEditor(typeof(SDPreviewNode))]
public class SDPreviewNodeView : BaseNodeView
{
	public override void Enable()
	{
		DrawDefaultInspector();
		var node = nodeTarget as SDPreviewNode;
		node.onPortsUpdated += (e) =>
		{
			Debug.Log(e);
		};
	}

	protected override void OnFieldChanged(string fieldName, object value)
	{
		base.OnFieldChanged(fieldName, value);
		Debug.Log("1");
	}
}