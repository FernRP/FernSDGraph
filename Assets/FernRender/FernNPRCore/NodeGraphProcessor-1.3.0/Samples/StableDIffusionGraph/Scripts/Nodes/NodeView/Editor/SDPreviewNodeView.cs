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

	private SDPreviewNode node;
	private Image previewImage;
	
	public override void Enable()
	{
		DrawDefaultInspector();
		node = nodeTarget as SDPreviewNode;
		previewImage = new Image();
		node.onProcessed += UpdateImageView;
		OnExpandAction += OnExpandChange;
		NotifyNodeChanged();
	}

	private void OnExpandChange(bool isExpand)
	{
		if (!expanded)
		{
			bottomPortContainer.Clear();
			return;
		}

		UpdateImageView();
	}

	public override void Disable()
	{
		base.Disable();
		node.onProcessed -= UpdateImageView;
		OnExpandAction -= OnExpandChange;
	}

	void UpdateImageView()
	{
		if(node.inputImage == null) return;
		if(bottomPortContainer.Contains(previewImage))
			bottomPortContainer.Remove(previewImage);
		previewImage.scaleMode = ScaleMode.ScaleAndCrop;
		previewImage.image = node.inputImage;
		bottomPortContainer.Add(previewImage);
		RefreshExpandedState();
	}

	public override bool RefreshPorts()
	{
		bool result = base.RefreshPorts();
		UpdateImageView();
		return result;
	}
}