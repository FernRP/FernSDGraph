using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace FernNPRCore.SDNodeGraph
{
    public class SDNode : BaseNode
    {
        /// <summary>
        /// List of all the nodes in the graph.
        /// </summary>
        /// <typeparam name="BaseNode"></typeparam>
        /// <returns></returns>
        [SerializeReference]
        public List< BaseNode >							nodes = new List< BaseNode >();
    }
}