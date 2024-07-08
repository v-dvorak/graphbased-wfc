using wfc;
using wfc.Examples;
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
        public void BaseProblem()
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
        public void ForceValue_Impossible_DoubleAssignment()
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
            Sudoku problem = new Sudoku(new int[,]
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
            Sudoku result = solver.Solve(problem);

            // Arrange
            Assert.IsTrue(SudokuChecker.DoesNotHallucinate(problem, result));
        }
        [TestMethod]
        public void Sudoku_CorrectSolution_0()
        {
            // Arrange
            Sudoku problem = new Sudoku(new int[,]
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
            Sudoku result = solver.Solve(problem);

            // Arrange
            Assert.IsTrue(SudokuChecker.DoesNotHallucinate(problem, result) && SudokuChecker.IsSudokuValid(result.Board));
        }
        [TestMethod]
        public void Sudoku_CorrectSolution_1()
        {
            // Arrange
            Sudoku problem = new Sudoku(new int[,]
            {
                { 2, 0, 0, 0, 0, 0, 0, 4, 0, },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, },
                { 0, 0, 0, 4, 0, 5, 0, 9, 7, },
                { 0, 5, 0, 8, 2, 0, 3, 0, 0, },
                { 0, 0, 0, 1, 0, 7, 2, 0, 0, },
                { 7, 0, 0, 6, 0, 0, 9, 0, 5, },
                { 8, 0, 5, 9, 0, 0, 6, 0, 2, },
                { 0, 6, 3, 0, 7, 0, 4, 0, 0, },
            });
            SudokuSolver solver = new SudokuSolver();

            // Act
            Sudoku result = solver.Solve(problem);

            // Arrange
            Assert.IsTrue(SudokuChecker.DoesNotHallucinate(problem, result) && SudokuChecker.IsSudokuValid(result.Board));
        }
        [TestMethod]
        public void Sudoku_CorrectSolution_2()
        {
            // Arrange
            Sudoku problem = new Sudoku(new int[,]
            {
                { 0, 0, 0, 2, 3, 0, 0, 0, 7, },
                { 0, 0, 0, 1, 0, 0, 6, 0, 0, },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, },
                { 1, 0, 3, 0, 4, 0, 0, 0, 0, },
                { 0, 5, 0, 0, 9, 0, 0, 0, 0, },
                { 0, 0, 0, 3, 7, 0, 5, 0, 4, },
                { 0, 0, 0, 6, 8, 4, 0, 7, 0, },
                { 0, 4, 5, 0, 1, 0, 0, 0, 0, },
                { 8, 0, 0, 7, 0, 0, 3, 4, 1, },
            });
            SudokuSolver solver = new SudokuSolver();

            // Act
            Sudoku result = solver.Solve(problem);

            // Arrange
            Assert.IsTrue(SudokuChecker.DoesNotHallucinate(problem, result) && SudokuChecker.IsSudokuValid(result.Board));
        }
        [TestMethod]
        public void Sudoku_CorrectSolution_3()
        {
            // Arrange
            Sudoku problem = new Sudoku(new int[,]
            {
                { 0, 6, 0, 0, 8, 0, 0, 0, 5, },
                { 0, 2, 5, 0, 4, 7, 0, 8, 0, },
                { 7, 0, 0, 0, 5, 0, 0, 0, 0, },
                { 0, 1, 0, 0, 0, 5, 0, 9, 0, },
                { 4, 0, 0, 0, 0, 1, 0, 0, 6, },
                { 0, 0, 8, 0, 0, 0, 4, 5, 0, },
                { 5, 0, 0, 6, 0, 0, 0, 0, 0, },
                { 0, 0, 0, 0, 0, 2, 5, 0, 0, },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, },
            });
            SudokuSolver solver = new SudokuSolver();

            // Act
            Sudoku result = solver.Solve(problem);

            // Arrange
            Assert.IsTrue(SudokuChecker.DoesNotHallucinate(problem, result) && SudokuChecker.IsSudokuValid(result.Board));
        }
        [TestMethod]
        public void Sudoku_CorrectSolution_4()
        {
            // Arrange
            Sudoku problem = new Sudoku(new int[,]
            {
                { 0, 0, 7, 0, 0, 3, 0, 4, 6, },
                { 0, 0, 0, 0, 5, 0, 0, 8, 0, },
                { 0, 0, 0, 7, 0, 9, 0, 0, 0, },
                { 0, 0, 0, 0, 0, 5, 0, 0, 7, },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, },
                { 7, 0, 8, 0, 0, 0, 3, 0, 0, },
                { 5, 0, 1, 0, 0, 0, 0, 7, 0, },
                { 6, 7, 0, 0, 3, 8, 0, 0, 0, },
                { 0, 0, 0, 5, 7, 0, 6, 3, 0, },
            });
            SudokuSolver solver = new SudokuSolver();

            // Act
            Sudoku result = solver.Solve(problem);

            // Arrange
            Assert.IsTrue(SudokuChecker.DoesNotHallucinate(problem, result) && SudokuChecker.IsSudokuValid(result.Board));
        }
        [TestMethod]
        public void Sudoku_CorrectSolution_5()
        {
            // Arrange
            Sudoku problem = new Sudoku(new int[,]
            {
                { 7, 0, 0, 0, 0, 0, 0, 0, 1, },
                { 0, 0, 4, 0, 5, 0, 7, 8, 0, },
                { 0, 0, 9, 0, 4, 0, 2, 0, 0, },
                { 2, 0, 0, 0, 0, 0, 8, 0, 0, },
                { 4, 0, 0, 8, 0, 0, 3, 0, 0, },
                { 0, 0, 0, 0, 2, 0, 1, 0, 0, },
                { 0, 2, 0, 0, 1, 0, 0, 0, 0, },
                { 0, 4, 0, 0, 0, 0, 5, 0, 0, },
                { 0, 0, 1, 0, 3, 8, 0, 2, 0, },
            });
            SudokuSolver solver = new SudokuSolver();

            // Act
            Sudoku result = solver.Solve(problem);

            // Arrange
            Assert.IsTrue(SudokuChecker.DoesNotHallucinate(problem, result) && SudokuChecker.IsSudokuValid(result.Board));
        }
        [TestMethod]
        public void Sudoku_CorrectSolution_6()
        {
            // Arrange
            Sudoku problem = new Sudoku(new int[,]
            {
                { 0, 0, 0, 7, 0, 5, 0, 0, 2, },
                { 0, 0, 3, 0, 0, 8, 0, 7, 0, },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, },
                { 2, 0, 5, 0, 0, 0, 0, 0, 0, },
                { 0, 4, 6, 0, 0, 0, 0, 1, 5, },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, },
                { 4, 0, 0, 5, 0, 0, 0, 8, 0, },
                { 6, 0, 1, 0, 8, 0, 0, 0, 0, },
                { 8, 7, 9, 6, 2, 0, 0, 5, 0, },
            });
            SudokuSolver solver = new SudokuSolver();

            // Act
            Sudoku result = solver.Solve(problem);

            // Arrange
            Assert.IsTrue(SudokuChecker.DoesNotHallucinate(problem, result) && SudokuChecker.IsSudokuValid(result.Board));
        }
        [TestMethod]
        public void Sudoku_CorrectSolution_7()
        {
            // Arrange
            Sudoku problem = new Sudoku(new int[,]
            {
                { 0, 0, 2, 0, 0, 0, 0, 0, 7, },
                { 0, 4, 0, 0, 0, 0, 0, 0, 0, },
                { 0, 0, 0, 1, 0, 0, 0, 0, 5, },
                { 0, 0, 0, 0, 0, 6, 0, 0, 0, },
                { 0, 0, 0, 0, 0, 9, 6, 0, 0, },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, },
                { 0, 2, 0, 8, 0, 3, 9, 7, 6, },
                { 0, 6, 4, 9, 2, 5, 0, 0, 0, },
                { 0, 3, 0, 6, 7, 0, 5, 2, 0, },
            });
            SudokuSolver solver = new SudokuSolver();

            // Act
            Sudoku result = solver.Solve(problem);

            // Arrange
            Assert.IsTrue(SudokuChecker.DoesNotHallucinate(problem, result) && SudokuChecker.IsSudokuValid(result.Board));
        }
        [TestMethod]
        public void Sudoku_CorrectSolution_8()
        {
            // Arrange
            Sudoku problem = new Sudoku(new int[,]
            {
                { 8, 0, 0, 0, 0, 0, 0, 0, 0, },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, },
                { 4, 5, 0, 1, 8, 0, 0, 0, 0, },
                { 2, 0, 4, 0, 3, 0, 0, 9, 0, },
                { 0, 6, 5, 0, 0, 8, 1, 0, 0, },
                { 0, 0, 0, 0, 0, 4, 0, 5, 0, },
                { 0, 0, 8, 6, 0, 0, 9, 7, 2, },
                { 6, 0, 0, 0, 7, 0, 0, 0, 5, },
                { 0, 7, 0, 0, 0, 2, 0, 6, 0, },
            });
            SudokuSolver solver = new SudokuSolver();

            // Act
            Sudoku result = solver.Solve(problem);

            // Arrange
            Assert.IsTrue(SudokuChecker.DoesNotHallucinate(problem, result) && SudokuChecker.IsSudokuValid(result.Board));
        }
        [TestMethod]
        public void Sudoku_CorrectSolution_9()
        {
            // Arrange
            Sudoku problem = new Sudoku(new int[,]
            {
                { 3, 0, 0, 7, 0, 1, 0, 0, 6, },
                { 0, 0, 0, 0, 0, 9, 0, 7, 0, },
                { 0, 7, 0, 0, 5, 0, 1, 0, 0, },
                { 2, 0, 3, 0, 0, 6, 7, 0, 0, },
                { 0, 0, 0, 0, 0, 0, 2, 0, 0, },
                { 8, 0, 0, 1, 0, 2, 0, 0, 0, },
                { 5, 0, 1, 2, 0, 4, 0, 0, 7, },
                { 0, 6, 0, 9, 0, 0, 0, 0, 0, },
                { 0, 0, 0, 0, 0, 5, 3, 0, 0, },
            });
            SudokuSolver solver = new SudokuSolver();

            // Act
            Sudoku result = solver.Solve(problem);

            // Arrange
            Assert.IsTrue(SudokuChecker.DoesNotHallucinate(problem, result) && SudokuChecker.IsSudokuValid(result.Board));
        }
        [TestMethod]
        public void Sudoku_CorrectSolution_10()
        {
            // Arrange
            Sudoku problem = new Sudoku(new int[,]
            {
                { 0, 0, 0, 7, 0, 4, 2, 6, 0, },
                { 0, 2, 0, 0, 0, 0, 0, 0, 0, },
                { 0, 0, 0, 2, 0, 0, 0, 0, 4, },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, },
                { 0, 0, 6, 0, 0, 0, 0, 1, 0, },
                { 0, 0, 0, 0, 2, 3, 0, 0, 0, },
                { 0, 4, 0, 6, 0, 0, 9, 0, 8, },
                { 6, 0, 2, 0, 0, 8, 5, 4, 1, },
                { 9, 8, 1, 0, 4, 0, 0, 7, 3, },
            });
            SudokuSolver solver = new SudokuSolver();

            // Act
            Sudoku result = solver.Solve(problem);

            // Arrange
            Assert.IsTrue(SudokuChecker.DoesNotHallucinate(problem, result) && SudokuChecker.IsSudokuValid(result.Board));
        }

    }
}
