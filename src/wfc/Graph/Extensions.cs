namespace GBWFC.Graph
{
    public static class GraphExtensions
    {
        public static Edge Edge(this (int, int) edge) => new Edge(edge);
    }
}
