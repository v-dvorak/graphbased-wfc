using QuikGraph;

namespace GBWFC.Graph
{
    public class GraphConverter
    {
        public static (WFCGraph, Dictionary<TVertex, Node>) ProcessGraph<TVertex, TEdge>(IEdgeListGraph<TVertex, TEdge> graph)
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
            return (new WFCGraph(allNodes.ToArray()), nodeMapping);
        }
    }
}
