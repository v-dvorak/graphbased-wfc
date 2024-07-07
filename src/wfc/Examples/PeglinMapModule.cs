namespace wfc.Examples
{
    public static class PeglinMapModule
    {
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
        public static List<(int, int)> GetEdges(IReadOnlyList<int[]> map)
        {
            List<(int, int)> edges = new();
            for (int i = 0; i < map.Count - 1; i++)
            {
                // top
                if (i + 1 < map.Count && map[i].Length < map[i + 1].Length)
                {
                    for (int j = 0; j < map[i].Length; j++)
                    {
                        edges.Add((map[i][j], map[i + 1][j]));
                        edges.Add((map[i][j], map[i + 1][j + 1]));
                    }
                }
                // middle
                else if (i + 2 < map.Count && map[i].Length == map[i + 2].Length)
                {
                    edges.Add((map[i][0], map[i + 2][0]));
                    edges.Add((map[i][0], map[i + 1][0]));
                    for (int j = 0; j < map[i + 1].Length; j++)
                    {
                        edges.Add((map[i][j], map[i + 1][j]));
                        edges.Add((map[i][j + 1], map[i + 1][j]));
                    }
                    edges.Add((map[i][^1], map[i + 1][^1]));
                    edges.Add((map[i][^1], map[i + 2][^1]));
                }
                // bottom
                else
                {
                    for (int j = 0; j < map[i + 1].Length; j++)
                    {
                        edges.Add((map[i][j], map[i + 1][j]));
                        edges.Add((map[i][j + 1], map[i + 1][j]));
                    }
                }
            }
            return edges;
        }
    }
}
