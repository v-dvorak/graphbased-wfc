namespace GBWFC.Graph
{
    public enum GraphDirectedness { Undirected, Directed };
    public class WFCGraph
    {
        public Node[] AllNodes;
        public int TotalOptions;
        private int totalAssigned = 0;
        public bool AllSet { get => AllNodes.Length <= totalAssigned; }
        public WFCGraph(Node[] nodes, int totalOptions = -1)
        {
            AllNodes = nodes;
            TotalOptions = totalOptions;
            totalAssigned = 0;
            if (totalOptions != -1)
            {
                InitializeNodeOptions(totalOptions);
            }
        }
        public WFCGraph(IReadOnlyList<Edge> edges, GraphDirectedness direct = GraphDirectedness.Directed, int options = -1)
        {
            TotalOptions = options;
            totalAssigned = 0;
            // Determine unique node IDs from edges
            HashSet<int> nodeIds = new HashSet<int>();
            foreach (var edge in edges)
            {
                nodeIds.Add(edge.Parent); // Parent node ID
                nodeIds.Add(edge.Child); // Child node ID
            }

            // Create nodes and populate AllNodes array
            AllNodes = new Node[nodeIds.Count];

            foreach (int nodeId in nodeIds)
            {
                Node newNode = new Node(nodeId);
                AllNodes[nodeId] = newNode;
            }

            // Build relationships (edges) between nodes
            foreach (var edge in edges)
            {
                int parentId = edge.Parent;
                int childId = edge.Child;

                Node parentNode = AllNodes[parentId];
                Node childNode = AllNodes[childId];

                parentNode.AddChild(childNode);
                childNode.AddParent(parentNode);
                if (direct == GraphDirectedness.Undirected)
                {
                    parentNode.AddParent(childNode);
                    childNode.AddChild(parentNode);
                }
            }
            if (options != -1)
            {
                InitializeNodeOptions(options);
            }
        }
        /// <summary>
        /// Should only be used by the <see cref="Copy()"/> method!
        /// </summary>
        /// <param name="total">Value to change <see cref="totalAssigned"/> to.</param>
        public void SetTotalAssigned(int total)
        {
            totalAssigned = total;
        }
        public WFCGraph Copy()
        {
            // initialize all nodes
            Node[] newNodes = new Node[AllNodes.Length];
            for (int i = 0; i < AllNodes.Length; i++)
            {
                newNodes[i] = new Node(AllNodes[i].Id, 0);
                // options
                newNodes[i].Options = AllNodes[i].Options.Copy();
                // value
                if (AllNodes[i].IsSet)
                {
                    newNodes[i].AssignValue(AllNodes[i].AssignedValue);
                }
            }

            // assign parents and children
            for (int i = 0; i < newNodes.Length; i++)
            {
                foreach (Node child in AllNodes[i].Children)
                {
                    newNodes[i].AddChild(newNodes[child.Id]);
                }
                foreach (Node parent in AllNodes[i].Parents)
                {
                    newNodes[i].AddParent(newNodes[parent.Id]);
                }
            }
            WFCGraph g = new WFCGraph(newNodes, TotalOptions);
            g.SetTotalAssigned(totalAssigned);
            return g;
        }
        /// <summary>
        /// Sets value <see cref="int"/> to a given <see cref="Node"/>, does not update node's options.
        /// </summary>
        /// <param name="node">Node whose value will change.</param>
        /// <param name="chosen">Value to assign to given node.</param>
        public void AssignValueToNode(Node node, int chosen)
        {
            // assigns value to node, has no side effects
            node.AssignValue(chosen);
            totalAssigned += 1;
        }
        /// <summary>
        /// Initializes all nodes' <see cref="Node.Options"/> to given amount of options.
        /// </summary>
        /// <param name="options">Number of options every node will have.</param>
        public void InitializeNodeOptions(int options)
        {
            foreach (Node node in AllNodes)
            {
                node.InitializeOptions(options);
            }
        }
        /// <summary>
        /// Resets single node's <see cref="Node.AssignedValue"/> and account for it in <see cref="totalAssigned"/>.
        /// Throws an exception if given node does not have a value assigned.
        /// </summary>
        /// <param name="node">Node whose value will be reset.</param>
        /// <exception cref="ArgumentException">Given node does not gave a value assigned.</exception>
        public void ResetValueAssignment(Node node)
        {
            if (!node.IsSet)
            {
                throw new ArgumentException("node", "Cannot reset unsigned node");
            }
            node.ResetValue();
            totalAssigned -= 1;
        }
        /// <summary>
        /// Goes through every <see cref="Node"/> and resets if <see cref="Node.Options"/>.
        /// </summary>
        public void Reset()
        {
            foreach (Node node in AllNodes)
            {
                node.InitializeOptions(TotalOptions);
            }
        }
        /// <summary>
        /// Parses a list of edges from a specified file.
        /// </summary>
        /// <param name="filePath">The path to the file containing graph's edges.</param>
        /// <returns>A list of <see cref="Edge"/> structs parsed from the file.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is null or empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist.</exception>
        /// <exception cref="IOException">Thrown when an I/O error occurs while opening the file.</exception>
        /// <exception cref="FormatException">Thrown when the file contains invalid edge data.</exception>
        /// <remarks>
        /// This method reads a file specified by <paramref name="filePath"/> and parses each line to create a list of edges.
        /// Each line in the file should represent an edge with two integers separated by a space.
        /// Example file content:
        /// <code>
        /// 1 2
        /// 2 3
        /// 3 4
        /// </code>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the <see cref="ParseEdgesFromFile"/> method:
        /// <code>
        /// using System;
        /// using System.Collections.Generic;
        ///
        /// public class Program
        /// {
        ///     public static void Main()
        ///     {
        ///         try
        ///         {
        ///             string filePath = "edges.txt";
        ///             List&lt;Edge&gt; edges = ParseEdgesFromFile(filePath);
        ///
        ///             foreach (var edge in edges)
        ///             {
        ///                 Console.WriteLine($"Edge: {edge.First} - {edge.Second}");
        ///             }
        ///         }
        ///         catch (Exception ex)
        ///         {
        ///             Console.WriteLine($"An error occurred: {ex.Message}");
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public static List<Edge> ParseEdgesFromFile(string filePath)
        {
            List<Edge> edges = new List<Edge>();

            try
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 2)
                        {
                            // parse
                            int first = int.Parse(parts[0]);
                            int second = int.Parse(parts[1]);
                            // add
                            edges.Add(new Edge(first, second));
                        }
                        else
                        {
                            Console.WriteLine($"Invalid line format: {line}. Skipping.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
                return null;
            }

            return edges;
        }
        
        public List<Edge> GetEdges()
        {
            List<Edge> edges = new();
            foreach (Node parent in AllNodes)
            {
                foreach (Node child in parent.Children)
                {
                    edges.Add((parent.Id, child.Id).Edge());
                }
            }
            return edges;
        }
        /// <summary>
        /// Collects all assigned values into a <see cref="List{int}"/>.
        /// </summary>
        /// <returns>List of <c>int</c>s.</returns>
        public List<int> ToList()
        {
            List<int> output = new(AllNodes.Length);
            foreach (Node node in AllNodes)
            {
                output.Add(node.AssignedValue);
            }
            return output;
        }
        /// <summary>
        /// Collects all assigned values into a <see cref="List{(int, int)}"/>, the format is <c>(nodeId, assignedValue)</c>.
        /// </summary>
        /// <returns>List of tuples, <c>(int, int)</c>.</returns>
        public List<(int, int)> ToListOfTuples()
        {
            List<(int, int)> output = new(AllNodes.Length);
            foreach (Node node in AllNodes)
            {
                output.Add((node.Id, node.AssignedValue));
            }
            return output;
        }
        public override string ToString()
        {
            string output = "Graph";
            foreach (Node node in AllNodes)
            {
                output += '\n' + node.ToString();
            }
            return output;
        }
    }
}
