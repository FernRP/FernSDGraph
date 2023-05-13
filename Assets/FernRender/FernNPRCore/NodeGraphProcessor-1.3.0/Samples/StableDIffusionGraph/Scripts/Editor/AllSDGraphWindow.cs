using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;

namespace FernNPRCore.SDNodeGraph
{

    public class AllSDGraphWindow : BaseGraphWindow
    {
        BaseGraph			tmpGraph;
        CustomToolbarView	toolbarView;

        
        protected override void OnDestroy()
        {
            graphView?.Dispose();
            DestroyImmediate(tmpGraph);
        }

        protected override void InitializeWindow(BaseGraph graph)
        {
            titleContent = new GUIContent("Stable Diffusion Graph");

            if (graphView == null)
            {
                graphView = new AllGraphView(this);
                toolbarView = new CustomToolbarView(graphView);
                graphView.Add(toolbarView);
            }

            rootView.Add(graphView);
        }

        protected override void InitializeGraphView(BaseGraphView view)
        {
            // graphView.OpenPinned< ExposedParameterView >();
            // toolbarView.UpdateButtonStatus();
        }
    }

}
