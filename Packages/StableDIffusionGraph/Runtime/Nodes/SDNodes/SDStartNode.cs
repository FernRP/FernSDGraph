using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine.SDGraph;
using GraphProcessor;
using Newtonsoft.Json;
using NodeGraphProcessor.Examples;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using NetAuthorizationUtil = UnityEngine.SDGraph.NetAuthorizationUtil;
using SDModel = UnityEngine.SDGraph.SDModel;
using SDUtil = UnityEngine.SDGraph.SDUtil;

namespace UnityEngine.SDGraph
{
	[System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Start")]
	public class SDStartNode : StartSDProcessorNode
	{
		private string outServerURL = "http://127.0.0.1:7860";
		
		public override string		name => "SD Start";
		
		protected override void Enable()
		{
			base.Enable();
		}

		protected override IEnumerator Execute()
		{
			SDGraphResource.SdGraphDataHandle.OverrideSettings = false;
			outServerURL = SDGraphResource.SdGraphDataHandle.GetServerURL();
			yield return null;
		}
	}
}
