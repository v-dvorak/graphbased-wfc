using GBWFC.Graph;
using GBWFC.Modules;
using GBWFC.Solver;

namespace MapShowcase
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // generate map and a graph from it
            List<int[]> map = MapModule.GenerateMap(5, 3);
            List<Edge> edges = MapModule.GetEdges(map);
            WFCGraph g = new WFCGraph(edges, GraphDirectedness.Directed);
            // load rules
            Rulebook rb = new Rulebook(RuleParser.RulesFromJSON("map_rules.json").ToArray());

            // solve
            WFCSolver solver = new WFCSolver(rb, [10, 5, 5, 5]);
            WFCGraph solved = solver.Solve(g, [(0, 0).ConstraintById()]);

            // create and open image
            GraphModule.CreateImage(solved, "map.png", "F:/graphviz/bin", colors: ["#999999", "#ff0000", "#ffff00", "#0000ff"], engine: GraphModule.GraphVizEngine.dot);
            GraphModule.OpenImage("map.png");
        }
    }
}
