using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using FernNPRCore.SDNodeGraph;

[System.Serializable, NodeMenuItem("Constant/String")]
public class StringNode : SDNode
{
    [Input(name = "In")] public string inputString;
    [Output(name = "Out")] public string output;

    [HideInInspector]
    public string textFiledValue = "";

    public override string name => "String";

    [HideInInspector]
    public bool isShowString = true;

    protected override void Process()
    {
        base.Process();
        output = inputString + textFiledValue;
    }
}