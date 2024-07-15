using GBWFC.Graph;
using QuikGraph;

namespace WFCTests
{
    public static class TestExtensions
    {
        /// <summary>
        /// Determines if two lists contain the same items, same count. Does not ignore duplicates but ignores the order.
        /// via: https://stackoverflow.com/a/24361129
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static bool SequenceEqualsIgnoreOrder<T>(this IEnumerable<T> list1, IEnumerable<T> list2, IEqualityComparer<T> comparer = null)
        {
            if (list1 is ICollection<T> ilist1 && list2 is ICollection<T> ilist2 && ilist1.Count != ilist2.Count)
                return false;

            if (comparer == null)
                comparer = EqualityComparer<T>.Default;

            var itemCounts = new Dictionary<T, int>(comparer);
            foreach (T s in list1)
            {
                if (itemCounts.ContainsKey(s))
                {
                    itemCounts[s]++;
                }
                else
                {
                    itemCounts.Add(s, 1);
                }
            }
            foreach (T s in list2)
            {
                if (itemCounts.ContainsKey(s))
                {
                    itemCounts[s]--;
                }
                else
                {
                    return false;
                }
            }
            return itemCounts.Values.All(c => c == 0);
        }
    }
    [TestClass]
    public class ConverterTests
    {
        [TestMethod]
        public void BaseProblem_Directed()
        {
            // Arrange
            var directedGraph = new AdjacencyGraph<int, Edge<int>>();
            directedGraph.AddVertexRange([1, 2, 3, 4]);
            directedGraph.AddEdge(new Edge<int>(1, 2));
            directedGraph.AddEdge(new Edge<int>(1, 3));
            directedGraph.AddEdge(new Edge<int>(2, 4));
            directedGraph.AddEdge(new Edge<int>(3, 4));

            // Act
            WFCGraph g = GraphConverter.ProcessGraph(directedGraph).Item1;

            // Assert
            Assert.IsTrue(g.GetEdges().SequenceEqualsIgnoreOrder(
                [(0, 1).Edge(), (0, 2).Edge(), (1, 3).Edge(), (2, 3).Edge()]
                ));
        }
        [TestMethod]
        public void BaseProblem_Undirected()
        {
            // Arrange
            var undirectedGraph = new UndirectedGraph<int, UndirectedEdge<int>>();
            undirectedGraph.AddVertexRange([0, 1, 2, 3]);
            undirectedGraph.AddEdge(new UndirectedEdge<int>(0, 1));
            undirectedGraph.AddEdge(new UndirectedEdge<int>(0, 2));
            undirectedGraph.AddEdge(new UndirectedEdge<int>(0, 3));
            undirectedGraph.AddEdge(new UndirectedEdge<int>(1, 2));
            undirectedGraph.AddEdge(new UndirectedEdge<int>(2, 3));

            // Act
            WFCGraph g = GraphConverter.ProcessGraph(undirectedGraph).Item1;

            // Assert
            Assert.IsTrue(g.GetEdges().SequenceEqualsIgnoreOrder(
                [
                    (0, 1).Edge(), (1, 0).Edge(),
                    (0, 2).Edge(), (2, 0).Edge(),
                    (0, 3).Edge(), (3, 0).Edge(),
                    (1, 2).Edge(), (2, 1).Edge(),
                    (2, 3).Edge(), (3, 2).Edge(),
                ]
            ));
        }
        [TestMethod]
        public void BaseProblem_Undirected2()
        {
            // Arrange
            var undirectedGraph = new UndirectedGraph<int, UndirectedEdge<int>>();
            undirectedGraph.AddVertexRange([1, 2, 3, 4]);
            undirectedGraph.AddEdge(new UndirectedEdge<int>(1, 2));
            undirectedGraph.AddEdge(new UndirectedEdge<int>(1, 3));
            undirectedGraph.AddEdge(new UndirectedEdge<int>(1, 4));
            undirectedGraph.AddEdge(new UndirectedEdge<int>(2, 3));
            undirectedGraph.AddEdge(new UndirectedEdge<int>(3, 4));

            // Act
            WFCGraph g = GraphConverter.ProcessGraph(undirectedGraph).Item1;

            // Assert
            Assert.IsTrue(g.GetEdges().SequenceEqualsIgnoreOrder(
                [
                    (0, 1).Edge(), (1, 0).Edge(),
                    (0, 2).Edge(), (2, 0).Edge(),
                    (0, 3).Edge(), (3, 0).Edge(),
                    (1, 2).Edge(), (2, 1).Edge(),
                    (2, 3).Edge(), (3, 2).Edge(),
                ]
            ));
        }
    }
}
