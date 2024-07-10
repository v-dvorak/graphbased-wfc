using System.Runtime.CompilerServices;

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
            // assigns value to node, has no side effects
            node.AssignValue(chosen);
            totalAssigned += 1;
        }
        public void InitializeNodeOptions(int options)
        {
            foreach (Node node in AllNodes)
            {
                node.InitializeOptions(options);
            }
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
        public void Reset()
        {
            HashSet<int> options = new HashSet<int>(TotalOptions);
            for (int i = 0; i < TotalOptions; i++)
            {
                options.Add(i);
            }
            foreach (Node node in AllNodes)
            {
                node.ResetValue();
                node.Options = options.Copy();
            }
        }
        public Graph(Node[] nodes, int totalOptions)
        {
            AllNodes = nodes;
            TotalOptions = totalOptions;
            totalAssigned = 0;
        }

        public Graph(IReadOnlyList<Edge> edges, GraphDirectedness direct = GraphDirectedness.Directed, int options = -1)
        {
            TotalOptions = options;
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
        private bool isSet = false;
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
        public bool TryUpdateNodeNeighbors(Rule ruleForChildren, Rule ruleForParents)
        {
            foreach (Node child in Children)
            {
                if (child.IsSet())
                {
                    // chosen color does not satisfy current setting
                    if (!ruleForChildren.Options.Contains(child.AssignedValue))
                    {
                        // continue while loop (with other options)
                        //Console.WriteLine("Child is not consistent");
                        //Console.WriteLine($"Child {child.Id}, Parent {Id}");
                        //Console.WriteLine($"Child options {child.AssignedValue}, Parent value {AssignedValue}");
                        return false;
                    }
                    continue;
                }

                child.Options.RemoveWhere(s => !ruleForChildren.Options.Contains(s));

                if (child.Options.Count == 0)
                {
                    // continue while loop (with other options)
                    //Console.WriteLine("Child cant be filled anymore");
                    //Console.WriteLine($"Child {child.Id}, Parent {Id}");
                    return false;
                }
            }
            foreach (Node parent in Parents)
            {
                if (parent.IsSet())
                {
                    // chosen color does not satisfy current setting
                    if (!ruleForParents.Options.Contains(parent.AssignedValue))
                    {
                        // continue while loop (with other options)
                        //Console.WriteLine("Parent is not consistent");
                        //Console.WriteLine($"Parent {parent.Id}, child {Id}");
                        //Console.WriteLine($"Parent options {parent.AssignedValue}, Child value {AssignedValue}");
                        return false;
                    }
                    continue;
                }

                parent.Options.RemoveWhere(s => !ruleForParents.Options.Contains(s));

                if (parent.Options.Count == 0)
                {
                    //// continue while loop (with other options)
                    //Console.WriteLine("Parent cant be filled anymore");
                    //Console.WriteLine($"Parent {parent.Id}, Child {Id}");
                    return false;
                }
            }
            return true;
        }
        
        public bool IsSet()
        {
            return isSet;
        }
        public void AssignValue(int value)
        {
            if (IsSet()) throw new ArgumentException("Can't set value, value is already set.", "value");

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
