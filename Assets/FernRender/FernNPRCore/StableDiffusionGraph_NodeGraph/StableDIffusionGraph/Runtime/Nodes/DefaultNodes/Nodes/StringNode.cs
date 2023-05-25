using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using FernNPRCore.SDNodeGraph;

[System.Serializable, NodeMenuItem("String")]
public class StringNode : SDNode
{
    [Output(name = "Out")] public string output;

    public override string name => "String";

    [HideInInspector]
    public bool isShowString = true;
}