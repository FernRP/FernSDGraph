using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.SDGraph;

[System.Serializable, NodeMenuItem("Control Flow/Switch")]
public class SwitchNode : LinearSDProcessorNode
{
    [Input] public List<object> inputs = new List<object>();

    [Input] public int index;


    [Output(name = "Out")] public object output;

    public override string name => "Switch";

    public override void Process()
    {
        if (inputs == null || index < 0 || index >= inputs.Count)
            return;
        output = inputs[index];
    }
}