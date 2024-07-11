namespace wfc
{
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
        /// <summary>
        /// Main solver method, takes in initialized graph and tries to solve it with rules given by the Solver itself.
        /// </summary>
        /// <param name="graph"><see cref="Graph"/> to solve.</param>
        /// <param name="pq"><see cref="PriorityQueue.PrioritySet{TElement, TPriority}"/></param>
        /// <param name="depth">Debug param, recursion depth.</param>
        /// <returns>Graph with all values set, null if the graph can't be solved according to rules.</returns>
        private Graph? RecursiveSolve2(Graph graph, PriorityQueue.PrioritySet<Node, double> pq, int depth)
        {
            // get node to collapse
            Node collapsingNode;
            double collapsingNodePriority;
            pq.TryDequeue(out collapsingNode, out collapsingNodePriority);

            // convert node options and weights to lists (enables simple removal of elements)
            List<int> options = [.. collapsingNode.Options];
            List<int> weights = new();
            foreach (int k in options)
            {
                weights.Add(globalWeights[k]);
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
            while (options.Count > 0)
            {
                // choose color and remove from options and weights
                int chosenIndex = wrs.Choose(weights);
                int chosen = options[chosenIndex];
                options.RemoveAt(chosenIndex);
                weights.RemoveAt(chosenIndex);

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
                    if (!child.IsSet)
                    {
                        double prior = evaluateNode(child.Options, globalWeights);
                        pq.TryUpdate(child, prior);
                    }
                }
                for (int i = 0; i < collapsingNode.Parents.Count; i++)
                {
                    Node parent = collapsingNode.Parents[i];
                    parent.Options = originalOptionsForParents[i].Copy();
                    if (!parent.IsSet)
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
        /// <summary>
        /// Solves the provided graph using a recursive approach and a priority queue.
        /// </summary>
        /// <param name="graph">The graph to be solved.</param>
        /// <returns>
        /// The solved graph if a solution is found; otherwise, <c>null</c>.
        /// </returns>
        /// <remarks>
        /// This method initializes the node options of the graph based on the rule count from the solver rulebook,
        /// sets up a priority queue, and then attempts to solve the graph recursively.
        /// </remarks>
        public Graph? Solve(Graph graph)
        {
            graph.InitializeNodeOptions(SolverRulebook.RuleCount);
            return RecursiveSolve2(graph, SetUpPriorityQueue(graph), 0);
        }
        /// <summary>
        /// Solves the provided graph using a recursive approach and a priority queue.
        /// </summary>
        /// <param name="graph">The graph to be solved.</param>
        /// <param name="initialized">If true, skips the graph initialization.</param>
        /// <returns>
        /// The solved graph if a solution is found; otherwise, <c>null</c>.
        /// </returns>
        /// <remarks>
        /// This method initializes the node options of the graph based on the rule count from the solver rulebook,
        /// sets up a priority queue, and then attempts to solve the graph recursively.
        /// </remarks>
        public Graph? Solve(Graph graph, bool initialized = false)
        {
            if (!initialized)
            {
                graph.InitializeNodeOptions(SolverRulebook.RuleCount);
            }
            return RecursiveSolve2(graph, SetUpPriorityQueue(graph), 0);
        }
        /// <summary>
        /// Solves the provided graph using a recursive approach and a priority queue considering given constraints.
        /// </summary>
        /// <param name="graph">The graph to be solved.</param>
        /// <param name="constraints">Constraints to consider.</param>
        /// <returns>
        /// The solved graph if a solution is found; otherwise, <c>null</c>.
        /// </returns>
        /// <remarks>
        /// This method initializes the node options of the graph based on the rule count from the solver rulebook,
        /// applies constraints,
        /// sets up a priority queue, and then attempts to solve the graph recursively.
        /// </remarks>
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
                        return null;
                    }
                }
                catch (ArgumentException)
                {
                    // user tried to assign value to an already set node
                    return null;
                }
            }
            return Solve(graph, initialized: true);
        }
        /// <summary>
        /// Solves the provided graph using a recursive approach and a priority queue considering given constraints.
        /// </summary>
        /// <param name="graph">The graph to be solved.</param>
        /// <param name="constraints">Constraints to consider.</param>
        /// <returns>
        /// The solved graph if a solution is found; otherwise, <c>null</c>.
        /// </returns>
        /// <remarks>
        /// This method initializes the node options of the graph based on the rule count from the solver rulebook,
        /// applies constraints,
        /// sets up a priority queue, and then attempts to solve the graph recursively.
        /// </remarks>
        public Graph? Solve(Graph graph, IEnumerable<ConstraintById> constraints)
        {
            return Solve(graph, constraints.Select(constraint => (graph.AllNodes[constraint.NodeId], constraint.ForcedValue).ConstraintByNode()));
        }
        private bool TryForceValueToNodeWithUpdate(Graph graph, Node node, int chosen)
        {
            // used to force certain value to a node before looking for solution
            // assign value
            graph.AssignValueToNode(node, chosen);
            // update
            Rule ruleForChildren = SolverRulebook.GetRuleForChildren(chosen);
            Rule ruleForParents = SolverRulebook.GetRuleForParents(chosen);
            return node.TryUpdateNodeNeighbors(ruleForChildren, ruleForParents);
        }
        private PriorityQueue.PrioritySet<Node, double> SetUpPriorityQueue(Graph graph)
        {
            PriorityQueue.PrioritySet<Node, double> pq = new();
            foreach (Node node in graph.AllNodes)
            {
                if (!node.IsSet)
                {
                    double prior = evaluateNode(node.Options, globalWeights);
                    pq.Enqueue(node, prior);
                }
            }
            return pq;
        }
    }
}
