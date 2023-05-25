using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;
using UnityEngine.Rendering;

namespace FernNPRCore.SDNodeGraph
{
	[NodeCustomEditor(typeof(SDNode))]
	public class SDNodeView : BaseNodeView
	{
		protected VisualElement previewContainer;
		
		protected new StableDiffusionGraphView  owner => base.owner as StableDiffusionGraphView;

		protected new SDNode nodeTarget => base.nodeTarget as SDNode;
		const string stylesheetName = "SDGraphCommon";
		
		protected virtual string header => string.Empty;
		protected override bool hasSettings => nodeTarget.hasSettings;
		
		protected SDNodeSettingView settingsView;
		
		Label processTimeLabel;
		Image pinIcon;
		
		protected override VisualElement CreateSettingsView()
		{
			settingsView = new SDNodeSettingView(nodeTarget.settings, owner);
			settingsView.AddToClassList("RTSettingsView");

			var currentDim = nodeTarget.settings.dimension;
			settingsView.RegisterChangedCallback(() => {
				nodeTarget.OnSettingsChanged();

				// When the dimension is updated, we need to update all the node ports in the graph
				var newDim = nodeTarget.settings.dimension;
				if (currentDim != newDim)
				{
					// We delay the port refresh to let the settings finish it's update 
					schedule.Execute(() =>{ 
						{
							// Refresh ports on all the nodes in the graph
							nodeTarget.UpdateAllPortsLocal();
							RefreshPorts();
						}
					}).ExecuteLater(1);
					currentDim = newDim;
				}
			});

			return settingsView;
		}

		public override void Enable()
		{
			var stylesheet = Resources.Load<StyleSheet>(stylesheetName);
			if(!styleSheets.Contains(stylesheet))
				styleSheets.Add(stylesheet);
			
			nodeTarget.onExecuteFinish += UpdateTexturePreview;
			
			// Fix the size of the node
			style.width = nodeTarget.nodeWidth; 
			
			controlsContainer.AddToClassList("ControlsContainer");
			
			if (!string.IsNullOrEmpty(header))
			{
				var title = new Label(header);
				title.AddToClassList("PropertyEditorTitle");
				controlsContainer.Add(title);
			}
			
			pinIcon = new Image{ image = SDEditorUtils.pinIcon, scaleMode = ScaleMode.ScaleToFit };
			var pinButton = new Button(() => {
				if (nodeTarget.isPinned)
					UnpinView();
				else
					PinView();
			});
			pinButton.Add(pinIcon);
			if (nodeTarget.isPinned)
				PinView();

			pinButton.AddToClassList("PinButton");
			rightTitleContainer.Add(pinButton);

			if (nodeTarget.hasPreview)
			{
				var saveButton = new Button(OnSave);
				saveButton.style.backgroundImage = SDTextureHandle.SaveIcon;
				saveButton.style.width = 20;
				saveButton.style.height = 20;
				saveButton.style.alignSelf = Align.FlexEnd;
				saveButton.style.bottom = 5;
				rightTitleContainer.Add(saveButton);
			}

			previewContainer = new VisualElement();
			previewContainer.AddToClassList("Preview");
			controlsContainer.Add(previewContainer);
			UpdateTexturePreview();
			DrawDefaultInspector();
		}
		
		~SDNodeView()
		{
			
		}
		
		private void OnSave()
		{
			if (nodeTarget.previewTexture != null)
			{
				string path = EditorUtility.SaveFilePanel("Save texture as PNG", "Assets", $"img_preview.png", "png");
				if (path.Length != 0)
				{
					SDUtil.SaveAsLinearPNG(nodeTarget.previewTexture, path);
					AssetDatabase.Refresh();
					SDUtil.SetToNone(path);
				}
			}
		}
		
		void UpdatePorts()
		{
			nodeTarget.UpdateAllPorts();
			RefreshPorts();
		}
		
		internal void UnpinView()
		{
			nodeTarget.isPinned = false;
			nodeTarget.nodeLock = false;
			pinIcon.tintColor = Color.white;
			pinIcon.image = SDEditorUtils.pinIcon;
			pinIcon.transform.rotation = Quaternion.identity;
		}
		
		internal void PinView()
		{
			nodeTarget.isPinned = true;
			nodeTarget.nodeLock = true;
			pinIcon.tintColor = new Color32(245, 127, 23, 255);
			pinIcon.image = SDEditorUtils.unpinIcon;
		}
		
		void UpdateTexturePreview()
		{
			if (nodeTarget.hasPreview)
			{
				if (previewContainer != null && previewContainer.childCount == 0)
					CreateTexturePreview(previewContainer, nodeTarget);
			}
		}
		
		protected virtual void DrawPreviewSettings(Texture texture)
		{
			GUILayout.Space(6);

			if (Event.current.type == EventType.KeyDown)
			{
				if (Event.current.keyCode == KeyCode.Delete)
					owner.DelayedDeleteSelection();
			}

			using(new GUILayout.HorizontalScope(EditorStyles.toolbar, GUILayout.Height(12)))
				DrawPreviewToolbar(texture);
		}
		
		protected virtual void DrawPreviewToolbar(Texture texture)
		{
			EditorGUI.BeginChangeCheck();

			bool r = GUILayout.Toggle( (nodeTarget.previewMode & PreviewChannels.R) != 0,"R", EditorStyles.toolbarButton);
			bool g = GUILayout.Toggle( (nodeTarget.previewMode & PreviewChannels.G) != 0,"G", EditorStyles.toolbarButton);
			bool b = GUILayout.Toggle( (nodeTarget.previewMode & PreviewChannels.B) != 0,"B", EditorStyles.toolbarButton);
			bool a = GUILayout.Toggle( (nodeTarget.previewMode & PreviewChannels.A) != 0,"A", EditorStyles.toolbarButton);

			if (EditorGUI.EndChangeCheck())
			{
				owner.RegisterCompleteObjectUndo("Updated Preview Masks");
				nodeTarget.previewMode =
					(r ? PreviewChannels.R : 0) |
					(g ? PreviewChannels.G : 0) |
					(b ? PreviewChannels.B : 0) |
					(a ? PreviewChannels.A : 0);
			}

			if (texture.mipmapCount > 1)
			{
				GUILayout.Space(8);

				nodeTarget.previewMip = GUILayout.HorizontalSlider(nodeTarget.previewMip, 0.0f, texture.mipmapCount - 1, GUILayout.Width(64));
				GUILayout.Label($"Mip #{Mathf.RoundToInt(nodeTarget.previewMip)}", EditorStyles.toolbarButton);
			}

			GUILayout.FlexibleSpace();

			if(nodeTarget.canEditPreviewSRGB)
			{
				EditorGUI.BeginChangeCheck();

				bool srgb = GUILayout.Toggle(nodeTarget.previewSRGB, "sRGB", EditorStyles.toolbarButton);

				if (EditorGUI.EndChangeCheck())
				{
					owner.RegisterCompleteObjectUndo("Updated Preview Masks");
					nodeTarget.previewSRGB = srgb;
				}
			}
		}
		

		protected void CreateTexturePreview(VisualElement previewContainer, SDNode node)
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

		void CreateTexturePreviewImGUI(VisualElement previewContainer, SDNode node)
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
				
				DrawPreviewSettings(node.previewTexture);

				Rect previewRect = GetPreviewRect(node.previewTexture);
				DrawImGUIPreview(node, previewRect, node.previewSlice);
				
				DrawTextureInfoHover(previewRect, node.previewTexture);
				
			}) { name = "ImGUIPreview" };

			SDUtil.ScheduleAutoHide(previewContainer, owner);

			previewContainer.Add(previewImageSlice);
		}
		
		void DrawTextureInfoHover(Rect previewRect, Texture texture)
		{
			Rect infoRect = previewRect;
			infoRect.yMin += previewRect.height - 24;
			infoRect.height = 20;
			previewRect.yMax -= 4;

			// Check if the mouse is in the graph view rect:
			if (!(EditorWindow.mouseOverWindow is StableDiffusionGraphWindow mixtureWindow && mixtureWindow.GetCurrentGraph() == owner.graph))
				return;

			// On Hover : Transparent Bar for Preview with information
			if (previewRect.Contains(Event.current.mousePosition) && !infoRect.Contains(Event.current.mousePosition))
			{
				EditorGUI.DrawRect(infoRect, new Color(0, 0, 0, 0.65f));

				infoRect.xMin += 8;

				// Shadow
				GUI.color = Color.white;
				int slices = 1;
				GUI.Label(infoRect, $"{texture.width}x{texture.height} - {nodeTarget.settings.GetGraphicsFormat(owner.graph)}", EditorStyles.boldLabel);
			}
		}

		Rect GetPreviewRect(Texture texture)
		{
			float width = 256.0f; // force preview in width
			float scaleFactor = width / texture.width;
			float height = Mathf.Min(256.0f, texture.height * scaleFactor);
			return GUILayoutUtility.GetRect(1, width, 1, height);
		}

		protected virtual void DrawImGUIPreview(SDNode node, Rect previewRect, float currentSlice)
		{
			var outputNode = node as OutputNode;
			SDGraphResource.texture2DPreviewMaterial.SetTexture("_MainTex", node.previewTexture);
			SDGraphResource.texture2DPreviewMaterial.SetVector("_Size",
				new Vector4(node.previewTexture.width, node.previewTexture.height, 1, 1));
			SDGraphResource.texture2DPreviewMaterial.SetVector("_Channels", SDEditorUtils.GetChannelsMask(nodeTarget.previewMode));
			SDGraphResource.texture2DPreviewMaterial.SetFloat("_PreviewMip", 0);
			SDGraphResource.texture2DPreviewMaterial.SetFloat("_EV100", nodeTarget.previewEV100);
			SDGraphResource.texture2DPreviewMaterial.SetFloat("_IsSRGB", 0);
			SDGraphResource.texture2DPreviewMaterial.SetFloat("_IsSRGB", nodeTarget.previewSRGB? 1 : 0);

			if (Event.current.type == EventType.Repaint)
				EditorGUI.DrawPreviewTexture(previewRect, node.previewTexture, SDGraphResource.texture2DPreviewMaterial,
					ScaleMode.ScaleToFit, 0, 0);
		}
	}
}
