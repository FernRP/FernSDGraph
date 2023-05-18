using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;
using System.Linq;

[NodeCustomEditor(typeof(StringNode))]
public class StringNodeView : BaseNodeView
{
	private TextField textArea;
	private StringNode node;
	Button togglePreviewButton = null;
	
	const string stylesheetName = "SDGraphCommon";

	public override void Enable()
	{
		var stylesheet = Resources.Load<StyleSheet>(stylesheetName);
		if(!styleSheets.Contains(stylesheet))
			styleSheets.Add(stylesheet);
		
		node = nodeTarget as StringNode;

		togglePreviewButton = new Button(() =>
		{
			node.isShowString = !node.isShowString;
			UpdatePreviewCollapseState();
		});
		togglePreviewButton.ClearClassList();
		togglePreviewButton.AddToClassList("PreviewToggleButton");
		controlsContainer.Add(togglePreviewButton);
		
		textArea = new TextField(-1, true, false, '*') { value = node.output };
		textArea.Children().First().style.unityTextAlign = TextAnchor.UpperLeft;
		textArea.style.whiteSpace = WhiteSpace.Normal;
		textArea.style.width = 200;
		textArea.style.height = float.NaN;
		textArea.RegisterValueChangedCallback(v => {
			owner.RegisterCompleteObjectUndo("Edit string node");
			node.output = v.newValue;
		});
		controlsContainer.Add(textArea);
	}
	
	void UpdatePreviewCollapseState()
	{
		if (!node.isShowString)
		{
			if (controlsContainer.Contains(textArea))
			{
				controlsContainer.Remove(textArea);
			}
			togglePreviewButton.RemoveFromClassList("Collapsed");
		}
		else
		{
			if (!controlsContainer.Contains(textArea))
			{
				controlsContainer.Add(textArea);
			}
			togglePreviewButton.AddToClassList("Collapsed");
		}
	}

	public override void Disable()
	{
		base.Disable();
		OnExpandAction = null;
	}
}