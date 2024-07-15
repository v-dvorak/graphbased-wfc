using GBWFC.Graph;
using GBWFC.Solver;

namespace WFCTests
{
    [TestClass]
    public class GraphColoringTests
    {
        [TestMethod]
        public void BaseProblem_Undirected()
        {
            // Arrange
            Rule[] rules = [
                new Rule(0, [1]),
                new Rule(1, [0]),
                ];
            List<Edge> edges = [
                (0,1).Edge(),
                (1,2).Edge(),
                (2,3).Edge(),
                (3,0).Edge(),
                ];
            int[] globalWeights = [1, 1];
            WFCGraph g = new WFCGraph(edges, GraphDirectedness.Undirected);
            Rulebook rb = new Rulebook(rules);
            WFCSolver sl = new WFCSolver(rb, globalWeights);

            // Act
            WFCGraph result = sl.Solve(g);

            // Assert
            Assert.IsTrue(
                result is not null &&
                (result.ToList().SequenceEqual([0, 1, 0, 1]) || result.ToList().SequenceEqual([1, 0, 1, 0]))
                );
        }
        [TestMethod]
        public void BaseProblem_Directed()
        {
            // Arrange
            Rule[] rules = [
                new Rule(0, [1]),
                new Rule(1, [0]),
                ];
            List<Edge> edges = [
                (0,1).Edge(),
                (1,2).Edge(),
                (2,3).Edge(),
                (3,0).Edge(),
                ];
            int[] globalWeights = [1, 1];
            WFCGraph g = new WFCGraph(edges, GraphDirectedness.Directed);
            Rulebook rb = new Rulebook(rules);
            WFCSolver sl = new WFCSolver(rb, globalWeights);

            // Act
            WFCGraph result = sl.Solve(g);

            // Assert
            Assert.IsTrue(
                result is not null &&
                (result.ToList().SequenceEqual([0, 1, 0, 1]) || result.ToList().SequenceEqual([1, 0, 1, 0]))
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
            List<Edge> edges = [
                (0,1).Edge(),
                (1,2).Edge(),
                (2,3).Edge(),
                (3,0).Edge(),
                ];
            int[] globalWeights = [1, 1];
            WFCGraph g = new WFCGraph(edges);
            Rulebook rb = new Rulebook(rules);
            WFCSolver sl = new WFCSolver(rb, globalWeights);

            // Act
            WFCGraph result = sl.Solve(g, [(0, 0).ConstraintById()]);

            // Assert
            Assert.IsTrue(
                result is not null &&
                result.ToList().SequenceEqual([0, 1, 0, 1])
                );
        }
        [TestMethod]
        public void ForceValue_Possible_CorrectAssignment()
        {
            // Arrange
            Rule[] rules = [
                new Rule(0, [1]),
                new Rule(1, [0]),
                ];
            List<Edge> edges = [
                (0,1).Edge(),
                (1,2).Edge(),
                (2,3).Edge(),
                (3,0).Edge(),
                ];
            int[] globalWeights = [1, 1];
            WFCGraph g = new WFCGraph(edges);
            Rulebook rb = new Rulebook(rules);
            WFCSolver sl = new WFCSolver(rb, globalWeights);

            // Act
            WFCGraph result = sl.Solve(g, [(0, 0).ConstraintById(), (1, 0).ConstraintById()]);

            // Assert
            Assert.IsTrue(result is null);
        }
        [TestMethod]
        public void ForceValue_Impossible_DoubleAssignment_SameValue()
        {
            // Arrange
            Rule[] rules = [
                new Rule(0, [1]),
                new Rule(1, [0]),
                ];
            List<Edge> edges = [
                (0,1).Edge(),
                (1,2).Edge(),
                (2,3).Edge(),
                (3,0).Edge(),
                ];
            int[] globalWeights = [1, 1];
            WFCGraph g = new WFCGraph(edges);
            Rulebook rb = new Rulebook(rules);
            WFCSolver sl = new WFCSolver(rb, globalWeights);

            // Act
            WFCGraph result = sl.Solve(g, [(0, 0).ConstraintById(), (0, 0).ConstraintById()]);

            // Assert
            Assert.IsTrue(result is null);
        }
        [TestMethod]
        public void ForceValue_Impossible_DoubleAssignment_DifferentValues()
        {
            // Arrange
            Rule[] rules = [
                new Rule(0, [1]),
                new Rule(1, [0]),
                ];
            List<Edge> edges = [
                (0,1).Edge(),
                (1,2).Edge(),
                (2,3).Edge(),
                (3,0).Edge(),
                ];
            int[] globalWeights = [1, 1];
            WFCGraph g = new WFCGraph(edges);
            Rulebook rb = new Rulebook(rules);
            WFCSolver sl = new WFCSolver(rb, globalWeights);

            // Act
            WFCGraph result = sl.Solve(g, [(0, 0).ConstraintById(), (0, 1).ConstraintById()]);

            // Assert
            Assert.IsTrue(result is null);
        }
    }
}
