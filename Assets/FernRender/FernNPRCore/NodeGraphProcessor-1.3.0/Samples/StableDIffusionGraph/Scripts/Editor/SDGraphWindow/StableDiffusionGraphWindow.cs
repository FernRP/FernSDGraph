using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraphProcessor;

namespace FernNPRCore.SDNodeGraph
{
    public class StableDiffusionGraphWindow : BaseGraphWindow
    {
        StableDiffusionGraph tmpGraph;

        [MenuItem("Window/Stable Diffusion Graph Menu")]
        public static StableDiffusionGraphWindow OpenWithTmpGraph()
        {
            var graphWindow = CreateWindow<StableDiffusionGraphWindow>();

            // When the graph is opened from the window, we don't save the graph to disk
            graphWindow.tmpGraph = ScriptableObject.CreateInstance<StableDiffusionGraph>();
            graphWindow.tmpGraph.hideFlags = HideFlags.HideAndDontSave;
            graphWindow.InitializeGraph(graphWindow.tmpGraph);

            graphWindow.Show();

            return graphWindow;
        }

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
                graphView = new StableDiffusionGraphView(this);
                graphView.Add(new MiniMapView(graphView));
                graphView.Add(new StableDiffusionToolbarView(graphView));
            }

            rootView.Add(graphView);
        }
    }
}