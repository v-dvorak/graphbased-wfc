using wfc;
namespace WFCTests
{
    [TestClass]

    public class GraphColoring
    {
        public bool GraphEquality(Graph graph, int[] coloring)
        {
            for (int i = 0; i < graph.AllNodes.Length; i++)
            {
                if (graph.AllNodes[i].AssignedValue != coloring[i])
                {
                    return false;
                }
            }
            return true;
        }
        [TestMethod]
        public void BaseExample()
        {
            // Arrange
            Rule[] rules = [
                new Rule(0, [1]),
                new Rule(1, [0]),
                ];
            List<(int, int)> edges = [
                (0,1),
                (1,2),
                (2,3),
                (3,0),
                ];
            int[] globalWeights = [1, 1];
            Graph g = new Graph(edges, rules.Length);
            Rulebook rb = new Rulebook(rules);
            Solver sl = new Solver(globalWeights, rb);

            // Act
            Graph result = sl.Solve(g);

            // Assert
            Assert.IsTrue(result is not null &&
                (GraphEquality(result, [0, 1, 0, 1]) || GraphEquality(result, [1, 0, 1, 0]))
                );
        }
        [TestMethod]
        public void ForceValue_Possible()
        {
            // Arrange
            Rule[] rules = [
                new Rule(0, [1]),
                new Rule(1, [0]),
                ];
            List<(int, int)> edges = [
                (0,1),
                (1,2),
                (2,3),
                (3,0),
                ];
            int[] globalWeights = [1, 1];
            Graph g = new Graph(edges, rules.Length);
            Rulebook rb = new Rulebook(rules);
            Solver sl = new Solver(globalWeights, rb);

            // Act
            Graph result = sl.Solve(g, [(g.AllNodes[0], 0)]);

            // Assert
            Assert.IsTrue(result is not null &&
                GraphEquality(result, [0, 1, 0, 1])
                );
        }
        [TestMethod]
        public void ForceValue_Impossible_CorrectAssignment()
        {
            // Arrange
            Rule[] rules = [
                new Rule(0, [1]),
                new Rule(1, [0]),
                ];
            List<(int, int)> edges = [
                (0,1),
                (1,2),
                (2,3),
                (3,0),
                ];
            int[] globalWeights = [1, 1];
            Graph g = new Graph(edges, rules.Length);
            Rulebook rb = new Rulebook(rules);
            Solver sl = new Solver(globalWeights, rb);

            // Act
            Graph result = sl.Solve(g, [(g.AllNodes[0], 0), (g.AllNodes[1], 0)]);

            // Assert
            Assert.IsTrue(result is null);
        }
        [TestMethod]
        public void ForceValue_Impossible_DoubletAssignment()
        {
            // Arrange
            Rule[] rules = [
                new Rule(0, [1]),
                new Rule(1, [0]),
                ];
            List<(int, int)> edges = [
                (0,1),
                (1,2),
                (2,3),
                (3,0),
                ];
            int[] globalWeights = [1, 1];
            Graph g = new Graph(edges, rules.Length);
            Rulebook rb = new Rulebook(rules);
            Solver sl = new Solver(globalWeights, rb);

            // Act
            Graph result = sl.Solve(g, [(g.AllNodes[0], 0), (g.AllNodes[0], 0)]);

            // Assert
            Assert.IsTrue(result is null);
        }
    }
}