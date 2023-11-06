using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

[System.Serializable, NodeMenuItem("Math/Sub")]
public class SubNode : BaseNode
{
    [Input(name = "A")] public double inputA;
    [Input(name = "B")] public double inputB;

    [Output(name = "Out")] public double output;

    public override string name => "Sub";

    protected override void Process()
    {
        output = inputA - inputB;
    }
}