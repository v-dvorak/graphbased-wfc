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

        public Graph? RecursiveSolve2(Graph graph, int depth)
        {
            Node collapsingNode = LowestEntropy(graph.AllNodes);

            List<int> opts = [.. collapsingNode.Options];
            List<int> wghts = new();
            foreach (int k in opts)
            {
                wghts.Add(globalWeights[k]);
            }

            //Graph result = null;

            List<HashSet<int>> originalOptionsForChildren = new();
            // store options
            foreach (Node child in collapsingNode.Children)
            {
                originalOptionsForChildren.Add(child.Options.Copy());
            }

            while (opts.Count > 0) // && result is null)
            {
                // choose color and remove from options and weights
                int chosenIndex = wrs.Choose(wghts);
                int chosen = opts[chosenIndex];
                opts.RemoveAt(chosenIndex);
                wghts.RemoveAt(chosenIndex);

                graph.AssignValueToNode(collapsingNode, chosen);

                Rule rule = rb.GetRule(chosen);
                foreach (Node child in collapsingNode.Children)
                {
                    if (child.IsSet())
                    {
                        // chosen color does not satisfy current setting
                        if (!rule.Options.Contains(child.AssignedValue))
                        {
                            // continue while loop (with other options)
                            goto end_of_loop;
                        }
                        continue;
                    }

                    child.Options.RemoveWhere(s => !rule.Options.Contains(s));

                    if (child.Options.Count == 0)
                    {
                        // continue while loop (with other options)
                        goto end_of_loop;
                    }
                }

                if (graph.AllSet)
                {
                    return graph;
                }
                // recursion
                Graph? result = RecursiveSolve2(graph, depth + 1);
                if (result is not null)
                {
                    return result;
                }

            end_of_loop: { }
                // reset options
                for (int i = 0; i < collapsingNode.Children.Count; i++)
                {
                    collapsingNode.Children[i].Options = originalOptionsForChildren[i].Copy();
                }
                // resest assignment
                graph.ResetValueAssignment(collapsingNode);
            }
            return null;
        }

        public Graph? RecursiveSolve(Graph graph, PriorityQueue.PrioritySet<Node, double> pq, int depth)
        {
            Console.WriteLine(pq.Count);
            //Node first = pq.Dequeue();

            Node first = LowestEntropy(graph.AllNodes);
            List<int> opts = new();
            List<int> wghs = new();

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine($"Processing node {first.Id}");
            Console.WriteLine($"Depth {depth}");
            Console.WriteLine(new String('-', 20));
            Console.WriteLine(graph);
            Console.WriteLine(new String('-', 20));
            Console.WriteLine();

            foreach (int k in first.Options)
            {
                opts.Add(k);
            }

            Graph? res = null;
            //ChangeLogger logger = new();

            while (opts.Count > 0 && res is null) // && pq.Count > 0)
            {
                wghs = new();

                // get all weights
                foreach (int k in first.Options)
                {
                    wghs.Add(globalWeights[k]);
                }
                // choose item
                int chosenIndex = wrs.Choose(wghs);
                int chosen = opts[chosenIndex];
                opts.RemoveAt(chosenIndex);

                // set item
                graph.AssignValueToNode(first, chosen);
                //logger.LogValueAssignment(first);

                Console.WriteLine($"Assigning {chosen}");

                // update children
                Rule rule = rb.GetRule(chosen);
                foreach (Node child in first.Children)
                {
                    Console.WriteLine($"-- Updating child {child.Id}");
                    // set child
                    if (child.IsSet() && !rule.Options.Contains(child.AssignedValue))
                    {
                        Console.WriteLine($"Child {child.Id} is set and assignment is not valid.");
                        //graph.ResetFromLogger(logger);
                        return null;
                    }
                    // still not set child, update options and update entropy
                    if (!child.IsSet())
                    {
                        bool changed = false;
                        foreach (int k in child.Options)
                        {
                            if (!rule.Options.Contains(k))
                            {
                                child.Options.Remove(k);
                                //logger.LogOptionRemoval(child, k);
                                changed = true;
                            }
                        }
                        // update childs entropy
                        if (changed)
                        {
                            Console.WriteLine($"Updated child options: {String.Join(" ", child.Options)}");
                            double prior = Entropy.Shannon(globalWeights, child.Options);
                            pq.TryUpdate(child, prior);
                        }
                        // no options for non set child
                        if (child.Options.Count == 0)
                        {
                            Console.WriteLine($"Node {child.Id} is not set and count is 0, returning");
                            //graph.ResetFromLogger(logger);
                            return null;
                        }
                    }
                }
                // all nodes are set, solved
                if (graph.AllSet)
                {
                    return graph;
                }
                // not finished, search deeper
                else
                {
                    res = RecursiveSolve(graph.Copy(), pq, depth + 1);
                }
            }
            return res;
        }
        public Graph? Solve(Graph graph)
        {
            PriorityQueue.PrioritySet<Node, double> pq = new();
            foreach (Node n in graph.AllNodes)
            {
                double prior = Entropy.Shannon(globalWeights, n.Options);
                pq.Enqueue(n, prior);
            }

            return RecursiveSolve(graph, pq, 0);
        }
    }
}
