using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using GraphProcessor;
using System;
using UnityEditor;

namespace FernNPRCore.SDNodeGraph
{
	
	public class StableDiffusionGraphView : BaseGraphView
	{
		public StableDiffusionGraphView(EditorWindow window) : base(window) {}

		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			evt.menu.AppendSeparator();

			foreach (var nodeMenuItem in NodeProvider.GetNodeMenuEntries())
			{
				var mousePos = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
				Vector2 nodePosition = mousePos;
				evt.menu.AppendAction("Create/" + nodeMenuItem.path,
					(e) => CreateNodeOfType(nodeMenuItem.type, nodePosition),
					DropdownMenuAction.AlwaysEnabled
				);
			}

			base.BuildContextualMenu(evt);
		}

		void CreateNodeOfType(Type type, Vector2 position)
		{
			RegisterCompleteObjectUndo("Added " + type + " node");
			AddNode(BaseNode.CreateFromType(type, position));
		}

		public override BaseNodeView AddRelayNode(PortView inputPort, PortView outputPort, Vector2 position)
		{
			var displayName = inputPort.portData.displayName;
			if (displayName.Equals("Executed") 
			    || displayName.Equals("Executes"))
			{
				var relayNode = BaseNode.CreateFromType<SDRelayNode>(position);
				var view = AddNode(relayNode) as SDRelayNodeView;

				if (outputPort != null)
					Connect(view.inputPortViews[0], outputPort);
				if (inputPort != null)
					Connect(inputPort, view.outputPortViews[0]);

				return view;
			}
			else
			{
				var relayNode = BaseNode.CreateFromType<RelayNode>(position);
				var view = AddNode(relayNode) as RelayNodeView;

				if (outputPort != null)
					Connect(view.inputPortViews[0], outputPort);
				if (inputPort != null)
					Connect(inputPort, view.outputPortViews[0]);

				return view;
			}
		}
	}
}
