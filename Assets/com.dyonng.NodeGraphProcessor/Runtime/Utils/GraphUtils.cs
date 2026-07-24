using System;
using System.Collections.Generic;

namespace GraphProcessor
{
    public static class GraphUtils
    {
        enum State
        {
            White,
            Grey,
            Black,
        }

        class TarversalNode
        {
            public BaseNode node;
            public List<TarversalNode> inputs = new List<TarversalNode>();
            public List<TarversalNode> outputs = new List<TarversalNode>();
            public State    state = State.White;

            public TarversalNode(BaseNode node) { this.node = node; }
        }

        // A structure made for easy graph traversal
        class TraversalGraph
        {
            public List<TarversalNode> nodes = new List<TarversalNode>();
            public List<TarversalNode> outputs = new List<TarversalNode>();
        }

        static TraversalGraph ConvertGraphToTraversalGraph(BaseGraph graph)
        {
            TraversalGraph g = new TraversalGraph();
            Dictionary<BaseNode, TarversalNode> nodeMap = new Dictionary<BaseNode, TarversalNode>();

            foreach (var node in graph.nodes)
            {
                var tn = new TarversalNode(node);
                g.nodes.Add(tn);
                nodeMap[node] = tn;

                if (graph.graphOutputs.Contains(node))
                    g.outputs.Add(tn);
            }

            foreach (var tn in g.nodes)
            {
                tn.inputs = new List<TarversalNode>();
                foreach (var n in tn.node.GetInputNodes())
                    if (nodeMap.TryGetValue(n, out var mapped))
                        tn.inputs.Add(mapped);

                tn.outputs = new List<TarversalNode>();
                foreach (var n in tn.node.GetOutputNodes())
                    if (nodeMap.TryGetValue(n, out var mapped))
                        tn.outputs.Add(mapped);
            }

            return g;
        }

        public static List<BaseNode> DepthFirstSort(BaseGraph g)
        {
            var graph = ConvertGraphToTraversalGraph(g);
            List<BaseNode> depthFirstNodes = new List<BaseNode>();

            foreach (var n in graph.nodes)
                DFS(n);

            void DFS(TarversalNode n)
            {
                if (n.state == State.Black)
                    return;
                
                n.state = State.Grey;

                if (n.node is ParameterNode parameterNode && parameterNode.accessor == ParameterAccessor.Get)
                {
                    foreach (var setter in graph.nodes)
                    {
                        bool isMatchingSetter = setter.node is ParameterNode p &&
                            p.parameterGUID == parameterNode.parameterGUID &&
                            p.accessor == ParameterAccessor.Set;

                        if (isMatchingSetter && setter.state == State.White)
                            DFS(setter);
                    }
                }
                else
                {
                    foreach (var input in n.inputs)
                    {
                        if (input.state == State.White)
                            DFS(input);
                    }
                }

                n.state = State.Black;

                // Only add the node when his children are completely visited
                depthFirstNodes.Add(n.node);
            }

            return depthFirstNodes;
        }

        public static void FindCyclesInGraph(BaseGraph g, Action<BaseNode> cyclicNode)
        {
            var graph = ConvertGraphToTraversalGraph(g);
            List<TarversalNode> cyclicNodes = new List<TarversalNode>();

            foreach (var n in graph.nodes)
                DFS(n);

            void DFS(TarversalNode n)
            {
                if (n.state == State.Black)
                    return;
                
                n.state = State.Grey;

                foreach (var input in n.inputs)
                {
                    if (input.state == State.White)
                        DFS(input);
                    else if (input.state == State.Grey)
                        cyclicNodes.Add(n);
                }
                n.state = State.Black;
            }

            cyclicNodes.ForEach((tn) => cyclicNode?.Invoke(tn.node));
        }

        // Combines FindCyclesInGraph + DepthFirstSort into a single traversal-graph build
        // (each of those builds its own from scratch, which is wasteful when called back to back).
        internal static List<BaseNode> SortAndFindCycles(BaseGraph g, Action<BaseNode> cyclicNode)
        {
            var graph = ConvertGraphToTraversalGraph(g);

            // Derive graph outputs (leaf nodes) from the traversal graph we just built,
            // instead of a separate full GetOutputNodes() walk over every node.
            g.graphOutputs.Clear();
            foreach (var tn in graph.nodes)
                if (tn.outputs.Count == 0)
                    g.graphOutputs.Add(tn.node);

            // Pass 1: cycle detection (same logic as FindCyclesInGraph)
            List<TarversalNode> cyclicNodes = new List<TarversalNode>();

            foreach (var n in graph.nodes)
                DFSCycle(n);

            void DFSCycle(TarversalNode n)
            {
                if (n.state == State.Black)
                    return;

                n.state = State.Grey;

                foreach (var input in n.inputs)
                {
                    if (input.state == State.White)
                        DFSCycle(input);
                    else if (input.state == State.Grey)
                        cyclicNodes.Add(n);
                }
                n.state = State.Black;
            }

            cyclicNodes.ForEach((tn) => cyclicNode?.Invoke(tn.node));

            // Reset traversal state before the second pass
            foreach (var n in graph.nodes)
                n.state = State.White;

            // Pass 2: depth-first sort (same logic as DepthFirstSort), reusing the same traversal graph
            List<BaseNode> depthFirstNodes = new List<BaseNode>();

            foreach (var n in graph.nodes)
                DFSSort(n);

            void DFSSort(TarversalNode n)
            {
                if (n.state == State.Black)
                    return;

                n.state = State.Grey;

                if (n.node is ParameterNode parameterNode && parameterNode.accessor == ParameterAccessor.Get)
                {
                    foreach (var setter in graph.nodes)
                    {
                        bool isMatchingSetter = setter.node is ParameterNode p &&
                            p.parameterGUID == parameterNode.parameterGUID &&
                            p.accessor == ParameterAccessor.Set;

                        if (isMatchingSetter && setter.state == State.White)
                            DFSSort(setter);
                    }
                }
                else
                {
                    foreach (var input in n.inputs)
                    {
                        if (input.state == State.White)
                            DFSSort(input);
                    }
                }

                n.state = State.Black;

                // Only add the node when his children are completely visited
                depthFirstNodes.Add(n.node);
            }

            return depthFirstNodes;
        }
    }
}