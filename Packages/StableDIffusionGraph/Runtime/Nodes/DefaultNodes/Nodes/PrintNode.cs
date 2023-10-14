using System.Collections;
using System.Collections.Generic;
using FernNPRCore.SDNodeGraph;
using UnityEngine;
using GraphProcessor;
using NodeGraphProcessor.Examples;

[NodeMenuItem("Debug/Print")]
public class PrintNode : BaseNode
{
	[Input]
	public object	obj;

	public override string name => "Print";
}

[NodeMenuItem("Debug/Control Flow/Print")]
public class SDProcessorPrintNode : LinearSDProcessorNode
{
	[Input]
	public object	obj;

	public override string name => "Print";
}
