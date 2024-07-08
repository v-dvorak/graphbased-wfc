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

            List<Edge> edges = GridModule.GetGridEdges(height, width, overlap);
            Graph graph = new Graph(edges, solver.SolverRulebook.GetRuleCount(), GraphDirectedness.Undirected);
            Graph result = solver.Solve(graph);
            return GridModule.GraphToGrid(result, new int[height, width]);
        }
    }
    public static class GridModule
    {
        /// <summary>
        /// Returns list of edges/relations between cells inside a 2D grid.
        /// </summary>
        /// <param name="height">Grid height.</param>
        /// <param name="width">Grid width.</param>
        /// <param name="overlap">True if relations should overlap, first cell in row is a neighbor with the last cell in the same row.</param>
        /// <returns>List of edges.</returns>
        public static List<Edge> GetGridEdges(int height, int width, bool overlap = false)
        {
            List<Edge> output = new();
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (j + 1 < width)
                    {
                        output.Add((i * height + j, i * height + j + 1).Edge());
                    }
                    else if (overlap)
                    {
                        output.Add((i * height + j, i * height).Edge());
                    }
                    if (i + 1 < height)
                    {
                        output.Add((i * height + j, (i + 1) * height + j).Edge());
                    }
                    else if (overlap)
                    {
                        output.Add((i * height + j, j).Edge());
                    }
                }
            }
            return output;
        }
        /// <summary>
        /// Returns list of edges/relations between cells inside a 2D grid.
        /// </summary>
        /// <param name="overlap">True if relations should overlap, first cell in row is a neighbor with the last cell in the same row.</param>
        /// <returns>List of edges.</returns>
        public static List<Edge> GetGridEdges(int[,] grid, bool overlap = false)
        {
            return GetGridEdges(grid.GetLength(0), grid.GetLength(1), overlap: overlap);
        }
        /// <summary>
        /// Assigns values from <c>Graph</c> back to a grid.
        /// </summary>
        /// <param name="graph">Solved <c>Graph</c>.</param>
        /// <param name="input">Grid to output to, used to get grid dimensions.</param>
        /// <returns></returns>
        public static int[,] GraphToGrid(Graph graph, int[,] input)
        {
            return GraphToGrid(graph, input.GetLength(0), input.GetLength(1));
        }
        /// <summary>
        /// Assigns values from <c>Graph</c> back to a grid.
        /// </summary>
        /// <param name="graph">Solved <c>Graph</c>.</param>
        /// <param name="height">Output grid height.</param>
        /// <param name="width">Output grid width.</param>
        /// <returns></returns>
        public static int[,] GraphToGrid(Graph graph, int height, int width)
        {
            int[,] output = new int[height, width];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    output[i, j] = graph.AllNodes[i * width + j].AssignedValue;
                }
            }
            return output;
        }
        /// <summary>
        /// Generates bitmap from given grid with given color palette.
        /// </summary>
        /// <param name="grid">Input grid, based on this the colors are chosen.</param>
        /// <param name="colors">List of colors, are used to fill the grid.</param>
        /// <param name="squareSize">Size of one square in pixels.</param>
        /// <returns></returns>
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
