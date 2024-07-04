﻿namespace wfc
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

        private Graph? RecursiveSolve2(Graph graph, int depth)
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
            List<HashSet<int>> originalOptionsForParents = new();
            // store options
            foreach (Node child in collapsingNode.Children)
            {
                originalOptionsForChildren.Add(child.Options.Copy());
            }
            foreach (Node parent in collapsingNode.Parents)
            {
                originalOptionsForParents.Add(parent.Options.Copy());
            }

            while (opts.Count > 0) // && result is null)
            {
                // choose color and remove from options and weights
                int chosenIndex = wrs.Choose(wghts);
                int chosen = opts[chosenIndex];
                opts.RemoveAt(chosenIndex);
                wghts.RemoveAt(chosenIndex);

                graph.AssignValueToNode(collapsingNode, chosen);

                // update children
                Rule ruleForChildren = rb.GetRuleForChildren(chosen);
                foreach (Node child in collapsingNode.Children)
                {
                    if (child.IsSet())
                    {
                        // chosen color does not satisfy current setting
                        if (!ruleForChildren.Options.Contains(child.AssignedValue))
                        {
                            // continue while loop (with other options)
                            goto end_of_loop;
                        }
                        continue;
                    }

                    child.Options.RemoveWhere(s => !ruleForChildren.Options.Contains(s));

                    if (child.Options.Count == 0)
                    {
                        // continue while loop (with other options)
                        goto end_of_loop;
                    }
                }

                // update parents
                Rule ruleForParents = rb.GetRuleForParents(chosen);
                foreach (Node parent in collapsingNode.Parents)
                {
                    if (parent.IsSet())
                    {
                        // chosen color does not satisfy current setting
                        if (!ruleForParents.Options.Contains(parent.AssignedValue))
                        {
                            // continue while loop (with other options)
                            goto end_of_loop;
                        }
                        continue;
                    }

                    parent.Options.RemoveWhere(s => !ruleForParents.Options.Contains(s));

                    if (parent.Options.Count == 0)
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
                for (int i = 0; i < collapsingNode.Parents.Count; i++)
                {
                    collapsingNode.Parents[i].Options = originalOptionsForChildren[i].Copy();
                }
                // resest assignment
                graph.ResetValueAssignment(collapsingNode);
            }
            return null;
        }

        public Graph? Solve(Graph graph)
        {
            // preprocessing
            //PriorityQueue.PrioritySet<Node, double> pq = new();
            //foreach (Node n in graph.AllNodes)
            //{
            //    double prior = Entropy.Shannon(globalWeights, n.Options);
            //    pq.Enqueue(n, prior);
            //}

            return RecursiveSolve2(graph, 0);
        }
    }
}
