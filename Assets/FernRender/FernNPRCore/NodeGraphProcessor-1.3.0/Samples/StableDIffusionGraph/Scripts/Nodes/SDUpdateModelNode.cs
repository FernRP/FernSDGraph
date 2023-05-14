using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FernGraph;
using FernNPRCore.StableDiffusionGraph;
using Newtonsoft.Json;
using NodeGraphProcessor.Examples;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine.Events;

namespace FernNPRCore.SDNodeGraph
{
	
	[System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD UpdateModel")]
	public class SDUpdateModelNode : WaitableNode
	{
		[GraphProcessor.Input(name = "In")]
		public float                input;
        
      
		public override string		name => "SD UpdateModel";

		protected override void Process()
		{
			
		}
		
		
	}
}
