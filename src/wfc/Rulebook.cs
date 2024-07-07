using System.Text.Json;

namespace wfc
{
    public record struct Rule(int Item, int[] Options)
    {
        public int Item { get; } = Item;
        public int[] Options { get; } = Options;

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
        public Rulebook(Rule[] rules)
        {
            rulesForChildren = rules;
            rulesForParents = GetInverseRules(rules);
        }
        public int GetRuleCount()
        {
            return rulesForChildren.Length;
        }
        public Rule GetRuleForChildren(int parent)
        {
            return rulesForChildren[parent];
        }
        public Rule GetRuleForParents(int child)
        {
            return rulesForParents[child];
        }
        public static Rule[] GetColoringRules(int colorCount)
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
        public static Rule[] GetCascadeRules(int colorCount, bool overlap = false)
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
        public static Rule[] GetInverseRules(Rule[] rules)
        {
            // given an array of rules parent->children returns an array of rules children->parents
            HashSet<int>[] tempInverse = new HashSet<int>[rules.Length];
            for (int i = 0; i < rules.Length; i++)
            {
                tempInverse[i] = new HashSet<int>();
            }

            foreach (Rule rule in rules)
            {
                Rule[] invs = rule.GetInverseRules();
                foreach (Rule inv in invs)
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
        private class JSONData
        {
            public List<JSONRule> Items { get; set; }
        }
    }
}