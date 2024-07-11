using System.Text.Json;

namespace wfc
{
    public record struct Rule(int Item, int[] Options)
    {
        public int Item { get; } = Item;
        public int[] Options { get; } = Options;

        /// <summary>
        /// Returns a list of inverse rules.
        /// </summary>
        /// <example>
        /// <code>
        /// Rule(0, [0, 1, 2])
        /// </code>
        ///  has inverse rules: 
        /// <code>
        /// [ Rule(0, [0]), Rule(1, [0]), Rule(2, [0]) ] 
        /// </code>
        /// </example>
        /// <returns>An array of inverse rules.</returns>
        public Rule[] GetInverseRules()
        {
            Rule[] output = new Rule[Options.Length];
            for (int i = 0; i < Options.Length; i++)
            {
                output[i] = new Rule(Options[i], [Item]);
            }
            return output;
        }
        public override readonly string ToString()
        {
            string opt = "";
            foreach (int k in Options)
            {
                opt += k.ToString();
                opt += " ";
            }
            return $"Rule {{ Item = {Item}, Options = {opt}}}";
        }
    }
    public class Rulebook
    {
        private readonly Rule[] rulesForChildren;
        private readonly Rule[] rulesForParents;
        public int RuleCount { get { return rulesForChildren.Length; } }
        public Rulebook(Rule[] rules)
        {
            rulesForChildren = rules;
            rulesForParents = GetInverseRules(rules);
        }
        public Rule GetRuleForChildren(int parent)
        {
            return rulesForChildren[parent];
        }
        public Rule GetRuleForParents(int child)
        {
            return rulesForParents[child];
        }
        public static Rule[] CreateColoringRules(int colorCount)
        {
            Rule[] output = new Rule[colorCount];
            for (int i = 0; i < colorCount; i++)
            {
                int[] options = new int[colorCount - 1];
                int j = 0;
                int k = 0;
                while (j < colorCount)
                {
                    if (j != i)
                    {
                        options[k] = j;
                        k++;
                    }
                    j++;
                }
                output[i] = new Rule(i, options);
            }
            return output;
        }
        public static Rule[] CreateCascadeRules(int colorCount, bool overlap = false)
        {
            if (colorCount == 1)
            {
                return [new Rule(0, [0])];
            }
            else if (colorCount == 2)
            {
                return [new Rule(0, [0, 1]), new Rule(1, [0, 1])];
            }
            Rule[] output = new Rule[colorCount];
            output[0] = new Rule(0, [0, 1]);
            for (int i = 1; i < colorCount - 1; i++)
            {
                output[i] = new Rule(i, [i - 1, i, i + 1]);
            }
            output[colorCount - 1] = new Rule(colorCount - 1, [colorCount - 2, colorCount - 1]);
            return output;
        }
        /// <summary>
        /// Creates an array of inverse rules to apply in the opposite direction than the rules given as param.
        /// </summary>
        /// <param name="rules">Rules to invert.</param>
        /// <returns>An array of <see cref="Rule"/></returns>
        public static Rule[] GetInverseRules(Rule[] rules)
        {
            HashSet<int>[] tempInverse = new HashSet<int>[rules.Length];
            for (int i = 0; i < rules.Length; i++)
            {
                tempInverse[i] = new HashSet<int>();
            }

            foreach (Rule rule in rules)
            {
                Rule[] inverse = rule.GetInverseRules();
                foreach (Rule inv in inverse)
                {
                    foreach (int option in inv.Options)
                    {
                        tempInverse[inv.Item].Add(option);
                    }
                }
            }

            Rule[] inverseRules = new Rule[rules.Length];
            for (int i = 0; i < rules.Length; i++)
            {
                int[] rl = new int[tempInverse[i].Count];
                tempInverse[i].CopyTo(rl);
                inverseRules[i] = new Rule(i, rl);
            }
            return inverseRules;
        }
    }
    public static class RuleParser
    {
        /// <summary>
        /// Parses a list of rules from a JSON file and converts them to <see cref="Rule"/>s.
        /// </summary>
        /// <param name="path">The path to the JSON file containing the rules.</param>
        /// <returns>A list of <see cref="Rule"/> structs parsed from the JSON file and converted to numerical representation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is null or empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist.</exception>
        /// <exception cref="IOException">Thrown when an I/O error occurs while opening or reading the file.</exception>
        /// <exception cref="JsonException">Thrown when the JSON data is invalid or cannot be deserialized.</exception>
        /// <remarks>
        /// This method reads a JSON file specified by <paramref name="path"/>, parses the content into a list of <see cref="JSONRule"/> objects,
        /// and then converts each <see cref="JSONRule"/> to a <see cref="Rule"/> struct.
        /// </remarks>
        public static List<Rule> RulesFromJSON(string path)
        {
            List<JSONRule> rules = ParseFromJSON(path);
            List<Rule> output = new(rules.Count);
            foreach (JSONRule rule in rules)
            {
                (int item, int[] options) = rule.GetNumericalRepresentation();
                output.Add(new Rule(item, options));
            }
            return output;
        }
        private static List<JSONRule> ParseFromJSON(string path)
        {
            string json;
            using (StreamReader r = new StreamReader(path))
            {
                json = r.ReadToEnd();
            }
            var dataItems = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
            // convert rule classes
            List<JSONRule> items = new();
            foreach (var item in dataItems)
            {
                items.Add(new JSONRule { Item = item.Key, Options = item.Value });
            }
            return items;
        }
        private class JSONRule
        {
            public string Item { get; set; }
            public List<string> Options { get; set; }

            public (int, int[]) GetNumericalRepresentation()
            {
                int item = int.Parse(Item);
                int[] options = new int[Options.Count];
                for (int i = 0; i < Options.Count; i++)
                {
                    options[i] = int.Parse(Options[i]);
                }
                return (item, options);
            }
        }
    }
}