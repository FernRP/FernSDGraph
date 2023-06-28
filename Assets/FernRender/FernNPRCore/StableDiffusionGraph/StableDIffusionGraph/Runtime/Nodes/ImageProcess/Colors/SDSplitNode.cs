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

namespace FernNPRCore.SDNodeGraph
{
	[System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Split")]
	public class SDSplitNode : SDNode
	{
		[Input(name = "Image")]
		public Texture inputImage;
		
		[Output("R")]
		public CustomRenderTexture outputR;
		[Output("G")]
		public CustomRenderTexture outputG;
		[Output("B")]
		public CustomRenderTexture outputB;
		[Output("A")]
		public CustomRenderTexture outputA;
		
		private Material outputRMat, outputGMat, outputBMat, outputAMat;
		private readonly Vector4 channelR = new Vector4(1, 0, 0, 1);
		private readonly Vector4 channelG = new Vector4(0, 1, 0, 1);
		private readonly Vector4 channelB = new Vector4(0, 0, 1, 1);
		private readonly Vector4 channelA = new Vector4(0, 0, 0, 1);

		public override bool showDefaultInspector => true;

		public override string name => "SD Split";

		protected override void Enable()
		{
			hasSettings = true;
			
			base.Enable();
			
			UpdateTempRenderTexture(ref outputR);
			UpdateTempRenderTexture(ref outputG);
			UpdateTempRenderTexture(ref outputB);
			UpdateTempRenderTexture(ref outputA);
			
			var mat = GetTempMaterial("Hidden/SDGraph/Separate");
			outputRMat = new Material(mat){ hideFlags = HideFlags.HideAndDontSave };
			outputGMat = new Material(mat){ hideFlags = HideFlags.HideAndDontSave };
			outputBMat = new Material(mat){ hideFlags = HideFlags.HideAndDontSave };
			outputAMat = new Material(mat){ hideFlags = HideFlags.HideAndDontSave };

			outputR.material = outputRMat;
			outputG.material = outputGMat;
			outputB.material = outputBMat;
			outputA.material = outputAMat;
		}

		protected override void Process(CommandBuffer cmd)
		{
			base.Process();
			if(inputImage == null) return;
			
			UpdateTempRenderTexture(ref outputR);
			UpdateTempRenderTexture(ref outputG);
			UpdateTempRenderTexture(ref outputB);
			UpdateTempRenderTexture(ref outputA);
			
			SetMaterialParams(outputRMat, channelR);
			SetMaterialParams(outputGMat, channelG);
			SetMaterialParams(outputBMat, channelB);
			SetMaterialParams(outputAMat, channelA);
			
			void SetMaterialParams(Material m, Vector4 channel)
			{
				SDUtil.SetTextureWithDimension(m, "_Source", inputImage);
				m.SetVector("_Channel", channel);
			}
			outputR.Update();
			outputG.Update();
			outputB.Update();
			outputA.Update();
		}
	}
}
