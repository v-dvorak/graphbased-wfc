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
        public static double Shannon(int[] globalWeights, HashSet<int> constraints)
        {
            List<double> rel = ConvertWeights(globalWeights, constraints);
            double entropy = Shannon(rel);
            return entropy;
        }
        public static List<double> ConvertWeights(int[] globalWeights, HashSet<int> constraints)
        {
            if (globalWeights.Length == 0)
            {
                return new List<double>();
            }
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
