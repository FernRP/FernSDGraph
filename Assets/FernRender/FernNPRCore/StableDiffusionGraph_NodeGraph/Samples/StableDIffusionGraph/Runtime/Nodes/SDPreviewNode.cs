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
	public class SDPreviewNode : SDNode
	{
		[Input(name = "Image")]
		public Texture inputImage;
		[Input(name = "Seed")]
		public long seed;
		[Output(name = "Image")]
		public CustomRenderTexture outImage;

		public override string name => "SD Preview";

		public override Texture previewTexture => outImage;

		protected override void Enable()
		{
			hasPreview = true;
			hasSettings = true;
			base.Enable();

			UpdateTempRenderTexture(ref outImage);
		}

		protected override void Disable()
		{
			base.Disable();
			if(outImage != null)
				outImage.Release();
		}

		protected override void Process()
		{
			base.Process();	
			UpdateTempRenderTexture(ref outImage);
			if(inputImage != null) Graphics.Blit(inputImage, outImage);
			outImage.Update();
		}
	}
}
