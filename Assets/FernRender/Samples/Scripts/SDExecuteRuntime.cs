using System.Collections;
using System.Collections.Generic;
using FernNPRCore.SDNodeGraph;
using NodeGraphProcessor.Examples;
using UnityEngine;

public class SDExecuteRuntime : MonoBehaviour
{
    public StableDiffusionGraph graph;

    private ConditionalProcessor executor;
    
    // Start is called before the first frame update
    [ContextMenu("Execute")]
    public void Execute()
    {
        graph.Open();
        executor = new ConditionalProcessor(graph);
        executor.UpdateComputeOrder();
        StopCoroutine(executor.RunAsync());
        StartCoroutine(executor.RunAsync());
    }
    
}
