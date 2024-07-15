using GBWFC.Graph;
using QuikGraph;

namespace WFCTests
{
    [TestClass]
    public class ConverterTests
    {
        [TestMethod]
        public void BaseProblem()
        {
            var directedGraph = new AdjacencyGraph<int, Edge<int>>();
            directedGraph.AddVertexRange(new[] { 1, 2, 3, 4 });
            directedGraph.AddEdge(new Edge<int>(1, 2));
            directedGraph.AddEdge(new Edge<int>(1, 3));
            directedGraph.AddEdge(new Edge<int>(2, 4));
            directedGraph.AddEdge(new Edge<int>(3, 4));

            WFCGraph g = GraphConverter.ProcessGraph(directedGraph).Item1;


        }
    }
}
