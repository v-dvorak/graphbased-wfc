using DotNetGraph.Compilation;
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using GBWFC.Graph;
using GBWFC.Modules;

namespace GBWFC.Modules
{
    public class GraphModule
    {
        /// <summary>
        /// Takes a graph in <c>Graph</c> format and turns int into a <c>DotGraph</c>.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="graphIdentifier">Graph name under which it will stored in the DOT format.</param>
        /// <param name="colors">List of colors that will to applied to nodes.</param>
        /// <param name="graphConfig">Action that will be applied to graph.</param>
        /// <param name="nodeConfig">Action that will be applied to every node.</param>
        /// <param name="edgeConfig">Action that will be applied to every edge.</param>
        /// <param name="removeDuplicates">Undirected <c>Graph</c> is represented as directed with two directed edges between nodes,
        /// this leads to doubling of edges in generated graph.</param>
        /// <returns><c>DotGraph</c>.</returns>
        public static DotGraph ConvertGraphToDotGraph(
            Graph.WFCGraph graph,
            string graphIdentifier = "Graph",
            string[]? colors = null,
            Action<DotGraph>? graphConfig = null,
            Action<DotNode>? nodeConfig = null,
            Action<DotEdge>? edgeConfig = null,
            bool removeDuplicates = true
            )
        {
            HashSet<(int, int)> assignedNodes = new();
            // create graph
            DotGraph dotGraph = new DotGraph().WithIdentifier(graphIdentifier);
            graphConfig?.Invoke(dotGraph);

            // create nodes
            foreach (Node node in graph.AllNodes)
            {
                var dotNode = new DotNode();
                nodeConfig?.Invoke(dotNode);
                dotNode.WithIdentifier(node.Id.ToString());
                if (colors is not null)
                {
                    dotNode.WithFillColor(colors[node.AssignedValue]);
                    dotNode.WithFontColor(Colors.DistinctColors.GetFontColor(colors[node.AssignedValue]));
                }
                dotGraph.Add(dotNode);
            }
            // create edges
            foreach (Node node in graph.AllNodes)
            {
                foreach (Node child in node.Children)
                {
                    if (!removeDuplicates ||
                        (!assignedNodes.Contains((child.Id, node.Id))
                        && !assignedNodes.Contains((node.Id, child.Id))))
                    {
                        var edge = new DotEdge();
                        edgeConfig?.Invoke(edge);
                        edge.From(node.Id.ToString()).To(child.Id.ToString());
                        //Console.WriteLine($"{node.Id} -> {child.Id}");
                        dotGraph.Add(edge);
                        assignedNodes.Add((node.Id, child.Id));
                    }
                }
            }
            return dotGraph;
        }
        public enum GraphVizEngine { dot, neato, fdp, sfdp, circo, twopi, osage, }
        /// <summary>
        /// Basic method that takes in <c>Graph</c> and renders is into an image using the GraphViz library, runs GraphViz using command line.
        /// </summary>
        /// <param name="graph"></param>
        public static void CreateImage(
            Graph.WFCGraph graph,
            string outputImagePath,
            string graphVizLibraryPath,
            GraphVizEngine engine = GraphVizEngine.dot
            )
        {
            var dotGraph = ConvertGraphToDotGraph(graph, colors: Colors.DistinctColors.Colors, nodeConfig: node => node
                .WithShape(DotNodeShape.Circle)
                .WithStyle(DotNodeStyle.Bold)
                .WithStyle(DotNodeStyle.Filled)
                .WithWidth(0.5)
                .WithHeight(0.5)
                .WithPenWidth(1.5)
            );


            // Save the DOT file
            using var writer = new StringWriter();
            var context = new CompilationContext(writer, new CompilationOptions());
            dotGraph.CompileAsync(context);

            var result = writer.GetStringBuilder().ToString();

            // Save it to a file
            File.WriteAllText("graph.dot", result);

            GenerateImageFromDot("graph.dot", outputImagePath, graphVizLibraryPath, engine);
        }
        /// <summary>
        /// Runs chosen GraphViz engine from command line with given arguments.
        /// </summary>
        /// <param name="dotFilePath"></param>
        /// <param name="outputImagePath"></param>
        /// <param name="graphVizLibraryPath"></param>
        /// <param name="engine"></param>
        static void GenerateImageFromDot(
            string dotFilePath,
            string outputImagePath,
            string graphVizLibraryPath,
            GraphVizEngine engine = GraphVizEngine.dot
            )
        {
            string engineName = Enum.GetName(typeof(GraphVizEngine), engine);
            Console.WriteLine($"Saving to {outputImagePath}");
            var command = $"{graphVizLibraryPath}\\{engineName}dot.exe -Tpng \"{dotFilePath}\" -o \"{outputImagePath}\"";
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }).WaitForExit();
        }
        /// <summary>
        /// Opens given image inside an image explorer. Only tested for Windows.
        /// </summary>
        /// <param name="imagePath">Path to image to show.</param>
        public static void OpenImage(string imagePath)
        {
            System.Diagnostics.Process.Start("explorer.exe", imagePath).WaitForExit();
        }
    }

}