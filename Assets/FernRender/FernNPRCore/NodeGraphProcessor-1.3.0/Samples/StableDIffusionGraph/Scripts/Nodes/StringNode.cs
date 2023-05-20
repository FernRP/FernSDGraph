using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using FernNPRCore.SDNodeGraph;

[System.Serializable, NodeMenuItem("String")]
public class StringNode : SDNode
{
    [Output(name = "Out"), SerializeField] public string output;

    public override string name => "String";

    public bool isShowString = true;
}