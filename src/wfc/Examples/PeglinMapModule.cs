namespace wfc.Examples
{
    public static class PeglinMapModule
    {
        /// <summary>
        /// Converts Map to <c>Graph</c>.
        /// </summary>
        /// <param name="map">List of map levels.</param>
        /// <param name="totalOptions">Number of cell types.</param>
        /// <returns></returns>
        public static Graph MapToGraph(IReadOnlyList<int[]> map, int totalOptions)
        {
            return new Graph(GetEdges(map), totalOptions);
        }
        /// <summary>
        /// Given height and width generates Peglin Map.
        /// </summary>
        /// <param name="maxWidth">There will no more than <c>maxWidth</c> cells on one level.</param>
        /// <param name="height">Height of maps middle part.</param>
        /// <returns></returns>
        public static List<int[]> GenerateMap(int maxWidth, int height)
        {
            List<int[]> map = new();
            int lastIndex = 0;
            map.Add([lastIndex++]);
            // top
            for (int i = 1; i < maxWidth; i++)
            {
                int[] newLevel = new int[i + 1];
                for (int j = 0; j < newLevel.Length; j++)
                {
                    newLevel[j] = lastIndex++;
                }
                map.Add(newLevel);
            }
            // middle
            for (int i = 0; i < height; i++)
            {
                int[] newLevel;
                if (map[^1].Length == maxWidth)
                {
                    newLevel = new int[maxWidth - 1];
                }
                else
                {
                    newLevel = new int[maxWidth];
                }
                for (int j = 0; j < newLevel.Length; j++)
                {
                    newLevel[j] = lastIndex++;
                }
                map.Add(newLevel);
            }
            // bottom
            int currentWidth = map[^1].Length - 1;
            while (currentWidth != 0)
            {
                int[] newLevel = new int[currentWidth];
                for (int i = 0; i < currentWidth; i++)
                {
                    newLevel[i] = lastIndex++;
                }
                map.Add(newLevel);
                currentWidth--;
            }
            return map;
        }
        /// <summary>
        /// Returns list of edges/relations between cells inside a Peglin Map.
        /// </summary>
        /// <param name="map">Peglin Map representation.</param>
        /// <returns>List of edges.</returns>
        public static List<Edge> GetEdges(IReadOnlyList<int[]> map)
        {
            List<Edge> edges = new();
            for (int i = 0; i < map.Count - 1; i++)
            {
                // top
                if (i + 1 < map.Count && map[i].Length < map[i + 1].Length)
                {
                    for (int j = 0; j < map[i].Length; j++)
                    {
                        edges.Add((map[i][j], map[i + 1][j]).Edge());
                        edges.Add((map[i][j], map[i + 1][j + 1]).Edge());
                    }
                }
                // middle
                else if (i + 2 < map.Count && map[i].Length == map[i + 2].Length)
                {
                    edges.Add((map[i][0], map[i + 2][0]).Edge());
                    edges.Add((map[i][0], map[i + 1][0]).Edge());
                    for (int j = 0; j < map[i + 1].Length; j++)
                    {
                        edges.Add((map[i][j], map[i + 1][j]).Edge());
                        edges.Add((map[i][j + 1], map[i + 1][j]).Edge());
                    }
                    edges.Add((map[i][^1], map[i + 1][^1]).Edge());
                    edges.Add((map[i][^1], map[i + 2][^1]).Edge());
                }
                // bottom
                else
                {
                    for (int j = 0; j < map[i + 1].Length; j++)
                    {
                        edges.Add((map[i][j], map[i + 1][j]).Edge());
                        edges.Add((map[i][j + 1], map[i + 1][j]).Edge());
                    }
                }
            }
            return edges;
        }
    }
}
