namespace wfc
{
    public static class Entropy
    {
        public static double Shannon(List<double> weights)
        {
            double entropy = 0;
            for (int i = 0; i < weights.Count; i++)
            {
                if (weights[i] != 0)
                {
                    entropy += weights[i] * Math.Log(weights[i]);
                }
            }
            return -entropy;
        }
        public static double Shannon(HashSet<int> constraints, int[] globalWeights)
        {
            List<double> rel = ConvertWeights(globalWeights, constraints);
            double entropy = Shannon(rel);
            return entropy;
        }
        public static List<double> ConvertWeights(int[] globalWeights, HashSet<int> constraints)
        {
            double total = 0;
            foreach (int k in constraints)
            {
                total += globalWeights[k];
            }
            List<double> output = new(constraints.Count);
            foreach (int k in constraints)
            {
                output.Add(globalWeights[k] / total);
            }
            return output;
        }
    }
}
