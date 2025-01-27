﻿using Refit;
using GBWFC.Graph;
using GBWFC.Solver;

namespace GBWFC.Modules
{
    public class SudokuSolver : ISolver<Sudoku>
    {
        private readonly WFCSolver solver;
        public SudokuSolver()
        {
            solver = new WFCSolver(
                new Rulebook(Rulebook.CreateColoringRules(9))
                );
        }
        public Sudoku Solve(Sudoku problem)
        {
            Graph.WFCGraph g = new Graph.WFCGraph(SudokuModule.GetSudokuEdges(), GraphDirectedness.Undirected);
            Graph.WFCGraph result = solver.Solve(g, SudokuModule.GetSetCells(problem));
            Sudoku output = new();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    output.Board[i, j] = result.AllNodes[i * 9 + j].AssignedValue + 1;
                }
            }

            return output;
        }
    }
    public static class SudokuModule
    {
        private static (int, int) Add(this (int, int) tuple, int add)
        {
            return (tuple.Item1 + add, tuple.Item2 + add);
        }
        /// <summary>
        /// Generates code for Sudoku testing.
        /// </summary>
        /// <param name="sudoku">Sudoku problem to be as test.</param>
        /// <param name="testName">Test method name.</param>
        /// <returns></returns>
        public static string GenerateTestCase(Sudoku sudoku, string testName)
        {
            string output = $"[TestMethod]\npublic void {testName}()\n{{\n// Arrange\nSudoku problem = new Sudoku(new int[,]\n{{\n";
            for (int i = 0; i < 9; i++)
            {
                output += "\t\t{ ";
                for (int j = 0; j < 9; j++)
                {
                    output += $"{sudoku.Board[i, j]}, ";
                }
                output += "},\n";
            }
            output += "});\nSudokuSolver solver = new SudokuSolver();\n\n// Act\nSudoku result = solver.Solve(problem);\n\n// Arrange\nAssert.IsTrue(SudokuChecker.DoesNotHallucinate(problem, result) && SudokuChecker.IsSudokuValid(result.Board));\n}";
            return output;
        }
        /// <summary>
        /// Generates code for Sudoku testing, calls the Sudoku API for new board.
        /// </summary>
        /// <param name="testName">Test method name.</param>
        /// <returns></returns>
        public static string GenerateTestCase(string testName)
        {
            return GenerateTestCase(new SudokuGenerator().GetNewBoard(SudokuDifficulty.hard).Item1, testName);
        }
        /// <summary>
        /// Goes through given Sudoku problem and returns a list of Constraints 
        /// </summary>
        /// <param name="problem"><c>Sudoku</c> problem to work with.</param>
        /// <returns></returns>
        public static List<ConstraintById> GetSetCells(Sudoku problem)
        {
            List<ConstraintById> output = new();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (problem.Board[i, j] > 0)
                    {
                        output.Add(new ConstraintById(i * 9 + j, problem.Board[i, j] - 1));
                    }
                }
            }
            return output;
        }
        /// <summary>
        /// Returns list of edges/relations between cells in Sudoku.
        /// </summary>
        /// <returns>List of edges.</returns>
        public static List<Edge> GetSudokuEdges()
        {
            List<Edge> edges = new();
            //return edges;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    for (int k = 1; j + k < 9; k++)
                    {
                        // rows
                        edges.Add((i * 9 + j, i * 9 + j + k).Edge());
                        // edges
                        edges.Add((i + 9 * j, i + 9 * (j + k)).Edge());
                    }
                }
            }
            // not an ideal solution, but it works
            (int, int)[] basicRelations = [
                (0, 1),   (0, 2),   (0, 9),   (0, 10), (0, 11), (0, 18), (0, 19), (0, 20),
                (1, 2),   (1, 9),   (1, 10),  (1, 11), (1, 18), (1, 19), (1, 20),
                (2, 9),   (2, 10),  (2, 11),  (2, 18), (2, 19), (2, 20),
                (9, 10),  (9, 11),  (9, 18),  (9, 19), (9, 20),
                (10, 11), (10, 18), (10, 19), (10, 20),
                (11, 18), (11, 19), (11, 20),
                (18, 19), (18, 20),
                (19, 20)
            ];

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    foreach ((int, int) relation in basicRelations)
                    {
                        edges.Add(relation.Add(j * 27 + i * 3).Edge());
                    }
                }
            }
            return edges;
        }
    }
    public class Sudoku : IEquatable<Sudoku>
    {
        public int[,] Board;
        public Sudoku(int[,] board)
        {
            Board = board;
        }
        public Sudoku(int[][] board)
        {
            int rows = board.Length;
            int cols = board[0].Length;
            int[,] result = new int[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    result[i, j] = board[i][j];
                }
            }
            Board = result;
        }
        public Sudoku()
        {
            Board = new int[9, 9];
        }
        public bool Equals(Sudoku? other)
        {
            if (other is null) return false;

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (Board[i, j] != other.Board[i, j]) return false;
                }
            }
            return true;
        }
        public override string ToString()
        {
            string output = "";
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    output += $"{Board[i, j]} ";
                }
                output += "\n";
            }
            return output;
        }
    }
    public enum SudokuDifficulty { easy, medium, hard, random };
    public class SudokuGenerator
    {
        private ISudokuApi apiCaller;
        public SudokuGenerator()
        {
            apiCaller = RestService.For<ISudokuApi>("https://sugoku.onrender.com");
        }
        /// <summary>
        /// Requests new board from "sugoku.onrender.com" and returns Sudoku board along with its difficulty.
        /// </summary>
        /// <param name="difficulty">Requested difficulty.</param>
        /// <returns></returns>
        public (Sudoku, string) GetNewBoard(string difficulty)
        {
            var item = apiCaller.GetSudokuBoard(difficulty).Result;
            return (new Sudoku(item.board), "hard");
        }
        /// <summary>
        /// Requests new board from "sugoku.onrender.com" and returns Sudoku board along with its difficulty.
        /// </summary>
        /// <param name="difficulty">Requested difficulty.</param>
        /// <returns></returns>
        public (Sudoku, SudokuDifficulty) GetNewBoard(SudokuDifficulty difficulty)
        {
            string diffName = Enum.GetName(typeof(SudokuDifficulty), difficulty);
            var item = apiCaller.GetSudokuBoard(diffName).Result;
            return (new Sudoku(item.board), difficulty);
        }
        #region helper_classes
        public interface ISudokuApi
        {
            [Get("/board?difficulty={difficulty}")]
            Task<SudokuBoard> GetSudokuBoard(string difficulty);
        }
        public class SudokuBoard
        {
            //public NewBoard newboard { get; set; }
            public int[][] board { get; set; }
        }
        #endregion
    }
    public static class SudokuChecker
    {
        /// <summary>
        /// Given a <c>Sudoku</c> problem and a proposed solution, checks if set cells remained untouched.
        /// </summary>
        /// <param name="problem">Original <c>Sudoku</c> problem.</param>
        /// <param name="solution">Proposed solution.</param>
        /// <returns></returns>
        public static bool DoesNotHallucinate(Sudoku problem, Sudoku solution)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (problem.Board[i, j] > 0 && problem.Board[i, j] != solution.Board[i, j])
                    {
                        Console.WriteLine($"Problem at {i}, {j}");
                        return false;
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// Returns true if board is a valid and solved <c>Sudoku</c> board..
        /// </summary>
        /// <param name="board">Proposed solution.</param>
        /// <returns></returns>
        public static bool IsValidSudoku(int[,] board)
        {
            return CheckRows(board) && CheckColumns(board) && CheckSubGrids(board);
        }
        public static bool IsValidSudoku(Sudoku sudoku)
        {
            return IsValidSudoku(sudoku.Board);
        }
        private static bool CheckRows(int[,] board)
        {
            for (int row = 0; row < 9; row++)
            {
                HashSet<int> seen = new HashSet<int>();
                for (int col = 0; col < 9; col++)
                {
                    int num = board[row, col];
                    if (num < 1 || num > 9 || !seen.Add(num))
                    {
                        Console.WriteLine($"Problem at row {row}");
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool CheckColumns(int[,] board)
        {
            for (int col = 0; col < 9; col++)
            {
                HashSet<int> seen = new HashSet<int>();
                for (int row = 0; row < 9; row++)
                {
                    int num = board[row, col];
                    if (num < 1 || num > 9 || !seen.Add(num))
                    {
                        Console.WriteLine($"Problem at column {col}");
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool CheckSubGrids(int[,] board)
        {
            for (int startRow = 0; startRow < 9; startRow += 3)
            {
                for (int startCol = 0; startCol < 9; startCol += 3)
                {
                    if (!CheckSubGrid(board, startRow, startCol))
                    {
                        Console.WriteLine($"Problem at subgrid {startRow}, {startCol}");
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool CheckSubGrid(int[,] board, int startRow, int startCol)
        {
            HashSet<int> seen = new HashSet<int>();
            for (int row = startRow; row < startRow + 3; row++)
            {
                for (int col = startCol; col < startCol + 3; col++)
                {
                    int num = board[row, col];
                    if (num < 1 || num > 9 || !seen.Add(num))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}