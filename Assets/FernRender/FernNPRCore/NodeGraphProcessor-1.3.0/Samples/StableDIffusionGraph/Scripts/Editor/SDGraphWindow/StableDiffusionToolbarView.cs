using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraphProcessor;
using Status = UnityEngine.UIElements.DropdownMenuAction.Status;

namespace FernNPRCore.SDNodeGraph
{
    public class StableDiffusionToolbarView : ToolbarView
    {
        public StableDiffusionToolbarView(BaseGraphView graphView) : base(graphView)
        {
        }

        protected override void AddButtons()
        {
            AddButton("Center", graphView.ResetPositionAndZoom);

            // bool processorVisible = graphView.GetPinnedElementStatus< ProcessorView >() != Status.Hidden;
            // showProcessor = AddToggle("Show Processor", processorVisible, (v) => graphView.ToggleView< ProcessorView>());
            bool exposedParamsVisible = graphView.GetPinnedElementStatus< ExposedParameterView >() != Status.Hidden;
            showParameters = AddToggle("Show Parameters", exposedParamsVisible, (v) => graphView.ToggleView< ExposedParameterView>());
            AddButton("Show In Project", () => EditorGUIUtility.PingObject(graphView.graph), false);
            bool conditionalProcessorVisible =
                graphView.GetPinnedElementStatus<ConditionalProcessorView>() != Status.Hidden;
            AddToggle("Show SD Graph Processor", conditionalProcessorVisible,
                (v) => graphView.ToggleView<ConditionalProcessorView>());
        }
    }
}