using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using log4net;
using POESKillTree.Model.Items;
using POESKillTree.Model.Items.Affixes;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Utils;
using POESKillTree.Utils.Extensions;

namespace UpdateDB.DataLoading
{
    /// <summary>
    /// Extracts item bases from the wiki as a <see cref="XmlItemList"/>.
    /// </summary>
    public class ItemDataLoader : XmlDataLoader<XmlItemList>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ItemDataLoader));

        private static readonly Regex NumberRegex = new Regex(@"\d+(\.\d+)?");

        private static readonly Regex LevelRegex = new Regex(@"Level (\d+)");
        private static readonly Regex DexRegex = new Regex(@"(\d+) Dex");
        private static readonly Regex StrRegex = new Regex(@"(\d+) Str");
        private static readonly Regex IntRegex = new Regex(@"(\d+) Int");

        private static readonly IReadOnlyDictionary<string, string> PropertyRenames = new Dictionary<string, string>
        {
            {"Evasion", "Evasion Rating"}
        };

        private static readonly IReadOnlyDictionary<string, string> ImplicitRenames = new Dictionary<string, string>
        {
            {"#% reduced Movement Speed (Hidden)", "#% reduced Movement Speed"}
        };

        private static readonly IEnumerable<XmlItemBase> Jewels =
            ItemGroup.Jewel.Types().Select(t => new XmlItemBase { ItemType = t, Name = t.ToString().Replace("Jewel", " Jewel") }).ToList();

        protected override async Task LoadAsync(HttpClient httpClient)
        {
            var wikiUtils = new WikiUtils(httpClient);
            var bases = await wikiUtils.SelectFromBaseItemsAsync(ParseTable);
            Data = new XmlItemList
            {
                ItemBases = bases.Union(Jewels).ToArray()
            };
        }

        private static IEnumerable<XmlItemBase> ParseTable(HtmlNode table, ItemType itemType)
        {
            // Select the item box in the first cell of each (non-header) row
            foreach (var cell in table.SelectNodes("tr/td[1]//*[contains(@class, 'item-box')]"))
            {
                var itemBox = WikiUtils.ParseItemBox(cell);
                var statGroups = itemBox.StatGroups;

                var implicits = new List<XmlStat>();
                var implicitFrom = 1F;
                var implicitTo = 1F;
                if (statGroups.Length > 2)
                {
                    foreach (var wikiItemStat in statGroups[2])
                    {
                        implicits.AddRange(ParseImplicit(wikiItemStat));
                    }
                    if (implicits.Any())
                    {
                        implicitFrom += implicits[0].From / 100;
                        implicitTo += implicits[0].To / 100;
                    }
                }

                var itemBase = new XmlItemBase
                {
                    ItemType = itemType,
                    Name = itemBox.TypeLine,
                    Implicit = implicits.Any() ? implicits.ToArray() : null,
                    Properties = ParseProperties(statGroups[0], implicitFrom, implicitTo).ToArray()
                };
                ParseRequirements(statGroups[1], itemBase);
                yield return itemBase;
            }
        }

        private static void ParseRequirements(IEnumerable<WikiItemStat> requirements, XmlItemBase itemBase)
        {
            var requirementLine =
                requirements.Select(s => s.StatsCombined).FirstOrDefault(s => s.StartsWith("Requires "));
            if (requirementLine == null)
                return;

            int i;
            if (TryParseFirstMatchToInt(LevelRegex, requirementLine, out i))
                itemBase.Level = i;
            if (TryParseFirstMatchToInt(StrRegex, requirementLine, out i))
                itemBase.Strength = i;
            if (TryParseFirstMatchToInt(DexRegex, requirementLine, out i))
                itemBase.Dexterity = i;
            if (TryParseFirstMatchToInt(IntRegex, requirementLine, out i))
                itemBase.Intelligence = i;
        }

        private static bool TryParseFirstMatchToInt(Regex regex, string input, out int i)
        {
            i = 0;
            var match = regex.Match(input);
            return match.Success && match.Groups[1].Value.TryParseInt(out i);
        }

        private static IEnumerable<XmlStat> ParseImplicit(WikiItemStat wikiItemStat)
        {
            var mod = wikiItemStat.StatsCombined;
            var matches = NumberRegex.Matches(mod);
            if (matches.Count <= 0) yield break;

            mod = NumberRegex.Replace(mod, "#").Replace("–", "-").Replace("#-#", "# to #");
            if (ImplicitRenames.ContainsKey(mod))
                mod = ImplicitRenames[mod];
            if (mod.Contains("# to # "))
            {
                if (matches.Count != 2)
                {
                    Log.Warn("Could not parse implicit " + wikiItemStat.StatsCombined);
                    yield break;
                }
                var from = matches[0].Value.ParseFloat();
                yield return new XmlStat
                {
                    From = from,
                    To = from,
                    Name = mod.Replace("#-#", "# minimum")
                };
                from = matches[1].Value.ParseFloat();
                yield return new XmlStat
                {
                    From = from,
                    To = from,
                    Name = mod.Replace("#-#", "# maximum")
                };
            }
            else
            {
                var from = matches[0].Value.ParseFloat();
                yield return new XmlStat
                {
                    From = from,
                    To = matches.Count > 1 ? matches[1].Value.ParseFloat() : from,
                    Name = mod.Replace("(# to #)", "#")
                };
            }
        }

        private static IEnumerable<XmlStat> ParseProperties(IEnumerable<WikiItemStat> properties,
            float implicitFrom, float implicitTo)
        {
            foreach (var wikiItemStat in properties)
            {
                if (wikiItemStat.Stats.Length <= 1)
                    continue;

                var stats = wikiItemStat.Stats;
                var combined = wikiItemStat.StatsCombined;
                if (stats[0].Item2 != WikiStatColor.Default)
                {
                    Log.Warn($"Property {combined} begins with wrong color {stats[0].Item2}");
                    continue;
                }
                var name = wikiItemStat.Stats[0].Item1.TrimEnd(':', ' ');
                if (combined.Contains("%"))
                    name += " %";
                if (PropertyRenames.ContainsKey(name))
                    name = PropertyRenames[name];

                float from;
                float to;
                bool success;

                var modified = stats.Any(s => s.Item2 == WikiStatColor.Mod);
                var matches = NumberRegex.Matches(combined);
                switch (matches.Count)
                {
                    case 0:
                        Log.Warn($"No floats in property {combined}");
                        continue;
                    case 1:
                        success = matches[0].Value.TryParseFloat(out from);
                        to = from;
                        break;
                    case 2:
                        success = matches[0].Value.TryParseFloat(out from) &
                                  matches[1].Value.TryParseFloat(out to);
                        break;
                    default:
                        Log.Warn($"Too many floats in property {combined}");
                        continue;
                }

                if (!success)
                {
                    Log.Warn($"Could not parse floats from cell {combined}");
                    continue;
                }
                if (modified)
                {
                    from = (float) Math.Round(from / implicitFrom, 2);
                    to = (float)Math.Round(to / implicitTo, 2);
                }
                yield return new XmlStat
                {
                    From = from,
                    To = to,
                    Name = name
                };
            }
        }
    }
}