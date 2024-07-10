namespace wfc
{
    public delegate double EvaluateNode(HashSet<int> nodeOptions, int[] globalWeights);
    public class Solver : ISolver<Graph>
    {
        private readonly WeightedRandomSelector wrs;
        private readonly EvaluateNode evaluateNode;
        public Rulebook SolverRulebook { get; private set; }
        private readonly int[] globalWeights;
        public Solver(Rulebook rulebook, int[] globalWeights, EvaluateNode? evaluateNode = null)
        {
            if (evaluateNode is null)
            {
                this.evaluateNode = Entropy.Shannon;
            }
            wrs = new WeightedRandomSelector();
            SolverRulebook = rulebook;
            this.globalWeights = globalWeights;
        }
        public Solver(Rulebook rulebook, EvaluateNode? evaluateNode = null)
        {
            if (evaluateNode is null)
            {
                this.evaluateNode = Entropy.Shannon;
            }
            wrs = new WeightedRandomSelector();
            SolverRulebook = rulebook;
            globalWeights = new int[SolverRulebook.RuleCount].Ones();
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
                Rule ruleForChildren = SolverRulebook.GetRuleForChildren(chosen);
                Rule ruleForParents = SolverRulebook.GetRuleForParents(chosen);
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
                        double prior = evaluateNode(child.Options, globalWeights);
                        pq.TryUpdate(child, prior);
                    }
                }
                for (int i = 0; i < collapsingNode.Parents.Count; i++)
                {
                    Node parent = collapsingNode.Parents[i];
                    parent.Options = originalOptionsForParents[i].Copy();
                    if (!parent.IsSet())
                    {
                        double prior = evaluateNode(parent.Options, globalWeights);
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
                    double prior = evaluateNode(node.Options, globalWeights);
                    pq.Enqueue(node, prior);
                }
            }
            return pq;
        }

        public Graph? Solve(Graph graph)
        {
            graph.InitializeNodeOptions(SolverRulebook.RuleCount);
            return RecursiveSolve2(graph, SetUpPriorityQueue(graph), 0);
        }
        public Graph? Solve(Graph graph, bool initialized = false)
        {
            if (!initialized)
            {
                graph.InitializeNodeOptions(SolverRulebook.RuleCount);
            }
            return RecursiveSolve2(graph, SetUpPriorityQueue(graph), 0);
        }
        public Graph? Solve(Graph graph, IEnumerable<ConstraintByNode> constraints)
        {
            graph.InitializeNodeOptions(SolverRulebook.RuleCount);

            foreach (ConstraintByNode constraint in constraints)
            {
                try
                {
                    if (!TryForceValueToNodeWithUpdate(graph, constraint.Node, constraint.ForcedValue))
                    {
                        // something wrong with neighbor updates
                        Console.WriteLine("Unfeasible constraints");
                        return null;
                    }
                }
                catch (ArgumentException)
                {
                    // user tried to assign value to an already set node
                    Console.WriteLine("Multiple assignments to one node");
                    return null;
                }
            }
            return Solve(graph, initialized: true);
        }
        public Graph? Solve(Graph graph, IEnumerable<ConstraintById> constraints)
        {
            return Solve(graph, constraints.Select(constraint => (graph.AllNodes[constraint.NodeId], constraint.ForcedValue).ConstraintByNode()));
        }
        public bool TryForceValueToNodeWithUpdate(Graph graph, Node node, int chosen)
        {
            // used to force certain value to a node before looking for solution
            // assign value
            graph.AssignValueToNode(node, chosen);
            // update
            Rule ruleForChildren = SolverRulebook.GetRuleForChildren(chosen);
            Rule ruleForParents = SolverRulebook.GetRuleForParents(chosen);
            return node.TryUpdateNodeNeighbors(ruleForChildren, ruleForParents);
        }
    }
}
