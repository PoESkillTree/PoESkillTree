using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace PoESkillTree.Computation.Console
{
    internal static class StatLinesUpdater
    {
        public static void UpdateSkillTreeStatLines()
        {
            // From PoESkillTree.Computation.Console/bin/Debug/ to the same folder under WPFSKillTree
            var treePath = "../../../WPFSKillTree/bin/Debug/Data/SkillTree.txt";
            var json = JObject.Parse(File.ReadAllText(treePath));
            var nodes = json.Value<JObject>("nodesDict");
            var statLines = nodes.PropertyValues()
                .OrderBy(t => t.Value<int>("id")) // Order for more useful diffs
                .SelectMany(t => t["sd"].Values<string>())
                .Select(s => s.Replace("\n", " "));

            var statLinesPath = "../../Data/SkillTreeStatLines.txt";
            File.WriteAllLines(statLinesPath, statLines);
        }
    }
}