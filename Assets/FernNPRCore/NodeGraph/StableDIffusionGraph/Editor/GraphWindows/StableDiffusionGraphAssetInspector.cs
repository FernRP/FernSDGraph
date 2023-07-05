using System.Collections;
using System.Collections.Generic;
using FernNPRCore.SDNodeGraph;
using UnityEngine;
using UnityEditor;
using GraphProcessor;
using UnityEngine.UIElements;

namespace FernNPRCore.SDNodeGraph
{
	[CustomEditor(typeof(StableDiffusionGraph), true)]
	public class StableDiffusionGraphAssetInspector : GraphInspector
	{
		protected override void CreateInspector()
		{
			base.CreateInspector();

			root.Add(new Button(() => EditorWindow.GetWindow<StableDiffusionGraphWindow>().InitializeGraph(target as BaseGraph))
			{
				text = "Open window"
			});
		}
	}

}
