using GBWFC.Graph;

namespace GBWFC.Solver
{
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

}
