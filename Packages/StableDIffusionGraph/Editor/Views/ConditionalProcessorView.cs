using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using GraphProcessor;
using NodeGraphProcessor.Examples;
using Unity.EditorCoroutines.Editor;

public class ConditionalProcessorView : PinnedElementView
{
    ConditionalProcessor	processor;
    BaseGraphView           graphView;

    public ConditionalProcessorView() => title = "SD Graph Processor";

    protected override void Initialize(BaseGraphView graphView)
    {
        processor = new ConditionalProcessor(graphView.graph);
        this.graphView = graphView;

        graphView.computeOrderUpdated += processor.UpdateComputeOrder;

        Button runButton = new Button(OnPlay) { name = "ActionButton", text = "Run" };

        content.Add(runButton);
    }

    void OnPlay()
    {
        EditorCoroutineUtility.StartCoroutine(processor.RunAsync(), graphView);
    }
}
