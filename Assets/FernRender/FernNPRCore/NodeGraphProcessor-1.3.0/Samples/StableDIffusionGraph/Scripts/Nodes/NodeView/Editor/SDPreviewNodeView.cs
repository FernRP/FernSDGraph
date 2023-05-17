using System.Collections;
using System.Collections.Generic;
using FernNPRCore.SDNodeGraph;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;
using NodeGraphProcessor.Examples;
using Unity.VisualScripting;

[NodeCustomEditor(typeof(SDPreviewNode))]
public class SDPreviewNodeView : BaseNodeView
{

	private SDPreviewNode previewNode;
	
	public override void Enable()
	{
		DrawDefaultInspector();
		previewNode = nodeTarget as SDPreviewNode;
		previewNode.onProcessed += UpdateImageView;
	}

	private void UpdateImageView(WaitableNode obj)
	{
		Debug.Log("1");
		var portView = previewNode.GetPort(nameof(previewNode.inputImage), null);
		portView.PushData();
		if(previewNode.inputImage == null) return;
		extensionContainer.Clear();
		var previewImage = new Image();
		previewImage.scaleMode = ScaleMode.ScaleAndCrop;
		previewImage.image =previewNode.inputImage;
		int scaleWidth = (int)((float)previewImage.image.width);
		int scaleHeight = (int)((float)previewImage.image.height);
		style.maxWidth = scaleWidth;
		style.maxHeight = scaleHeight;
		previewImage.style.maxWidth = scaleWidth;
		var asptio = (float)previewImage.image.width / (float)scaleWidth;
		previewImage.style.maxHeight = previewImage.image.height / asptio;
		extensionContainer.Add(previewImage);
		RefreshExpandedState();
	}

	public override void Disable()
	{
		base.Disable();
		previewNode.onProcessed -= UpdateImageView;
	}

	
	void UpdateImageView()
	{
		var portView = previewNode.GetPort(nameof(previewNode.inputImage), null);
		portView.PushData();
		if(previewNode.inputImage == null) return;
		extensionContainer.Clear();
		var previewImage = new Image();
		previewImage.scaleMode = ScaleMode.ScaleAndCrop;
		previewImage.image =previewNode.inputImage;
		int scaleWidth = (int)((float)previewImage.image.width);
		int scaleHeight = (int)((float)previewImage.image.height);
		style.maxWidth = scaleWidth;
		style.maxHeight = scaleHeight;
		previewImage.style.maxWidth = scaleWidth;
		var asptio = (float)previewImage.image.width / (float)scaleWidth;
		previewImage.style.maxHeight = previewImage.image.height / asptio;
		extensionContainer.Add(previewImage);
		RefreshExpandedState();
	}

	public override bool RefreshPorts()
	{
		bool result = base.RefreshPorts();
		UpdateImageView();
		return result;
	}

	protected override void OnFieldChanged(string fieldName, object value)
	{
		base.OnFieldChanged(fieldName, value);
		Debug.Log("1");
	}
}