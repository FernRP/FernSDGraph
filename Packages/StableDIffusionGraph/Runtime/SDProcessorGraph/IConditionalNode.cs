using System.Collections.Generic;
using System.Reflection;
using GraphProcessor;

namespace UnityEngine.SDGraph
{
	public interface IConditionalNode
	{
		IEnumerable<SDProcessorNode> GetExecutedNodes();

		FieldInfo[] GetNodeFields(); // Provide a custom order for fields (so conditional links are always at the top of the node)
	}
}