using GBWFC.Modules;
using System.Diagnostics;

namespace SudokuShowcase
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // setup sudoku generator and solver
            SudokuGenerator gen = new();
            (Sudoku problem, SudokuDifficulty diff) = gen.GetNewBoard(SudokuDifficulty.hard);
            SudokuSolver solver = new();

            Stopwatch sw = new Stopwatch();
            Console.WriteLine("Now solving:");
            Console.WriteLine($"Difficulty: {Enum.GetName(typeof(SudokuDifficulty), diff)}");
            Console.WriteLine(problem);

            sw.Start();

            Sudoku solved = solver.Solve(problem);

            sw.Stop();

            if (SudokuChecker.IsValidSudoku(solved))
            {
                Console.WriteLine("Solved:");
                Console.WriteLine(solved);
                Console.WriteLine($"Solved in {sw.Elapsed.Milliseconds} ms", sw.Elapsed);
            }
        }
    }
}
