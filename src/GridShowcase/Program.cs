using GBWFC.Modules;
using GBWFC.Solver;

namespace GridShowcase
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Rule[] rules = Rulebook.CreateCascadeRules(5);
            GridSolver solver = new GridSolver([2, 3, 5, 3, 2], rules, overlap: true);

            Console.WriteLine("Solving...");
            int[,] solved = solver.Solve(30, 30);

            string outputName = "temp.png";
            string[] colors = ["#0c589c", "#f4c430", "#6cc24a", "#997163", " #bbbbbb"];

            Console.WriteLine("Generating image...");
            GridModule.GenerateImage(solved, colors).Save(outputName);

            Console.WriteLine("Saving...");
            GraphModule.OpenImage(outputName);
        }
    }
}
