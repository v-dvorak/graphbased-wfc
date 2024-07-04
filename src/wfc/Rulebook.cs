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
        private readonly WeightedRandomSelector wrs;
        public Rulebook(Rule[] rules)
        {
            rulesForChildren = rules;
            rulesForParents = GetInverseRules(rules);
            wrs = new WeightedRandomSelector();
        }
        public Rule GetRuleForChildren(int parent)
        {
            return rulesForChildren[parent];
        }
        public Rule GetRuleForParents(int child)
        {
            return rulesForParents[child];
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