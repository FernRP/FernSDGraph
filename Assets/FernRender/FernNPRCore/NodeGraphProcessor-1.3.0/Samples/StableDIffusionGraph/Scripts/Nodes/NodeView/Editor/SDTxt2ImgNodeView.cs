using System.Collections;
using System.Collections.Generic;
using FernNPRCore.SDNodeGraph;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;

[NodeCustomEditor(typeof(SDTxt2ImgNode))]
public class SDTxt2ImgNodeView : BaseNodeView
{
	private SDTxt2ImgNode node;
	private Image previewImage;
	private bool isDebug = true;
	public override void Enable()
	{
		DrawDefaultInspector();
		node = nodeTarget as SDTxt2ImgNode;
		previewImage = new Image();
		node.onExecuteFinish += UpdatePreviewImage;
		
		NotifyNodeChanged();
	}
	
	public override void Disable()
	{
		base.Disable();
		node.onExecuteFinish -= UpdatePreviewImage;
	}

	private void UpdatePreviewImage()
	{
		SDUtil.Log("Txt2Img UpdatePreview", isDebug);
		if(node.outputImage == null) return;
		if(contentContainer.Contains(previewImage))
			bottomPortContainer.Remove(previewImage);
		previewImage.scaleMode = ScaleMode.ScaleAndCrop;
		previewImage.image = node.outputImage;
		bottomPortContainer.Add(previewImage);
		RefreshExpandedState();
	}
}