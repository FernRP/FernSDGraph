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
using NodeGraphProcessor.Examples;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using NetAuthorizationUtil = FernNPRCore.SDNodeGraph.NetAuthorizationUtil;
using SDModel = FernNPRCore.SDNodeGraph.SDModel;
using SDUtil = FernNPRCore.SDNodeGraph.SDUtil;

namespace FernNPRCore.SDNodeGraph
{
	[System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Start")]
	public class SDStartNode : WaitableNode, IConditionalNode
	{
		[Output(name = "Executes")]
		public ConditionalLink		executes;
		[Output(name = "Server URL")] 
		public string outServerURL = "http://127.0.0.1:7860";
		
		[ChangeEvent(true)]
		public bool overrideSettings = false;
		
		[VisibleIf(nameof(overrideSettings), true)]
		public string serverURL = "http://127.0.0.1:7860";
		[VisibleIf(nameof(overrideSettings), true)]
		public bool useAuth = false;
		[VisibleIf(nameof(useAuth), true)]
		public string user = "";
		[VisibleIf(nameof(useAuth), true)]
		public string pass = "";
		
		public override string		name => "SD Start";

		protected override void Process()
		{
			if (overrideSettings&&!string.IsNullOrEmpty(serverURL))
			{
				SDGraphResource.SdGraphDataHandle.OverrideSettings = true;
				SDGraphResource.SdGraphDataHandle.OverrideServerURL = serverURL;
				SDGraphResource.SdGraphDataHandle.OverrideUseAuth = useAuth;
				SDGraphResource.SdGraphDataHandle.OverrideUsername = user;
				SDGraphResource.SdGraphDataHandle.OverridePassword = pass;
				outServerURL = serverURL;
			}
			else
			{
				SDGraphResource.SdGraphDataHandle.OverrideSettings = false;
				outServerURL = SDGraphResource.SdGraphDataHandle.serverURL;
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
