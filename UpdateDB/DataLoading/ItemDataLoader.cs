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

        /// <summary>
        /// Contains all item types that have hidden implicits that are not listed in the wiki tables.
        /// </summary>
        private static readonly IReadOnlyDictionary<ItemType, XmlStat> HiddenImplicits;

        private static readonly IEnumerable<XmlItemBase> Jewels =
            ItemGroup.Jewel.Types().Select(t => new XmlItemBase { ItemType = t, Name = t.ToString().Replace("Jewel", " Jewel") }).ToList();

        static ItemDataLoader()
        {
            var penalty3 = new XmlStat
            {
                Name = "#% reduced Movement Speed",
                From = 3,
                To = 3
            };
            var penalty4 = new XmlStat
            {
                Name = "#% reduced Movement Speed",
                From = 4,
                To = 4
            };
            var penalty8 = new XmlStat
            {
                Name = "#% reduced Movement Speed",
                From = 8,
                To = 8
            };
            var dict = new Dictionary<ItemType, XmlStat>();
            foreach (var itemType in ItemGroup.BodyArmour.Types())
            {
                dict[itemType] = penalty4;
            }
            dict[ItemType.BodyArmourArmour] = penalty8;
            dict[ItemType.BodyArmourArmourEnergyShield] = penalty8;
            foreach (var itemType in ItemGroup.Shield.Types())
            {
                dict[itemType] = penalty3;
            }
            HiddenImplicits = dict;
        }

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
                if (HiddenImplicits.ContainsKey(itemType))
                {
                    implicits.Add(HiddenImplicits[itemType]);
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
            return match.Success && TryParseInt(match.Groups[1].Value, out i);
        }

        private static IEnumerable<XmlStat> ParseImplicit(WikiItemStat wikiItemStat)
        {
            var mod = wikiItemStat.StatsCombined;
            var matches = NumberRegex.Matches(mod);
            if (matches.Count <= 0) yield break;

            mod = NumberRegex.Replace(mod, "#").Replace("–", "-").Replace("#-#", "# to #");
            if (mod.Contains("# to # "))
            {
                if (matches.Count != 2)
                {
                    Log.Warn("Could not parse implicit " + wikiItemStat.StatsCombined);
                    yield break;
                }
                var from = ParseFloat(matches[0].Value);
                yield return new XmlStat
                {
                    From = from,
                    To = from,
                    Name = mod.Replace("#-#", "# minimum")
                };
                from = ParseFloat(matches[1].Value);
                yield return new XmlStat
                {
                    From = from,
                    To = from,
                    Name = mod.Replace("#-#", "# maximum")
                };
            }
            else
            {
                var from = ParseFloat(matches[0].Value);
                yield return new XmlStat
                {
                    From = from,
                    To = matches.Count > 1 ? ParseFloat(matches[1].Value) : from,
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
                        success = TryParseFloat(matches[0].Value, out from);
                        to = from;
                        break;
                    case 2:
                        success = TryParseFloat(matches[0].Value, out from) &
                                  TryParseFloat(matches[1].Value, out to);
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