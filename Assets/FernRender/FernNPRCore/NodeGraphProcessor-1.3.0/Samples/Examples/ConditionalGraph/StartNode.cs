using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using FernNPRCore.SDNodeGraph;
using FernNPRCore.StableDiffusionGraph;
using GraphProcessor;
using Newtonsoft.Json;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using NetAuthorizationUtil = FernNPRCore.SDNodeGraph.NetAuthorizationUtil;
using SDModel = FernNPRCore.SDNodeGraph.SDModel;
using SDUtil = FernNPRCore.SDNodeGraph.SDUtil;

namespace NodeGraphProcessor.Examples
{
	[System.Serializable, NodeMenuItem("Conditional/Start")]
	public class StartNode : WaitableNode, IConditionalNode
	{
		[Output(name = "Executes")]
		public ConditionalLink		executes;
		[Output(name = "Server URL")] 
		public string outServerURL = "http://127.0.0.1:7860";
		
		public bool overrideSettings = false;
		
		[VisibleIf(nameof(overrideSettings), true)]
		public string serverURL = "http://127.0.0.1:7860";
		[VisibleIf(nameof(overrideSettings), true)]
		public bool useAuth = false;
		[VisibleIf(nameof(useAuth), true)]
		public string user = "";
		[VisibleIf(nameof(useAuth), true)]
		public string pass = "";

		public override string		name => "Start";

		protected override void Process()
		{
			if (overrideSettings&&!string.IsNullOrEmpty(serverURL))
			{
				SDGraphDataHandle.Instance.OverrideSettings = true;
				SDGraphDataHandle.Instance.OverrideServerURL = serverURL;
				SDGraphDataHandle.Instance.OverrideUseAuth = useAuth;
				SDGraphDataHandle.Instance.OverrideUsername = user;
				SDGraphDataHandle.Instance.OverridePassword = pass;
				outServerURL = serverURL;
			}
			else
			{
				SDGraphDataHandle.Instance.OverrideSettings = false;
				outServerURL = SDGraphDataHandle.Instance.serverURL;
			}
			
			// Return all the nodes connected to the executes port
		}

		public override IEnumerable< ConditionalNode >GetExecutedNodes()
		{
			base.GetExecutedNodes();
			return GetOutputNodes().OfType<ConditionalNode>();
		}
		
		public override FieldInfo[] GetNodeFields() => base.GetNodeFields();
	}
}
