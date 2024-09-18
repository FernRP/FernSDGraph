using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SDGraph;
using GraphProcessor;
using Debug = UnityEngine.Debug;

namespace NodeGraphProcessor.Examples
{
    public class ConditionalProcessor : BaseGraphProcessor
    {
        List<BaseNode> processList;
        List<SDStartNode> startNodeList;
        public bool pause;
        private bool m_IsExecuting = false;
        public bool IsExecuting => m_IsExecuting;

        Dictionary<BaseNode, List<BaseNode>> nonConditionalDependenciesCache =
            new Dictionary<BaseNode, List<BaseNode>>();


        public IEnumerator<BaseNode> currentGraphExecution { get; private set; } = null;

        // static readonly float   maxExecutionTimeMS = 100; // 100 ms max execution time to avoid infinite loops

        /// <summary>
        /// Manage graph scheduling and processing
        /// </summary>
        /// <param name="graph">Graph to be processed</param>
        public ConditionalProcessor(BaseGraph graph) : base(graph)
        {
        }

        public override void UpdateComputeOrder()
        {
            // Gather start nodes:
            startNodeList = graph.nodes.Where(n => n is SDStartNode).Select(n => n as SDStartNode).ToList();

            // In case there is no start node, we process the graph like usual
            if (startNodeList.Count == 0)
            {
                processList = graph.nodes.OrderBy(n => n.computeOrder).ToList();
            }
            else
            {
                nonConditionalDependenciesCache.Clear();
                // Prepare the cache of non-conditional node execution
            }
        }

        public override void Run()
        {
            
        }
        
        public IEnumerator RunAsyncWithStartNode(SDStartNode startNode)
        {
            if (m_IsExecuting)
            {
                SDUtil.Log("SD is In Executing");
                yield break;
            }

            m_IsExecuting = true;
            startNodeList.Clear();
            startNodeList.Add(startNode);
            processList = graph.nodes.OrderBy(n => n.computeOrder).ToList();
            yield return RunAsync();
            m_IsExecuting = false;
        }

        public override IEnumerator RunAsync()
        {
            if (startNodeList.Count == 0)
            {
                RunTheGraph();
            }
            else
            {
                Stack<BaseNode> nodeToExecute = new Stack<BaseNode>();
                // Add all the start nodes to the execution stack
                startNodeList.ForEach(s => nodeToExecute.Push(s));
                // Execute the whole graph:
                yield return RunTheGraph(nodeToExecute);
            }
        }

        private void WaitedRun(Stack<BaseNode> nodesToRun)
        {
            // Execute the waitable node:
            var enumerator = RunTheGraph(nodesToRun);

            while (enumerator.MoveNext())
                ;
        }

        IEnumerable<BaseNode> GatherNonConditionalDependencies(BaseNode node)
        {
            Stack<BaseNode> dependencies = new Stack<BaseNode>();
            
            dependencies.Push(node);

            while (dependencies.Count > 0)
            {
                var dependency = dependencies.Pop();

                foreach (var d in dependency.GetInputNodes().Where(n => !(n is IConditionalNode)))
                    dependencies.Push(d);

                if (dependency != node)
                    yield return dependency;
            }
        }

        private void RunTheGraph()
        {
            int count = processList.Count;

            for (int i = 0; i < count; i++)
            {
                processList[i].OnProcess();
            }
        }

        private IEnumerator RunTheGraph(Stack<BaseNode> nodeToExecute)
        {
            HashSet<BaseNode> nodeDependenciesGathered = new HashSet<BaseNode>(); // 跟踪已经收集到的节点的依赖关系

            while (nodeToExecute.Count > 0)
            {
                var node = nodeToExecute.Pop();
                
                
                // TODO: maxExecutionTimeMS

                if (node == null) continue;

                // In case the node is conditional, then we need to execute it's non-conditional dependencies first
                if (node is IConditionalNode)
                {

                    if (!nodeDependenciesGathered.Contains(node))
                    {
                        nodeToExecute.Push(node);
                        nodeDependenciesGathered.Add(node);
                        foreach (var nonConditionalNode in GatherNonConditionalDependencies(node))
                        {
                            nodeToExecute.Push(nonConditionalNode);
                        }
                    }
                    else
                    {
                        SDUtil.Log("Current Execute Node: " + node.name);

                        // Execute the conditional node:
                        yield return node.OnExecute();
                        
                        //yield return node;

                        // And select the next nodes to execute:
                        switch (node)
                        {
                            // special code path for the loop node as it will execute multiple times the same nodes
                            case ForLoopNode forLoopNode:
                                
                                forLoopNode.index = forLoopNode.start - 1; // Initialize the start index
                                
                                for (int i = (int)forLoopNode.start; i < forLoopNode.end; i++)
                                {
                                    foreach (var n in forLoopNode.GetExecutedNodesLoopBody())
                                        nodeToExecute.Push(n);
                                }
                                
                                foreach (var n in forLoopNode.GetExecutedNodesLoopCompleted())
                                    nodeToExecute.Push(n);
                                
                                break;
                            // another special case for waitable nodes, like "wait for a coroutine", wait x seconds", etc.
                            // case WaitableNode waitableNode:
                            //     SDUtil.Log("WaitableNode");
                            //     foreach (var n in waitableNode.GetExecutedNodes())
                            //         nodeToExecute.Push(n);
                            //
                            //     waitableNode.onExecuteFinish -= WaitAffterExecute;
                            //     waitableNode.onExecuteFinish += WaitAffterExecute;
                            //     void WaitAffterExecute()
                            //     {
                            //        
                            //         Debug.LogError("1");
                            //     }
                            //
                            //     break;
                            case IConditionalNode cNode:
                                foreach (var n in cNode.GetExecutedNodes())
                                    nodeToExecute.Push(n);
                                break;
                            default:
                                Debug.LogError($"Conditional node {node} not handled");
                                break;
                        }
                        

                        nodeDependenciesGathered.Remove(node);
                    }
                }
                else
                {
                    node.OnProcess();
                    yield return node;
                }
            }
        }
    }
}