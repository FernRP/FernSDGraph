using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphProcessor;
using UnityEngine;

namespace UnityEngine.SDGraph
{
    [System.Serializable]
    /// <summary>
    /// This is the base class for every node that is executed by the conditional processor, it takes an executed bool as input to 
    /// </summary>
    public abstract class SDProcessorNode : SDNode, IConditionalNode
    {
        /// <summary>
        /// Triggered when the node is processes
        /// </summary>
        public abstract IEnumerable<SDProcessorNode> GetExecutedNodes();

        // Assure that the executed field is always at the top of the node port section
    }
    
    [System.Serializable]
    public abstract class StartSDProcessorNode : SDProcessorNode
    {
        [Output(name = "Executes")] public ConditionalLink executes;
        
        public Action<float> onProgressUpdate;
        public Action onProgressFinish;
        public Action onProgressStart;

        public override IEnumerable<SDProcessorNode> GetExecutedNodes()
        {
            // Return all the nodes connected to the executes port
            return outputPorts.FirstOrDefault(n => n.fieldName == nameof(executes))
                .GetEdges().Select(e => e.inputNode as SDProcessorNode);
        }
        
        public override FieldInfo[] GetNodeFields()
        {
            var fields = base.GetNodeFields();
            return fields;
        }
    }

    [System.Serializable]
    public abstract class LinearSDProcessorNode : SDProcessorNode
    {
        // These booleans will controls wether or not the execution of the folowing nodes will be done or discarded.
        [Input(name = "Executed", allowMultiple = false)]
        public ConditionalLink executed;
        
        [Output(name = "Executes")] public ConditionalLink executes;
        
        public Action<float> onProgressUpdate;
        public Action onProgressFinish;
        public Action onProgressStart;

        public override IEnumerable<SDProcessorNode> GetExecutedNodes()
        {
            // Return all the nodes connected to the executes port
            return outputPorts.FirstOrDefault(n => n.fieldName == nameof(executes))
                .GetEdges().Select(e => e.inputNode as SDProcessorNode);
        }
        
        public override FieldInfo[] GetNodeFields()
        {
            var fields = base.GetNodeFields();
            Array.Sort(fields, (f1, f2) => f1.Name == nameof(executed) ? -1 : 1);
            return fields;
        }
    }
    
    [System.Serializable]
    public abstract class ForLoopSDProcessorNode : SDProcessorNode
    {
        // These booleans will controls wether or not the execution of the folowing nodes will be done or discarded.
        [Input(name = "Executed", allowMultiple = false)]
        public ConditionalLink executed;
        
        //[Output(name = "Executes")] public ConditionalLink executes;
        
        public Action<float> onProgressUpdate;
        public Action onProgressFinish;
        public Action onProgressStart;

        public override FieldInfo[] GetNodeFields()
        {
            var fields = base.GetNodeFields();
            Array.Sort(fields, (f1, f2) => f1.Name == nameof(executed) ? -1 : 1);
            return fields;
        }
    }

    [System.Serializable]
    public abstract class WaitableNode : LinearSDProcessorNode
    {
        [Output(name = "Execute After")] public ConditionalLink executeAfter;

        protected void ProcessFinished()
        {
            onProcessFinished?.Invoke(this);
        }

        [HideInInspector] public Action<WaitableNode> onProcessFinished;

        public IEnumerable<SDProcessorNode> GetExecuteAfterNodes()
        {
            return outputPorts.FirstOrDefault(n => n.fieldName == nameof(executeAfter))
                .GetEdges().Select(e => e.inputNode as SDProcessorNode);
        }
    }
}