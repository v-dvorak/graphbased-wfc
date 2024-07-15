using GBWFC.Graph;
using GBWFC.Modules;
using GBWFC.Solver;

namespace GraphColoringShowcase
{
    internal class Program
    {
        static void Main(string[] args)
        {
            WFCGraph g = new WFCGraph(WFCGraph.ParseEdgesFromFile("graph.txt"), GraphDirectedness.Undirected);
            Rule[] rules = Rulebook.CreateColoringRules(4);

            WFCSolver solver = new WFCSolver(rules);
            WFCGraph solved = solver.Solve(g);

            if (solved is not null)
            {
                string outputName = "temp.png";
                string graphVizPath = "F:/graphviz/bin";
                Console.WriteLine("Success!");
                GraphModule.CreateImage(solved, outputName, graphVizPath, GraphModule.GraphVizEngine.fdp);
                GraphModule.OpenImage(outputName);
            }
        }
    }
}
