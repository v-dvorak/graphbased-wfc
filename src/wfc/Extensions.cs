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
    }
    public static class GraphExtensions
    {
        public static Graph Copy(this Graph graph) => graph.Copy();
        public static Edge Edge(this (int, int) edge) => new Edge(edge);
        public static ConstraintById ConstraintById(this (int, int) constraint) => new ConstraintById(constraint);
        public static ConstraintByNode ConstraintByNode(this (Node, int) constraint) => new ConstraintByNode(constraint);
    }
    public static class MathExtensions
    {
        public static List<double> MultiplyBy(this List<double> list1, List<double> list2)
        {
            // multiplies first list by values from second list
            if (list1.Count != list2.Count)
            {
                throw new ArgumentException("Lists must have the same length.");
            }

            for (int i = 0; i < list1.Count; i++)
            {
                list1[i] = list1[i] * list2[i];
            }

            return list1;
        }
        public static List<double> Log(this List<double> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values), "Input list cannot be null.");
            }
            List<double> output = new(values.Count);
            for (int i = 0; i < values.Count; i++)
            {
                output.Add(Math.Log(values[i]));
            }
            return output;
        }
        public static int Choose(this List<int> values, bool[] constraints)
        {
            if (values.Count != constraints.Length)
            {
                throw new ArgumentException("Lists must be the same length.");
            }
            int total = 0;
            for (int i = 0; i < values.Count; i++)
            {
                if (constraints[i]) total += values[i];
            }
            return total;
        }
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
