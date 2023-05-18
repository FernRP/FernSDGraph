using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;

namespace FernNPRCore.SDNodeGraph
{

	public class SDGraphNodeView : BaseNodeView
	{
		protected VisualElement previewContainer;

		protected new BaseNode nodeTarget => base.nodeTarget as BaseNode;
		const string stylesheetName = "SDGraphCommon";

		public override void Enable()
		{
			var stylesheet = Resources.Load<StyleSheet>(stylesheetName);
			if(!styleSheets.Contains(stylesheet))
				styleSheets.Add(stylesheet);
			
			nodeTarget.onExecuteFinish += UpdateTexturePreview;

			previewContainer = new VisualElement();
			previewContainer.AddToClassList("Preview");
			controlsContainer.Add(previewContainer);
			UpdateTexturePreview();
			DrawDefaultInspector();
		}
		
		void UpdateTexturePreview()
		{
			if (nodeTarget.hasPreview)
			{
				if (previewContainer != null && previewContainer.childCount == 0)
					CreateTexturePreview(previewContainer, nodeTarget);
			}
		}

		protected void CreateTexturePreview(VisualElement previewContainer, BaseNode node)
		{
			previewContainer.Clear();
			
			if (node.previewTexture == null)
				return;
			
			VisualElement texturePreview = new VisualElement();
			previewContainer.Add(texturePreview);

			CreateTexturePreviewImGUI(texturePreview, node);

			previewContainer.name = node.previewTexture.dimension.ToString();

			Button togglePreviewButton = null;
			togglePreviewButton = new Button(() =>
			{
				nodeTarget.isPreviewCollapsed = !nodeTarget.isPreviewCollapsed;
				UpdatePreviewCollapseState();
			});
			togglePreviewButton.ClearClassList();
			togglePreviewButton.AddToClassList("PreviewToggleButton");
			previewContainer.Add(togglePreviewButton);

			UpdatePreviewCollapseState();

			void UpdatePreviewCollapseState()
			{
				if (!nodeTarget.isPreviewCollapsed)
				{
					texturePreview.style.display = DisplayStyle.Flex;
					togglePreviewButton.RemoveFromClassList("Collapsed");
					nodeTarget.previewVisible = true;
				}
				else
				{
					texturePreview.style.display = DisplayStyle.None;
					togglePreviewButton.AddToClassList("Collapsed");
					nodeTarget.previewVisible = false;
				}
			}
		}

		void CreateTexturePreviewImGUI(VisualElement previewContainer, BaseNode node)
		{
			if (node.showPreviewExposure)
			{
				var previewExposure = new Slider(0, 10)
				{
					label = "Preview EV100",
					value = node.previewEV100,
				};
				previewExposure.RegisterValueChangedCallback(e => { node.previewEV100 = e.newValue; });
				previewContainer.Add(previewExposure);
			}

			var previewImageSlice = new IMGUIContainer(() =>
			{
				if (node.previewTexture == null)
					return;

				Rect previewRect = GetPreviewRect(node.previewTexture);
				DrawImGUIPreview(node, previewRect, node.previewSlice);
			}) { name = "ImGUIPreview" };

			SDUtil.ScheduleAutoHide(previewContainer, owner);

			previewContainer.Add(previewImageSlice);
		}

		Rect GetPreviewRect(Texture texture)
		{
			float width = 256.0f; // force preview in width
			float scaleFactor = width / texture.width;
			float height = Mathf.Min(256.0f, texture.height * scaleFactor);
			return GUILayoutUtility.GetRect(1, width, 1, height);
		}

		protected virtual void DrawImGUIPreview(BaseNode node, Rect previewRect, float currentSlice)
		{
			var outputNode = node as OutputNode;
			SDGraphResource.texture2DPreviewMaterial.SetTexture("_MainTex", node.previewTexture);
			SDGraphResource.texture2DPreviewMaterial.SetVector("_Size",
				new Vector4(node.previewTexture.width, node.previewTexture.height, 1, 1));
			SDGraphResource.texture2DPreviewMaterial.SetVector("_Channels", Vector4.one);
			SDGraphResource.texture2DPreviewMaterial.SetFloat("_PreviewMip", 0);
			SDGraphResource.texture2DPreviewMaterial.SetFloat("_EV100", nodeTarget.previewEV100);
			SDGraphResource.texture2DPreviewMaterial.SetFloat("_IsSRGB", 0);

			if (Event.current.type == EventType.Repaint)
				EditorGUI.DrawPreviewTexture(previewRect, node.previewTexture, SDGraphResource.texture2DPreviewMaterial,
					ScaleMode.ScaleToFit, 0, 0);
		}
	}
}
