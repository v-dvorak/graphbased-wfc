namespace wfc
{
    public struct Edge
    {
        public int Parent;
        public int Child;
        public Edge(int parent, int child)
        {
            Parent = parent;
            Child = child;
        }
        public Edge((int, int) edge)
        {
            Parent = edge.Item1;
            Child = edge.Item2;
        }
    }
    public struct ConstraintById
    {
        public int NodeId;
        public int ForcedValue;
        public ConstraintById(int nodeId, int forceValue)
        {
            NodeId = nodeId;
            ForcedValue = forceValue;
        }
        public ConstraintById((int, int) constraint)
        {
            NodeId = constraint.Item1;
            ForcedValue = constraint.Item2;
        }
    }
    public struct ConstraintByNode
    {
        public Node Node;
        public int ForcedValue;
        public ConstraintByNode(Node node, int forcedValue)
        {
            Node = node;
            ForcedValue = forcedValue;
        }
        public ConstraintByNode((Node, int) constraint)
        {
            Node = constraint.Item1;
            ForcedValue = constraint.Item2;
        }
    }
    public enum GraphDirectedness { Undirected, Directed };
    public class Graph
    {
        public Node[] AllNodes;
        public int TotalOptions;
        private int totalAssigned = 0;
        public bool AllSet { get => AllNodes.Length <= totalAssigned; }
        public Graph(Node[] nodes, int totalOptions = -1)
        {
            AllNodes = nodes;
            TotalOptions = totalOptions;
            totalAssigned = 0;
            if (totalOptions != -1)
            {
                InitializeNodeOptions(totalOptions);
            }
        }
        public Graph(IReadOnlyList<Edge> edges, GraphDirectedness direct = GraphDirectedness.Directed, int options = -1)
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
        public Graph Copy()
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
            Graph g = new Graph(newNodes, TotalOptions);
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
        /// Resets single node's <see cref="Node.AssignedValue"/> and account for it in <see cref="Graph.totalAssigned"/>.
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

    public class Node
    {
        public int Id { get; }
        public int AssignedValue { get; private set; } = -1;
        public bool IsSet { get; private set; } = false;
        public List<Node> Parents;
        public List<Node> Children;
        public HashSet<int>? Options;
        public Node(int id, int totalOptions)
        {
            Id = id;
            Parents = new List<Node>();
            Children = new List<Node>();

            Options = new HashSet<int>(totalOptions);
            for (int i = 0; i < totalOptions; i++)
            {
                Options.Add(i);
            }
        }
        public Node(int id)
        {
            Id = id;

            Parents = new List<Node>();
            Children = new List<Node>();
        }
        public void UpdateOptions(int val)
        {
            Options.Remove(val);
        }
        public void InitializeOptions(int options)
        {
            Options = new();
            for (int i = 0; i < options; i++)
            {
                Options.Add(i);
            }
        }
        /// <summary>
        /// Based on given rules, updates options of <see cref="Children"/> and <see cref="Parents"/>.
        /// </summary>
        /// <param name="ruleForChildren"><see cref="Rule"/> to apply to <see cref="Children"/>.</param>
        /// <param name="ruleForParents"><see cref="Rule"/> to apply to <see cref="Parents"/>.</param>
        /// <returns></returns>
        public bool TryUpdateNodeNeighbors(Rule ruleForChildren, Rule ruleForParents)
        {
            foreach (Node child in Children)
            {
                if (child.IsSet)
                {
                    // chosen color does not satisfy current setting
                    if (!ruleForChildren.Options.Contains(child.AssignedValue))
                    {
                        return false;
                    }
                    continue;
                }

                child.Options.RemoveWhere(s => !ruleForChildren.Options.Contains(s));

                // no value can be set
                if (child.Options.Count == 0)
                {
                    return false;
                }
            }
            foreach (Node parent in Parents)
            {
                if (parent.IsSet)
                {
                    // chosen color does not satisfy current setting
                    if (!ruleForParents.Options.Contains(parent.AssignedValue))
                    {
                        return false;
                    }
                    continue;
                }

                parent.Options.RemoveWhere(s => !ruleForParents.Options.Contains(s));

                // no value can be set
                if (parent.Options.Count == 0)
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Assigns a value to the object if it has not already been set.
        /// </summary>
        /// <param name="value">The value to be assigned.</param>
        /// <exception cref="ArgumentException"> thrown when the value has already been set.</exception>
        public void AssignValue(int value)
        {
            if (IsSet) throw new ArgumentException("Can't set value, value is already set.", nameof(value));

            AssignedValue = value;
            IsSet = true;
        }
        /// <summary>
        /// Resets node's value.
        /// </summary>
        public void ResetValue()
        {
            AssignedValue = -1;
            IsSet = false;
        }
        public void AddChild(Node child)
        {
            Children.Add(child);
        }
        public void AddChild(List<Node> children)
        {
            Children.AddRange(children);
        }
        public void AddParent(Node parent)
        {
            Parents.Add(parent);
        }
        public void AddParent(List<Node> parents)
        {
            Parents.AddRange(parents);
        }
        public override string ToString()
        {
            return $"Node : Id = {Id}, Value = {AssignedValue}, Options = {string.Join(" ", Options)}";
        }
    }
}
