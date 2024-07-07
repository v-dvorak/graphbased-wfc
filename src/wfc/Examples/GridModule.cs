using System.Drawing;
namespace wfc.Examples
{
    public class GridSolver : ISolver<int[,]>
    {
        private readonly Solver solver;
        private readonly bool overlap;
        public GridSolver(int[] globalWeights, Rule[] rules, bool overlap = false)
        {
            solver = new Solver(globalWeights, new Rulebook(rules));
            this.overlap = overlap;
        }
        public GridSolver(int[] globalWeights, Rulebook rulebook, bool overlap = false)
        {
            solver = new Solver(globalWeights, rulebook);
            this.overlap = overlap;
        }
        public int[,] Solve(int[,] grid)
        {
            int height = grid.GetLength(0);
            int width = grid.GetLength(1);

            List<(int, int)> edges = GridModule.GetGridEdges(height, width, overlap);
            Graph graph = new Graph(edges, solver.SolverRulebook.GetRuleCount(), GraphDirectedness.Undirected);
            Graph result = solver.Solve(graph);
            return GridModule.GraphToGrid(result, new int[height, width]);
        }
    }
    public static class GridModule
    {
        public static List<(int, int)> GetGridEdges(int height, int width, bool overlap = false)
        {
            List<(int, int)> output = new();
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (j + 1 < width)
                    {
                        output.Add((i * height + j, i * height + j + 1));
                    }
                    else if (overlap)
                    {
                        output.Add((i * height + j, i * height));
                    }
                    if (i + 1 < height)
                    {
                        output.Add((i * height + j, (i + 1) * height + j));
                    }
                    else if (overlap)
                    {
                        output.Add((i * height + j, j));
                    }
                }
            }
            return output;
        }
        public static int[,] GraphToGrid(Graph graph, int[,] input)
        {
            int height = input.GetLength(0);
            int width = input.GetLength(1);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    input[i, j] = graph.AllNodes[i * width + j].AssignedValue;
                }
            }
            return input;
        }
        public static List<(int, int)> GetGridEdges(int[,] grid, bool overlap = false)
        {
            return GetGridEdges(grid.GetLength(0), grid.GetLength(1), overlap: overlap);
        }
        public static Bitmap GenerateImage(int[,] grid, IReadOnlyList<string> colors, int squareSize = 50)
        {
            int height = grid.GetLength(0);
            int width = grid.GetLength(1);

            Bitmap bitmap = new Bitmap(width * squareSize, height * squareSize);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int colorIndex = grid[y, x];
                        Color color = ColorTranslator.FromHtml(colors[colorIndex]);
                        using (Brush brush = new SolidBrush(color))
                        {
                            g.FillRectangle(brush, x * squareSize, y * squareSize, squareSize, squareSize);
                        }
                    }
                }
            }
            return bitmap;
        }
    }
}
