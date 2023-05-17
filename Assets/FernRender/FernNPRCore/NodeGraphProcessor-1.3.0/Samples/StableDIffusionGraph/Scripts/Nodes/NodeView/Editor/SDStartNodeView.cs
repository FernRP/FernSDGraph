using System;
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
		private Button setOverrideServerBtn;
		private bool isDebug = true;
		public override void Enable()
		{
			DrawDefaultInspector();
			if (nodeTarget is not SDStartNode node) return;
			if (!node.overrideSettings) return;
			setOverrideServerBtn = new Button(SetServerURL);
			setOverrideServerBtn.text = "Override ServerURL";
			extensionContainer.Add(setOverrideServerBtn);
			RefreshExpandedState();
		}
		
		private void SetServerURL()
		{
			var node = nodeTarget as SDStartNode;
			SDGraphResource.SdGraphDataHandle.OverrideSettings = true;
			if(node == null) return;
			SDGraphResource.SdGraphDataHandle.OverrideServerURL = node.serverURL;
			SDGraphResource.SdGraphDataHandle.OverrideUseAuth = node.useAuth;
			SDGraphResource.SdGraphDataHandle.OverrideUsername = node.user;
			SDGraphResource.SdGraphDataHandle.OverridePassword = node.pass;
			node.outServerURL = node.serverURL;
			SDUtil.Log($"Override ServerURL: {node.serverURL}", isDebug);
		}

		protected override void OnFieldChanged(string fieldName, object value)
		{			
			var node = nodeTarget as SDStartNode;
			if(node == null) return;
			if (fieldName == nameof(node.overrideSettings))
			{
				bool fileValue = (bool)value;
				if (!fileValue)
				{
					extensionContainer.Clear();
					RefreshExpandedState();
				}
				else
				{
					extensionContainer.Add(setOverrideServerBtn);
					RefreshExpandedState();
				}
			}
		}
	}
}
