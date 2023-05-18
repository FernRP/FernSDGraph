using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using FernNPRCore.StableDiffusionGraph;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEditor.Rendering;
using UnityEngine.Rendering;

[System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Camera Render")]
public class SDCameraRenderNode : BaseNode
{
	public override string name => "SD Camera Render";

	[Output("Color")] public Texture2D cameraColor;
	[Output("Normal")] public Texture2D cameranormal;
	[Output("Depth")] public Texture2D cameraDepth;
	public Camera camera;
	public int width = 512;
	public int height = 512;

	public RenderTexture colorTarget;
	public RenderTexture normalTarget;
	public RenderTexture depthTarget;
	public RenderTexture inpaintTarget;

	private RenderTexture originRT;

	private CommandBuffer cmd;

	protected override void Enable()
	{
		base.Enable();
		isUpdate = true;
		
		cmd = new CommandBuffer();
		cmd.name = "SD Camera Capture";
	}


	public override void Update()
	{
		base.Update();
		if(!IsValidate()) return;

		InitRenderTarget();
		RenderColor();
	}

	private bool IsValidate()
	{
		if (camera == null)
		{
			camera = Camera.main;
		}
		return camera != null;
	}

	private void InitRenderTarget()
	{
		if (colorTarget == null)
		{
			colorTarget = RenderTexture.GetTemporary(width, height, 24,
				camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
		}
		if (normalTarget == null)
		{
			normalTarget = RenderTexture.GetTemporary(width, height, 24,
				camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
		}
		if (depthTarget == null)
		{
			depthTarget = RenderTexture.GetTemporary(width, height, 24,
				camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
		}
		if (inpaintTarget == null)
		{
			inpaintTarget = RenderTexture.GetTemporary(width, height, 24,
				camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
		}
	}

	protected override void Process()
	{
		
	}

	protected override void Disable()
	{
		base.Disable();
		RenderTexture.ReleaseTemporary(colorTarget);
		RenderTexture.ReleaseTemporary(normalTarget);
		RenderTexture.ReleaseTemporary(depthTarget);
		RenderTexture.ReleaseTemporary(inpaintTarget);
	}

	protected void RenderColor()
	{
		originRT = camera.targetTexture;
		camera.targetTexture = colorTarget;
	    camera.Render();
	    camera.targetTexture = originRT;
	}
}
