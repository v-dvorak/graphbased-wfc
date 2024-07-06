using DotNetGraph.Compilation;
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using wfc.Examples;

namespace wfc.Examples
{
    class GraphModule
    {
        public static DotGraph ConvertGraphToDotGraph(Graph graph,
            string[]? colors = null,
            Action<DotNode>? nodeConfig = null,
            Action<DotEdge>? edgeConfig = null, bool removeDuplicates = true)
        {
            HashSet<(int, int)> assignedNodes = new();
            DotGraph dotGraph = new DotGraph().WithIdentifier("Grafus");
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
                        Console.WriteLine($"{node.Id} -> {child.Id}");
                        dotGraph.Add(edge);
                        assignedNodes.Add((node.Id, child.Id));
                    }
                }
            }
            return dotGraph;
        }
        public static void Run(Graph graph)
        {
            var dotGraph = ConvertGraphToDotGraph(graph, Colors.DistinctColors.Colors, node => node
                .WithShape(DotNodeShape.Circle)
                .WithFillColor(DotColor.Pink)
                .WithFontColor(DotColor.Blue)
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

            GenerateImageFromDot("graph.dot", "graph.png");
        }

        static void GenerateImageFromDot(string dotFilePath, string outputImagePath)
        {
            Console.WriteLine($"Saving to {outputImagePath}");
            var command = $"F:\\graphviz\\bin\\fdp.exe -Tpng \"{dotFilePath}\" -o \"{outputImagePath}\"";
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }).WaitForExit();
        }
    }

}