using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraphProcessor;
using UnityEngine.SDGraph;

namespace UnityEditor.SDGraph
{
    public class StableDiffusionGraphWindow : BaseGraphWindow
    {
        StableDiffusionGraph tmpGraph;
        
        public StableDiffusionGraph GetCurrentGraph() => graph as StableDiffusionGraph;

        protected override void OnDestroy()
        {
            graphView?.Dispose();
            DestroyImmediate(tmpGraph);
        }

        public static StableDiffusionGraphWindow Open(StableDiffusionGraph graph)
        {
            // Focus the window if the graph is already opened
            var stableDiffusionWindows = Resources.FindObjectsOfTypeAll<StableDiffusionGraphWindow>();
            foreach (var stableDiffusionWindow in stableDiffusionWindows)
            {
                if (stableDiffusionWindow.graph == graph)
                {
                    stableDiffusionWindow.Show();
                    stableDiffusionWindow.Focus();
                    return stableDiffusionWindow;
                }
            }

            var graphWindow = EditorWindow.CreateWindow<StableDiffusionGraphWindow>();

            graphWindow.Show();
            graphWindow.Focus();

            graphWindow.InitializeGraph(graph);

            return graphWindow;
        }

        public StableDiffusionToolbarView toolBarView;
        
        protected override void InitializeWindow(BaseGraph graph)
        {
            titleContent = new GUIContent("Stable Diffusion Graph");

            if (graphView == null)
            {
                graphView = new StableDiffusionGraphView(this);
                toolBarView = new StableDiffusionToolbarView(graphView);
                //graphView.Add(new MiniMapView(graphView));
                graphView.Add(toolBarView);
            }

            rootView.Add(graphView);
        }

        protected override void Update()
        {
            base.Update();
            if (toolBarView is { isAlwaysUpdate: true })
            {
                toolBarView.RunProcessor();
            }
        }
    }
}