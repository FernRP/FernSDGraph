using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using NodeGraphProcessor.Examples;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

namespace UnityEngine.SDGraph
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

		public bool alwaysUpdate = false;

		public override string name => "SD Preview";

		public override Texture previewTexture => outImage;

		protected override void Enable()
		{
			hasPreview = true;
			hasSettings = true;
			isUpdate = alwaysUpdate;
			base.Enable();

			UpdateTempRenderTexture(ref outImage);
		}

		public override void Update()
		{
			base.Update();
			if (alwaysUpdate)
			{				
				UpdateTempRenderTexture(ref outImage);
				if(inputImage != null) Graphics.Blit(inputImage, outImage);
				outImage.Update();
			}
		}

		protected override void Process(CommandBuffer cmd)
		{
			base.Process();	
			UpdateTempRenderTexture(ref outImage);
			if(inputImage != null) Graphics.Blit(inputImage, outImage);
			outImage.Update();
		}
	}
}
