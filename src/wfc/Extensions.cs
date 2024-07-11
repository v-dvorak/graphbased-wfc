namespace wfc
{
    public static class EnumExtensions
    {
        public static void Print<T>(this IEnumerable<T> list)
        {
            foreach (T elem in list)
            {
                Console.Write(elem);
                Console.Write(" ");
            }
            Console.WriteLine();
        }
        /// <summary>
        /// Fills an array with ones.
        /// </summary>
        /// <param name="list">Array to fill.</param>
        /// <returns>An filled with ones.</returns>
        public static int[] Ones(this int[] list)
        {
            for (int i = 0; i < list.Length; i++)
            {
                list[i] = 1;
            }
            return list;
        }
    }
    public static class GraphExtensions
    {
        public static Graph Copy(this Graph graph) => graph.Copy();
        public static Edge Edge(this (int, int) edge) => new Edge(edge);
        public static ConstraintById ConstraintById(this (int, int) constraint) => new ConstraintById(constraint);
        public static ConstraintByNode ConstraintByNode(this (Node, int) constraint) => new ConstraintByNode(constraint);
    }
}

namespace System
{
    public static class ObjectExtensions
    {
        public static HashSet<T> Copy<T>(this HashSet<T> original)
        {
            return new(original);
        }
    }
}
