namespace wfc
{
    public static class GraphExtensions
    {
        public static Graph Copy(this Graph graph)
        {
            return graph.Copy();
        }
    }
    public class Graph
    {
        public Node[] AllNodes;
        public int TotalOptions;
        private int totalAssigned = 0;
        public bool AllSet
        {
            get
            {
                return AllNodes.Length <= totalAssigned;
            }
        }
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
                if (AllNodes[i].IsSet())
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

        public void AssignValueToNode(Node node, int chosen)
        {
            node.AssignValue(chosen);
            totalAssigned += 1;
        }
        public void ResetValueAssignment(Node node)
        {
            if (!node.IsSet())
            {
                throw new ArgumentException("node", "Cannot reset unsigned node");
            }
            node.ResetValue();
            totalAssigned -= 1;
        }
        public Graph(Node[] nodes, int totalOptions)
        {
            AllNodes = nodes;
            TotalOptions = totalOptions;
            totalAssigned = 0;
        }

        public Graph(List<(int, int)> edges, int options)
        {
            TotalOptions = options;
            // Determine unique node IDs from edges
            HashSet<int> nodeIds = new HashSet<int>();
            foreach (var edge in edges)
            {
                nodeIds.Add(edge.Item1); // Parent node ID
                nodeIds.Add(edge.Item2); // Child node ID
            }

            // Create nodes and populate AllNodes array
            AllNodes = new Node[nodeIds.Count];
            Dictionary<int, Node> nodeMap = new Dictionary<int, Node>();

            int index = 0;
            foreach (int nodeId in nodeIds)
            {
                Node newNode = new Node(nodeId, TotalOptions);
                AllNodes[index++] = newNode;
                nodeMap[nodeId] = newNode;
            }

            // Build relationships (edges) between nodes
            foreach (var edge in edges)
            {
                int parentId = edge.Item1;
                int childId = edge.Item2;

                Node parentNode = nodeMap[parentId];
                Node childNode = nodeMap[childId];

                parentNode.AddChild(childNode);
                childNode.AddParent(parentNode);
            }
        }
        public void ResetFromLogger(ChangeLogger logger)
        {
            // reset options
            foreach ((Node node, int option) in logger.optionsChanged)
            {
                node.Options.Add(option);
            }
            // reset values
            foreach (Node node in logger.assignedChanged)
            {
                node.ResetValue();
            }
        }
        public static List<(int, int)> ParseEdgesFromFile(string filePath)
        {
            List<(int, int)> edges = new List<(int, int)>();

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
                            edges.Add((int.Parse(parts[0]), int.Parse(parts[1])));
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

    public class ChangeLogger
    {
        public List<Node> assignedChanged { get; private set; }
        public List<(Node, int)> optionsChanged { get; private set; }

        public ChangeLogger()
        {
            assignedChanged = new();
            optionsChanged = new();
        }
        public void LogValueAssignment(Node node)
        {
            assignedChanged.Add(node);
        }
        public void LogOptionRemoval(Node node, int optionRemoved)
        {
            optionsChanged.Add((node, optionRemoved));
        }
    }
    public class Node
    {
        public int Id { get; }
        public int AssignedValue { get; private set; } = -1;
        private bool isSet = false;
        public List<Node> Parents;
        public List<Node> Children;
        public HashSet<int> Options;
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
        public void UpdateOptions(int val)
        {
            Options.Remove(val);
        }
        public bool IsSet()
        {
            return isSet;
        }
        public void AssignValue(int value)
        {
            if (IsSet()) throw new Exception("Can't set value, value is already set.");

            AssignedValue = value;
            isSet = true;
        }
        public void ResetValue()
        {
            AssignedValue = -1;
            isSet = false;
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
