namespace GBWFC.Graph
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
}
