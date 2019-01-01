using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using PoESkillTree.GameModel.Items;

namespace PoESkillTree.Computation.Console
{
    internal static class TestDataUpdater
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

            var statLinesPath = "../../../PoESkillTree.GameModel/Data/SkillTreeStatLines.txt";
            File.WriteAllLines(statLinesPath, statLines);
        }

        public static void UpdateParseableBaseItems(BaseItemDefinitions baseItemDefinitions)
        {
            var seenImplicits = new HashSet<string>();
            var seenBuffs = new HashSet<string>();
            var baseIds = baseItemDefinitions.BaseItems
                .Where(d => d.ReleaseState != ReleaseState.Unreleased)
                .Where(d => d.ImplicitModifiers.Any(s => seenImplicits.Add(s.StatId))
                            || d.BuffStats.Any(s => seenBuffs.Add(s.StatId)))
                .Select(d => d.MetadataId);

            var parseablePath = "../../../PoESkillTree.Computation.IntegrationTests/Data/ParseableBaseItems.txt";
            File.WriteAllLines(parseablePath, baseIds);
        }
    }
}