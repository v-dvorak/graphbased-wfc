using GBWFC.Solver;

namespace GBWFC.Graph
{
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
