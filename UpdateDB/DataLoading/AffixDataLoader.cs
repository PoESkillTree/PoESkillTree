using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using POESKillTree.Model.Items.Affixes;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Utils.Extensions;

namespace UpdateDB.DataLoading
{
	using CSharpGlobalCode.GlobalCode_ExperimentalCode;
	/// <summary>
	/// Extracts affix information from exilemods.com as a <see cref="XmlAffixList"/>.
	/// </summary>
	public class AffixDataLoader : XmlDataLoader<XmlAffixList>
    {
        private const string Url = "http://www.exilemods.com/js/data.js";

        private const string Root = "htmlObject";

        private static readonly Regex AffixNameLineRegex = new Regex(@"<tr><td colspan='3'>\((.+?)\) (.*?)</td></tr>");

        private static readonly Regex MasterCraftedRegex = new Regex(@" lvl: \d+");

        private static readonly Tuple<Regex, string>[] GenericRangeChanges =
        {
            // Second value of "(Global) #% increased Freeze Duration on Enemies, #% chance to Freeze" for jewels is weird
            Tuple.Create(new Regex(@"(\d+), (\d+), .+"), "$1 to $2")
        };

        private delegate string ChangeRange(string affix, string tier, string range);

        private static readonly IReadOnlyDictionary<ItemType, ChangeRange> ItemTypeSpecificRangeChanges =
            new Dictionary<ItemType, ChangeRange>
            {
                // Amulet critical chance was nerfed
                {
                    ItemType.Amulet, (affix, tier, range) =>
                    {
                        if (affix == "#% increased Global Critical Strike Chance" && tier == "Elreon lvl: 6")
                            return "22 to 27";
                        return range;
                    }
                },
                // Jewel critical multiplier was adjusted when critical multiplier was renamed
                {ItemType.CobaltJewel, JewelCritMultiChange},
                {ItemType.CrimsonJewel, JewelCritMultiChange},
                {ItemType.ViridianJewel, JewelCritMultiChange}
            };

        private static string JewelCritMultiChange(string affix, string tier, string range)
        {
            if (!affix.Contains("Critical Strike Multiplier"))
                return range;
            switch (range)
            {
                case "6 to 8":
                    return "9 to 12";
                case "8 to 10":
                    return "15 to 18";
                case "10 to 12":
                    return "12 to 15";
                default:
                    return range;
            }
        }

        private static readonly Tuple<Regex, string>[] GenericNameChanges =
        {
            // Critical Strike Multiplier was renamed
            Tuple.Create(new Regex("#% increased Critical Strike Multiplier"), "+#% to Critical Strike Multiplier"),
            Tuple.Create(new Regex(@"#% increased (\w+) Critical Strike Multiplier"), "+#% to $1 Critical Strike Multiplier")
        };

        protected override async Task LoadAsync()
        {
            var file = await HttpClient.GetStringAsync(Url);
            file = file.Replace(Root + " = ", "{ \"" + Root + "\": ") + "}";

            var json = JObject.Parse(file);
            var types = from t in json["htmlObject"]
                        let p = (JProperty) t
                        where !p.Name.Contains("map") && !p.Name.Contains("flask") && !p.Name.Contains("fishing")
                        select new { Type = p.Name, Content = Trim(p.Value.ToString()) };
            
            // <tr><td colspan='3'>...</td><tr> -> mod started, next line describes the tiers
            // </table>... -> Prefixes end, Suffixes start on next line
            
            var affixes = new List<XmlAffix>();
            foreach (var type in types)
            {
                // We don't care about groups, they only contain the sub groups.
                if (type.Content.Contains("<a id="))
                    continue;

                ItemType itemType;
                if (!TryParseItemType(type.Type, out itemType))
                {
                    Console.WriteLine("Could not parse item type {0}, ignoring it.", type.Type);
                    continue;
                }

                var modType = ModType.Prefix;

                var lines = type.Content.Split('\n');
                for (var i = 0; i < lines.Length - 1; i++)
                {
                    if (lines[i].StartsWith("</table>"))
                    {
                        if (modType == ModType.Prefix)
                            modType = ModType.Suffix;
                        else
                            break;
                    }
                    if (!lines[i].StartsWith("<tr><td colspan='3'>"))
                        continue;

                    var nameLine = lines[i];
                    var affixName = ExtractAffixName(nameLine);
                    foreach (var nameChange in GenericNameChanges)
                    {
                        affixName = nameChange.Item1.Replace(affixName, nameChange.Item2);
                    }
                    var affix = new XmlAffix
                    {
                        ModType = modType,
                        Global = IsGlobal(nameLine),
                        ItemType = itemType
                    };

                    ChangeRange itemTypeSpecificRangeChange;
                    ItemTypeSpecificRangeChanges.TryGetValue(itemType, out itemTypeSpecificRangeChange);
                    Func<string, string, string> rangeRenameFunc = (_, range) => range;

                    var tierRows = lines[++i].Replace("</table>", "").Replace("<tr>", "")
                        .Split(new [] {"</tr>"}, StringSplitOptions.RemoveEmptyEntries);
                    var tierList = new List<XmlTier>();
                    var currentTier = 1;
                    foreach (var tierRow in tierRows.Reverse())
                    {
                        var columns = tierRow.Replace("<td>", "").Split(new[] { "</td>" }, StringSplitOptions.None);
                        var tierName = columns[2];
                        if (itemTypeSpecificRangeChange != null)
                        {
                            rangeRenameFunc = (aff, range) => itemTypeSpecificRangeChange(aff, tierName, range);
                        }
                        tierList.Add(new XmlTier
                        {
                            ItemLevel = columns[0].ParseInt(),
                            Stats = ExtractStats(columns[1], affixName, rangeRenameFunc).ToArray(),
                            Name = tierName,
                            IsMasterCrafted = MasterCraftedRegex.IsMatch(columns[2]),
                            Tier = tierName.Contains(" lvl: ") ? 0 : currentTier++
                        });
                    }
                    affix.Tiers = Enumerable.Reverse(tierList).ToArray();
                    affix.Name = string.Join(", ", affix.Tiers[0].Stats.Select(s => s.Name));

                    affixes.Add(affix);
                }
            }

            Data = new XmlAffixList
            {
                Affixes = affixes.ToArray()
            };
        }

        private static string Trim(string html)
        {
            return Regex.Replace(Regex.Replace(html
                .Replace("\t", "")
                .Replace("\\\"", "\"")
                .Replace("\"", "'"),
                @" class='[ _0-9a-zA-Z]+'", ""),
                @" +", " ")
                .Replace("> ", ">")
                .Replace(" <", "<")
                .Replace("<div>", "")
                .Replace("</div>", "");
        }

        private static bool TryParseItemType(string jsonType, out ItemType itemType)
        {
            var replaced = Regex.Replace(jsonType, "s$", "")
                .Replace("s_", "_")
                .Replace("1h", "OneHanded")
                .Replace("2h", "TwoHanded")
                .Replace("_and_", "_")
                .Replace("_", "")
                .Replace("stave", "staff")
                .Replace("boot", "boots")
                .Replace("glove", "gloves");
            return Enum.TryParse(replaced, true, out itemType);
        }

        private static bool IsGlobal(string line)
        {
            return AffixNameLineRegex.Match(line).Groups[1].Value.Equals("Global");
        }

        private static string ExtractAffixName(string line)
        {
            return AffixNameLineRegex.Match(line).Groups[2].Value;
        }

        private static IEnumerable<XmlStat> ExtractStats(string statColumn, string affixName, Func<string, string, string> rangeRenameFunc)
        {
            var affixesSplit = new List<string>();
            foreach (var split in Regex.Split(affixName, @"(?<=.*#.*), (?=.*#.*)"))
            {
                if (split.Contains("#") || !affixesSplit.Any())
                {
                    affixesSplit.Add(split);
                }
                else
                {
                    affixesSplit[affixesSplit.Count - 1] += ", " + split;
                }
            }

            var xmlStats = new List<XmlStat>();
            foreach (var tuple in statColumn.Split(new []{" / "}, StringSplitOptions.None).Zip(affixesSplit, Tuple.Create))
            {
                string stat = tuple.Item1;
                foreach (var rangeChange in GenericRangeChanges)
                {
                    stat = rangeChange.Item1.Replace(stat, rangeChange.Item2);
                }
                string affix = tuple.Item2;
                foreach (var nameChange in GenericNameChanges)
                {
                    affix = nameChange.Item1.Replace(affix, nameChange.Item2);
                }
                stat = rangeRenameFunc(affix, stat);
                var fromTo = stat.Split(new[] {" to "}, StringSplitOptions.None);
#if (PoESkillTree_UseSmallDec_ForAttributes)
				SmallDec from = fromTo[0];
				SmallDec to = fromTo.Length > 1 ? fromTo[1] : from;
#else
                float from = fromTo[0].ParseFloat();
                float to = fromTo.Length > 1 ? fromTo[1].ParseFloat() : from;
#endif
				xmlStats.Add(new XmlStat
                {
                    Name = affix,
                    From = new[] { from },
                    To = new[] { to }
                });
            }

            // merge "... # minimum ..., ... # maximum" into "... # to # ..."
            if (xmlStats.Count > 1)
            {
                var previous = xmlStats[0];
                for (int i = 1; i < xmlStats.Count; i++)
                {
                    var current = xmlStats[i];

                    string prevReplaced = previous.Name.Replace(" minimum", "");
                    string curReplaced = current.Name.Replace(" maximum", "");
                    if (prevReplaced == curReplaced)
                    {
                        previous.Name = previous.Name.Replace("# minimum", "# to #");
                        previous.From = previous.From.Concat(current.From).ToList();
                        previous.To = previous.To.Concat(current.To).ToList();
                        xmlStats.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        previous = xmlStats[i];
                    }
                }
            }

            return xmlStats;
        }
    }
}