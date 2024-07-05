using wfc;
namespace WFCTests
{
    [TestClass]

    public class GraphColoringTests
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
    [TestClass]
    public class SudokuTests
    {
        [TestMethod]
        public void Sudoku_Hallucination()
        {
            // Arrange
            Sudoku example = new Sudoku(new int[,]
            {
                { 0, 0, 0, 2, 0, 0, 0, 9, 0 },
                { 0, 0, 0, 0, 0, 9, 7, 0, 4 },
                { 9, 0, 0, 0, 7, 0, 2, 1, 8 },
                { 7, 8, 0, 3, 0, 0, 5, 0, 0 },
                { 0, 0, 9, 4, 6, 8, 3, 0, 7 },
                { 0, 2, 0, 0, 0, 7, 1, 0, 0 },
                { 0, 0, 0, 8, 2, 6, 9, 5, 0 },
                { 6, 3, 0, 1, 9, 0, 4, 0, 2 },
                { 0, 0, 5, 0, 0, 0, 8, 6, 0 }
            });
            SudokuSolver solver = new SudokuSolver();

            // Act
            Sudoku result = solver.Solve(example);

            // Arrange
            Assert.IsTrue(SudokuChecker.DoesNotHallucinate(example, result));
        }
        public void Sudoku_CorrectSolution()
        {
            // Arrange
            Sudoku example = new Sudoku(new int[,]
            {
                { 0, 0, 0, 2, 0, 0, 0, 9, 0 },
                { 0, 0, 0, 0, 0, 9, 7, 0, 4 },
                { 9, 0, 0, 0, 7, 0, 2, 1, 8 },
                { 7, 8, 0, 3, 0, 0, 5, 0, 0 },
                { 0, 0, 9, 4, 6, 8, 3, 0, 7 },
                { 0, 2, 0, 0, 0, 7, 1, 0, 0 },
                { 0, 0, 0, 8, 2, 6, 9, 5, 0 },
                { 6, 3, 0, 1, 9, 0, 4, 0, 2 },
                { 0, 0, 5, 0, 0, 0, 8, 6, 0 }
            });
            Sudoku expectedResult = new Sudoku(new int[,]
            {
                { 8, 4, 7, 2, 3, 1, 6, 9, 5 },
                { 5, 1, 2, 6, 8, 9, 7, 3, 4 },
                { 9, 6, 3, 5, 7, 4, 2, 1, 8 },
                { 7, 8, 6, 3, 1, 2, 5, 4, 9 },
                { 1, 5, 9, 4, 6, 8, 3, 2, 7 },
                { 3, 2, 4, 9, 5, 7, 1, 8, 6 },
                { 4, 7, 1, 8, 2, 6, 9, 5, 3 },
                { 6, 3, 8, 1, 9, 5, 4, 7, 2 },
                { 2, 9, 5, 7, 4, 3, 8, 6, 1 }

            });
            SudokuSolver solver = new SudokuSolver();

            // Act
            Sudoku result = solver.Solve(example);

            // Arrange
            Assert.Equals(result, expectedResult);
        }
    }
}
