using QuikGraph;

namespace wfc
{
    public class GraphConverter
    {
        public static (Graph, Dictionary<TVertex, Node>) ProcessGraph<TVertex, TEdge>(IEdgeListGraph<TVertex, TEdge> graph)
            where TEdge : IEdge<TVertex>
            where TVertex : notnull
        {
            var nodeMapping = new Dictionary<TVertex, Node>();
            List<Node> allNodes = new();
            // Create Node instances for each vertex in the graph
            int id = 0;
            foreach (var vertex in graph.Vertices)
            {
                Node node = new Node(id);
                nodeMapping[vertex] = node;
                allNodes.Add(node);

                id++;
            }

            // Establish parent-child relationships based on the edges in the graph

            foreach (var edge in graph.Edges)
            {
                Node parentNode = nodeMapping[edge.Source];
                Node childNode = nodeMapping[edge.Target];

                parentNode.Children.Add(childNode);
                childNode.Parents.Add(parentNode);
            }

            return (new Graph(allNodes.ToArray()), nodeMapping);
        }

        public static void AMogus()
        {
            var directedGraph = new AdjacencyGraph<int, Edge<int>>();
            directedGraph.AddVertexRange(new[] { 1, 2, 3, 4 });
            directedGraph.AddEdge(new Edge<int>(1, 2));
            directedGraph.AddEdge(new Edge<int>(1, 3));
            directedGraph.AddEdge(new Edge<int>(2, 4));
            directedGraph.AddEdge(new Edge<int>(3, 4));

            Graph g = ProcessGraph(directedGraph).Item1;

            foreach (Node node in ProcessGraph(directedGraph).Item1.AllNodes)
            {
                foreach (Node child in node.Children)
                {
                    Console.WriteLine($"{node.Id} -> {child.Id}");
                }
            }
        }
    }
}
