namespace GBWFC
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
        public static void Print<T>(this T[,] grid)
        {
            int height = grid.GetLength(0);
            int width = grid.GetLength(1);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Console.Write(grid[i, j]);
                    Console.Write(" ");
                }
                Console.WriteLine();
            }
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
