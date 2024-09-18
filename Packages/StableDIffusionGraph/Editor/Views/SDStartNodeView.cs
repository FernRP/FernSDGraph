using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;
using NodeGraphProcessor.Examples;
using Unity.EditorCoroutines.Editor;
using UnityEngine.SDGraph;

namespace UnityEditor.SDGraph
{
	[NodeCustomEditor(typeof(SDStartNode))]
	public class SDStartNodeView : SDNodeView
	{
		private Button setOverrideServerBtn;
		private Button executeBtn;
		private bool isDebug = true;
		private SDStartNode node;

		private ConditionalProcessor executor;
		private SDProcessGraphProcessor processor;
		
		public override void Enable()
		{
			base.Enable();
			node = nodeTarget as SDStartNode;
			if (node == null) return;

			executor = new ConditionalProcessor(owner.graph);
			executeBtn = new Button(StartExecute);
			executeBtn.text = "SD Start";
			mainContainer.Add(executeBtn);
			processor = new SDProcessGraphProcessor(owner.graph);
			RefreshExpandedState();
		}

		private void StartExecute()
		{
			processor.Run();
			EditorCoroutineUtility.StartCoroutine(executor.RunAsyncWithStartNode(node), owner);
		}
	}
}
