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
	[System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Preview Real")]
	public class SDPreviewRealNode : SDNode
	{
		[Input(name = "Image"), ShowAsDrawer]
		public Texture inputImage;
		[Input(name = "Seed")]
		public long seed;
		[Output(name = "Image")]
		public CustomRenderTexture outImage;

		public override string name => "SD Preview Real";

		protected override void Enable()
		{
			base.Enable();
		}

		protected override void Disable()
		{
			base.Disable();
			if (outImage != null)
				outImage.Release();
		}	

		protected override void Process()
		{
			base.Process();
			if(inputImage == null) return;
			Graphics.Blit(inputImage, outImage);
		}
	}
}
