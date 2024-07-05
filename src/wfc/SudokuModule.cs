using Refit;

namespace wfc
{
    public class SudokuChecker
    {
        public static bool DoesNotHallucinate(Sudoku example, Sudoku solution)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (example.Board[i, j] > 0 && example.Board[i, j] != solution.Board[i, j])
                    {
                        Console.WriteLine($"Problem at {i}, {j}");
                        return false;
                    }
                }
            }
            return true;
        }
        public static bool IsSudokuValid(int[,] board)
        {
            return CheckRows(board) && CheckColumns(board) && CheckSubGrids(board);
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
    public class SudokuSolver
    {
        private readonly Solver solver;
        public SudokuSolver()
        {
            solver = new Solver(
                [1, 1, 1, 1, 1, 1, 1, 1, 1],
                new Rulebook(Rulebook.GetColoringRules(9))
                );
        }
        public Sudoku Solve(Sudoku example)
        {
            Graph g = new Graph(SudokuModule.GetSudokuEdges(), 9, GraphDirectedness.Undirected);
            Graph result = solver.Solve(g, SudokuModule.GetSetCells(example));
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
        public static (int, int) Add(this (int, int) tuple, int add)
        {
            return (tuple.Item1 + add, tuple.Item2 + add);
        }
        public static List<(int, int)> GetSetCells(Sudoku example)
        {
            List<(int, int)> output = new();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (example.Board[i, j] > 0)
                    {
                        output.Add((i * 9 + j, example.Board[i, j] - 1));
                    }
                }
            }
            return output;
        }
        public static List<(int, int)> GetSudokuEdges()
        {
            List<(int, int)> edges = new List<(int, int)>();
            //return edges;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    for (int k = 1; j + k < 9; k++)
                    {
                        // rows
                        edges.Add((i * 9 + j, i * 9 + j + k));
                        // edges
                        edges.Add((i + 9 * j, i + 9 * (j + k)));
                    }
                }
            }

            (int, int)[] directions = [
                (0, 1), (0, 2), (0, 9), (0, 10), (0, 11), (0, 18), (0, 19), (0, 20),
                (1, 2), (1, 9), (1, 10), (1, 11), (1, 18), (1, 19), (1, 20),
                (2, 9), (2, 10), (2, 11), (2, 18), (2, 19), (2, 20),
                (9, 10), (9, 11), (9, 18), (9, 19), (9, 20),
                (10, 11), (10, 18), (10, 19), (10, 20),
                (11, 18), (11, 19), (11, 20),
                (18, 19), (18, 20),
                (19, 20)
            ];

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    foreach ((int, int) s in directions)
                    {
                        edges.Add(s.Add(j * 27 + i * 3));
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
    public class SudokuGenerator
    {
        private ISudokuApi apiCaller;
        public SudokuGenerator()
        {
            apiCaller = RestService.For<ISudokuApi>("https://sudoku-api.vercel.app/api");
        }
        public (Sudoku, Sudoku, string) GetNewBoard()
        {
            var item = apiCaller.GetSudokuBoard().Result;
            return (
                new Sudoku(item.newboard.grids[0].value),
                new Sudoku(item.newboard.grids[0].solution),
                item.newboard.grids[0].difficulty
                );
        }
        #region helper_classes
        public interface ISudokuApi
        {
            [Get("/dosuku")]
            Task<SudokuBoard> GetSudokuBoard();
        }
        public class SudokuBoard
        {
            public NewBoard newboard { get; set; }
        }
        public class NewBoard
        {
            public Grid[] grids { get; set; }
        }
        public class Grid
        {
            public int[][] value { get; set; }
            public int[][] solution { get; set; }
            public string difficulty { get; set; }
        }
        #endregion
    }
}