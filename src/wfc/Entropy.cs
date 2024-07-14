namespace GBWFC.Entropy
{
    /// <summary>
    /// Represents a method that evaluates a node based on its options and global weights.
    /// </summary>
    /// <param name="nodeOptions">A representation the options for the node.</param>
    /// <param name="globalWeights">A read-only list of <see cref="int"/> representing the global weights to be used in the evaluation.</param>
    /// <returns>
    /// A <see cref="double"/> representing the computed value of the node based on the provided options and global weights.
    /// </returns>
    public delegate double EvaluateNode(IEnumerable<int> nodeOptions, IReadOnlyList<int> globalWeights);

    /// <summary>
    /// Contains all implemented methods for node entropy evaluation and some helper methods.
    /// </summary>
    public static class WFCEntropy
    {
        /// <summary>
        /// Returns the Shannon Entropy based on given weights.
        /// Implemented according to the Wikipedia article: https://en.wikipedia.org/wiki/Entropy_(information_theory)
        /// </summary>
        /// <param name="weights">Weights of possible node values.</param>
        /// <returns>
        /// A <see cref="double"/> representing the computed value of the node based on the provided normalized weights.
        /// </returns>
        public static double Shannon(IReadOnlyList<double> weights)
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
        /// <summary>
        /// Returns the Shannon Entropy based on given global weights and weights that may still be assigned.
        /// </summary>
        /// <param name="constraints">Set of values that may still be assigned.</param>
        /// <param name="globalWeights">Global weights.</param>
        /// <returns>
        /// A <see cref="double"/> representing the computed entropy of the node based on the provided options and global weights.
        /// </returns>
        public static double Shannon(IEnumerable<int> constraints, IReadOnlyList<int> globalWeights)
        {
            List<double> rel = ConvertWeights(constraints, globalWeights);
            double entropy = Shannon(rel);
            return entropy;
        }
        /// <summary>
        /// Given a list of type frequencies returns a list of normalized probabilities.
        /// </summary>
        /// <param name="globalWeights"></param>
        /// <param name="constraints"></param>
        /// <returns></returns>
        public static List<double> ConvertWeights(IEnumerable<int> constraints, IReadOnlyList<int> globalWeights)
        {
            double total = 0;
            int conCount = 0;
            foreach (int k in constraints)
            {
                total += globalWeights[k];
                conCount++;
            }
            List<double> output = new(conCount);
            foreach (int k in constraints)
            {
                output.Add(globalWeights[k] / total);
            }
            return output;
        }
    }
}
