using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.SDGraph;

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

    public override void Process()
    {
        base.Process();
        output = inputString + textFiledValue;
    }
}