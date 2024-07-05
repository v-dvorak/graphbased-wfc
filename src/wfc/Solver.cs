namespace wfc
{
    public class Solver
    {
        private readonly WeightedRandomSelector wrs;
        private readonly Rulebook rb;
        private readonly int[] globalWeights;
        public Solver(int[] globalWeights, Rulebook rulebook)
        {
            wrs = new WeightedRandomSelector();
            this.globalWeights = globalWeights;
            rb = rulebook;
        }

        /*
        
        x = pq.dequeue()

        while x.options != [] and result neuspesny:
            color = weighted_random(x.options)
            x.set_color(color)

            rule = rulebook(color)

            foreach child in x.children:
                child.update_options(rule, color)
            
                if updated:
                    entropy = shannon(child)
                    pq.update(child, entropy)
            
                // edge cases returny
            
            result = fce(graf, pq)
         
        */
        public Node LowestEntropy(Node[] nodes)
        {
            if (nodes.Length == 0)
            {
                throw new ArgumentOutOfRangeException("nodes", "Cannot find maximum in an empty list");
            }
            int i = 0;
            while (i < nodes.Length)
            {
                if (!nodes[i].IsSet())
                {
                    break;
                }
                i++;
            }
            if (i == nodes.Length)
            {
                throw new ArgumentOutOfRangeException("nodes", "Cannot find any non-set node");
            }
            Node best = nodes[i];
            double bestEntropy = Entropy.Shannon(globalWeights, best.Options);
            while (i < nodes.Length)
            {
                Node currentNode = nodes[i];
                if (!currentNode.IsSet())
                {
                    double entropy = Entropy.Shannon(globalWeights, nodes[i].Options);
                    if (entropy < bestEntropy)
                    {
                        best = nodes[i];
                        bestEntropy = entropy;
                    }
                }
                i++;
            }
            return best;
        }
        private Graph? RecursiveSolve2(Graph graph, PriorityQueue.PrioritySet<Node, double> pq, int depth)
        {
            //Node collapsingNode = LowestEntropy(graph.AllNodes);
            Node collapsingNode;
            double collapsingNodePriority;
            pq.TryDequeue(out collapsingNode, out collapsingNodePriority);

            List<int> opts = [.. collapsingNode.Options];
            List<int> wghts = new();
            foreach (int k in opts)
            {
                wghts.Add(globalWeights[k]);
            }

            // store options for possible reset
            List<HashSet<int>> originalOptionsForChildren = new();
            List<HashSet<int>> originalOptionsForParents = new();
            foreach (Node child in collapsingNode.Children)
            {
                originalOptionsForChildren.Add(child.Options.Copy());
            }
            foreach (Node parent in collapsingNode.Parents)
            {
                originalOptionsForParents.Add(parent.Options.Copy());
            }

            // LOOK FOR SOLUTION
            while (opts.Count > 0)
            {
                // choose color and remove from options and weights
                int chosenIndex = wrs.Choose(wghts);
                int chosen = opts[chosenIndex];
                opts.RemoveAt(chosenIndex);
                wghts.RemoveAt(chosenIndex);

                graph.AssignValueToNode(collapsingNode, chosen);

                // update children and parents
                Rule ruleForChildren = rb.GetRuleForChildren(chosen);
                Rule ruleForParents = rb.GetRuleForParents(chosen);
                bool updateSuccess = collapsingNode.TryUpdateNodeNeighbors(ruleForChildren, ruleForParents);

                if (updateSuccess)
                {
                    // all values set are correct, return graph if finished, or continue search
                    if (graph.AllSet)
                    {
                        return graph;
                    }
                    // recursion
                    Graph? result = RecursiveSolve2(graph, pq, depth + 1);
                    if (result is not null)
                    {
                        return result;
                    }
                }

                // RESET
                // reset options
                for (int i = 0; i < collapsingNode.Children.Count; i++)
                {
                    Node child = collapsingNode.Children[i];
                    child.Options = originalOptionsForChildren[i].Copy();
                    if (!child.IsSet())
                    {
                        double prior = Entropy.Shannon(globalWeights, child.Options);
                        pq.TryUpdate(child, prior);
                    }
                }
                for (int i = 0; i < collapsingNode.Parents.Count; i++)
                {
                    Node parent = collapsingNode.Parents[i];
                    parent.Options = originalOptionsForParents[i].Copy();
                    if (!parent.IsSet())
                    {
                        double prior = Entropy.Shannon(globalWeights, parent.Options);
                        pq.TryUpdate(parent, prior);
                    }
                }
                // reset assignment
                graph.ResetValueAssignment(collapsingNode);
            }
            pq.Enqueue(collapsingNode, collapsingNodePriority);
            return null;
        }
        private PriorityQueue.PrioritySet<Node, double> SetUpPriorityQueue(Graph graph)
        {
            PriorityQueue.PrioritySet<Node, double> pq = new();
            foreach (Node node in graph.AllNodes)
            {
                if (!node.IsSet())
                {
                    double prior = Entropy.Shannon(globalWeights, node.Options);
                    pq.Enqueue(node, prior);
                }
            }
            return pq;
        }

        public Graph? Solve(Graph graph)
        {
            return RecursiveSolve2(graph, SetUpPriorityQueue(graph), 0);
        }
        public Graph? Solve(Graph graph, IEnumerable<(Node, int)> constraints)
        {
            foreach ((Node node, int value) in constraints)
            {
                try
                {
                    if (!TryForceValueToNodeWithUpdate(graph, node, value))
                    {
                        // something wrong with neighbor updates
                        return null;
                    }
                }
                catch (ArgumentException)
                {
                    // user tried to assign value to an already set node
                    return null;
                }
            }
            return Solve(graph);
        }
        public Graph? Solve(Graph graph, IEnumerable<(int, int)> constraints)
        {
            // constraints format : (node id, value)
            foreach ((int nodeId, int value) in constraints)
            {
                try
                {
                    if (!TryForceValueToNodeWithUpdate(graph, graph.AllNodes[nodeId], value))
                    {
                        // something wrong with neighbor updates
                        return null;
                    }
                }
                catch (ArgumentException)
                {
                    // user tried to assign value to an already set node
                    return null;
                }
            }
            return Solve(graph);
        }
        public bool TryForceValueToNodeWithUpdate(Graph graph, Node node, int chosen)
        {
            // used to force certain value to a node before looking for solution
            // assign value
            graph.AssignValueToNode(node, chosen);
            // update
            Rule ruleForChildren = rb.GetRuleForChildren(chosen);
            Rule ruleForParents = rb.GetRuleForParents(chosen);
            return node.TryUpdateNodeNeighbors(ruleForChildren, ruleForParents);
        }
    }
}
