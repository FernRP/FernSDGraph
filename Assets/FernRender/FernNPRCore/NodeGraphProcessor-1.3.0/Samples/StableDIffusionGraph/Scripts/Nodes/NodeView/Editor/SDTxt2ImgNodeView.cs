using System.Collections;
using System.Collections.Generic;
using FernNPRCore.SDNodeGraph;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;

[NodeCustomEditor(typeof(SDTxt2ImgNode))]
public class SDTxt2ImgNodeView : BaseNodeView
{
	public override void Enable()
	{
		DrawDefaultInspector();
		var node = nodeTarget as SDTxt2ImgNode;
		NotifyNodeChanged();

		node.onPortsUpdated += (e) =>
		{
			Debug.Log("123");
		};
	}
}