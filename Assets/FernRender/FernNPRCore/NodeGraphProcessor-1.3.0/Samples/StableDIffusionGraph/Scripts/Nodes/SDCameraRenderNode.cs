using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEditor.Rendering;
using UnityEngine.Rendering;

[System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Camera Render")]
public class SDCameraRenderNode : BaseNode
{
	public override string name => "SD Camera Render";

	[Output("Color")] public Texture2D color;
	[Output("Normal")] public Texture2D normal;
	[Output("Depth")] public Texture2D depth;
	[Input("Camera"), SerializeField] public GameObject cameraObj;
	public int width = 512;
	public int height = 512;

	public RenderTexture colorTarget;
	public RenderTexture normalTarget;
	public RenderTexture depthTarget;
	public RenderTexture inpaintTarget;

	private RenderTexture originRT;

	private CommandBuffer cmd;
	private Camera camera;

	protected override void Enable()
	{
		base.Enable();
		if(cameraObj == null) return;
		camera = cameraObj.GetComponent<Camera>();
		if(camera == null) return;
		
		cmd = new CommandBuffer();
		cmd.name = "SD Camera Capture";
		
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
		
		RenderColor();
	}

	protected override void Process()
	{
		base.Process();
		if(cameraObj == null) return;
		camera = cameraObj.GetComponent<Camera>();
		if(camera == null) return;

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

		RenderColor();
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
