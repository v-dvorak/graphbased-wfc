namespace wfc
{
    public class WeightedRandomSelector
    {
        private readonly Random _random;

        public WeightedRandomSelector()
        {
            _random = new Random();
        }

        public T Choose<T>(T[] items, float[] cumulativeWeights, float totalWeight)
        {
            float randomValue = (float)_random.NextDouble() * totalWeight;

            // find the index corresponding to the random value
            for (int i = 0; i < cumulativeWeights.Length; i++)
            {
                if (randomValue < cumulativeWeights[i])
                {
                    return items[i];
                }
            }

            // edge case (should never happen if weights are positive)
            throw new IndexOutOfRangeException("Options and Weights arrays must have the same length");
        }
        public T Choose<T>(List<T> items, List<int> globalWeights)
        {
            if (items.Count != globalWeights.Count)
            {
                throw new ArgumentException("Items and weights must have the same length.");
            }

            int chosenIndex = Choose(globalWeights);
            return items[chosenIndex];
        }
        public int Choose(List<int> globalWeights)
        {
            // Calculate the cumulative weights
            float[] cumulativeWeights = new float[globalWeights.Count];
            cumulativeWeights[0] = globalWeights[0];
            for (int i = 1; i < globalWeights.Count; i++)
            {
                cumulativeWeights[i] = cumulativeWeights[i - 1] + globalWeights[i];
            }

            // Generate a random number in the range [0, total weight)
            float totalWeight = cumulativeWeights[cumulativeWeights.Length - 1];
            float randomValue = (float)_random.NextDouble() * totalWeight;

            // Find the index corresponding to the random value
            for (int i = 0; i < cumulativeWeights.Length; i++)
            {
                if (randomValue < cumulativeWeights[i])
                {
                    return i;
                }
            }

            // Fallback (should never happen if weights are positive)
            throw new InvalidOperationException("Should never reach here if weights are positive.");
        }
    }
}