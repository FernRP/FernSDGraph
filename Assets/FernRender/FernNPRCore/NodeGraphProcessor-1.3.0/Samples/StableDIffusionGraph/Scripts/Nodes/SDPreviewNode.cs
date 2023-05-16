using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using NodeGraphProcessor.Examples;
using Unity.VisualScripting;
using Debug = UnityEngine.Debug;

namespace FernNPRCore.SDNodeGraph
{
	[System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Preview")]
	public class SDPreviewNode : WaitableNode
	{
		[ChangeEvent(true)]
		[Input(name = "Image")]
		public Texture2D input;
		[Input(name = "Seed")]
		public long seed;
		[Output(name = "Image")]
		public Texture2D outImage;

		public override string name => "SD Preview";

		protected override void Enable()
		{
			base.Enable();
			onAfterEdgeConnected += (e) =>
			{
				
			};
		}

		protected override void Process()
		{
			base.Process();
			GetPort(nameof(input), null).PushData();
			if(input != null) Debug.Log(input.width);
		}
	}
}
