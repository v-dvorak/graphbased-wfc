using GBWFC.Modules;
using GBWFC.Graph;
using GBWFC.Solver;

namespace MapShowcase
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // generate map and a graph from it
            List<int[]> map = MapModule.GenerateMap(8, 4);
            List<Edge> edges = MapModule.GetEdges(map);
            WFCGraph g = new WFCGraph(edges, GraphDirectedness.Directed);
            // load rules
            Rulebook rb = new Rulebook(RuleParser.RulesFromJSON("map_rules.json").ToArray());

            // solve
            WFCSolver solver = new WFCSolver(rb, [75, 15, 5, 5]);
            WFCGraph solved = solver.Solve(g, [(0, 0).ConstraintById()]);

            // create and open image
            GraphModule.CreateImage(solved, "map.png", "F:/graphviz/bin", GraphModule.GraphVizEngine.dot);
            GraphModule.OpenImage("map.png");
        }
    }
}
