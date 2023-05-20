using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraphProcessor;
using NodeGraphProcessor.Examples;
using Unity.EditorCoroutines.Editor;
using Status = UnityEngine.UIElements.DropdownMenuAction.Status;

namespace FernNPRCore.SDNodeGraph
{
    public class StableDiffusionToolbarView : ToolbarView
    {
        ConditionalProcessor executor;
        BaseGraphProcessor processor;

        public StableDiffusionToolbarView(BaseGraphView graphView) : base(graphView)
        {
            
        }
        

        protected override void AddButtons()
        {
            
            executor = new ConditionalProcessor(graphView.graph);
            graphView.computeOrderUpdated += executor.UpdateComputeOrder;
            
            processor = new ProcessGraphProcessor(graphView.graph);
            graphView.computeOrderUpdated += processor.UpdateComputeOrder;
            
            AddButton("Save", graphView.SaveGraphToDisk);
            AddButton("Stable Diffusion Execute", RunExecute);
            AddButton("Processor", RunProcessor);

            bool exposedParamsVisible = graphView.GetPinnedElementStatus< ExposedParameterView >() != Status.Hidden;
            showParameters = AddToggle("Show Parameters", exposedParamsVisible, (v) => graphView.ToggleView< ExposedParameterView>());
            AddButton("Show In Project", () => EditorGUIUtility.PingObject(graphView.graph), false);
            AddButton("Center", graphView.ResetPositionAndZoom);
        }
        
        void RunExecute()
        {
            EditorCoroutineUtility.StartCoroutine(executor.RunAsync(), this);
        }
        
        void RunProcessor()
        {
            processor.Run();
        }
    }
}