using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;

[System.Serializable, NodeMenuItem("Constant/Number")]
public class DoubleNode : BaseNode
{
    [Output("Out")]
	public double		output;
	
    [Input("In")]
	public double		input;

	public override string name => "Float";

	protected override void Process() => output = input;
}