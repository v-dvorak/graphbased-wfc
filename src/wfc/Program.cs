namespace wfc
{
    internal class Program
    {
        static void Main(string[] args)
        {
            List<Rule> rules = RuleParser.RulesFromJSON("rules.json");
            foreach (Rule rule in rules)
            {
                Console.WriteLine(rule);
            }

            int[] globalWeights = { 10, 10, 1, 1 };
            Rule[] rls = new Rule[rules.Count];
            for (int i = 0; i < rules.Count; i++)
            {
                rls[i] = rules[i];
            }
            Rulebook rb = new Rulebook(rls);
            foreach (Rule rule in rb.CreateInverseRules(rls))
            {
                Console.WriteLine(rule);
            }
            return;
            for (int i = 0; i < 1000; i++)
            {
                Solver sl = new Solver(globalWeights, rb);
                Graph gr = new(Graph.ParseEdgesFromFile("graph.txt"), globalWeights.Length);

                Graph? solved = sl.RecursiveSolve2(gr, 0);
                if (solved is null)
                {
                    Console.WriteLine(solved is null);
                }

                Console.WriteLine(solved);
            }
            return;

            Graph g1 = new(Graph.ParseEdgesFromFile("graph.txt"), globalWeights.Length);
            Graph g2 = g1.Copy();
            Console.WriteLine(g1);
            Console.WriteLine(g2);

            g1.AssignValueToNode(g1.AllNodes[2], 2);
            g1.AllNodes[1].Options.Remove(1);

            Console.WriteLine(g1);
            Console.WriteLine(g2);
            Graph g3 = g1.Copy();
            Console.WriteLine();
            Console.WriteLine(g3.AllSet);
            g3.AssignValueToNode(g3.AllNodes[1], 2);
            g3.AssignValueToNode(g3.AllNodes[0], 2);
            Console.WriteLine(g3.AllSet);

            Console.WriteLine(g3);


            //return;

            //Rule rule = new Rule(0, [1, 2, 3, 4, 5]);
            //Rulebook rb = new Rulebook([rule]);
            //int[] result = [0, 0, 0, 0, 0];
            //for (int i = 0; i < 1500; i++)
            //{
            //    int index = rb.Choose(0);
            //    result[index - 1] += 1;
            //}
            //for (int i = 0; i < result.Length; i++)
            //{
            //    Console.Write(result[i]);
            //    Console.Write(' ');
            //}
            //Console.WriteLine();

            return;
            {
                PriorityQueue.PrioritySet<char, double> pp = new();

                pp.Enqueue('A', 1);
                pp.Enqueue('B', 2);
                pp.Enqueue('C', 3);
                Console.WriteLine(pp.Peek());
                pp.TryUpdate('C', 0);
                Console.WriteLine(pp.Peek());
            }

            int[] GlobalWeights = { 10, 2, 2, 6, 40, 30 };
            int GO = GlobalWeights.Length;
            Graph g = new(Graph.ParseEdgesFromFile("graph.txt"), GO);

            Console.WriteLine(g.AllNodes[0].Id);
            g.AllNodes[0].UpdateOptions(1);
            Console.WriteLine(g.AllNodes[1].Id);
            g.AllNodes[1].UpdateOptions(2);
            Console.WriteLine(g.AllNodes[2].Id);
            g.AllNodes[2].UpdateOptions(5);


            PriorityQueue.PrioritySet<Node, double> pq = new();
            foreach (Node n in g.AllNodes)
            {
                List<double> rel = Entropy.ConvertWeights(GlobalWeights, n.Options);
                double entropy = Entropy.Shannon(rel);
                pq.Enqueue(n, entropy);
            }

            while (pq.Count != 0)
            {
                pq.TryDequeue(out Node n, out double prior);
                Console.Write($"{n.Id} : {prior} ");
                foreach (int k in n.Options)
                {
                    Console.Write($"{k} ");
                }
                Console.WriteLine();
            }
        }
    }
}
