using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using POESKillTree.Model.Items;

namespace UpdateEquipment
{
    /// <summary>
    /// Extracts affix information from exilemods.com as a <see cref="XmlAffixList"/>.
    /// </summary>
    public class AffixDataLoader : DataLoader<XmlAffixList>
    {
        private const string Url = "http://www.exilemods.com/js/data.js";

        private const string Root = "htmlObject";

        private static readonly Regex AffixNameLineRegex = new Regex(@"<tr><td colspan='3'>\((.+?)\) (.*?)</td></tr>");

        private static readonly Regex IncorrectFromToRegex = new Regex(@"(\d+), (\d+), .+");

        private static readonly Regex MasterCraftedRegex = new Regex(@" lvl: \d+");

        private const string IncorrectFromToRename = "$1 to $2";

        public override void Load()
        {
            string file;
            using (var client = new WebClient())
            {
                file = client.DownloadString(Url);
            }
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
                    var affix = new XmlAffix
                    {
                        ModType = modType,
                        Global = IsGlobal(nameLine),
                        ItemType = itemType,
                        Name = affixName
                    };

                    var tierRows = lines[++i].Replace("</table>", "").Replace("<tr>", "")
                        .Split(new [] {"</tr>"}, StringSplitOptions.RemoveEmptyEntries);
                    var tierList = new List<XmlTier>();
                    var currentTier = 1;
                    foreach (var tierRow in tierRows.Reverse())
                    {
                        var columns = tierRow.Replace("<td>", "").Split(new[] { "</td>" }, StringSplitOptions.None);
                        tierList.Add(new XmlTier
                        {
                            ItemLevel = ParseInt(columns[0]),
                            Stats = ExtractStats(columns[1], affixName).ToArray(),
                            Name = columns[2],
                            IsMasterCrafted = MasterCraftedRegex.IsMatch(columns[2]),
                            Tier = columns[2].Contains(" lvl: ") ? 0 : currentTier++
                        });
                    }
                    affix.Tiers = Enumerable.Reverse(tierList).ToArray();

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

        private static IEnumerable<XmlStat> ExtractStats(string statColumn, string affixName)
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
            foreach (var tuple in statColumn.Split(new []{" / "}, StringSplitOptions.None).Zip(affixesSplit, Tuple.Create))
            {
                var stat = tuple.Item1;
                if (IncorrectFromToRegex.IsMatch(stat))
                    stat = IncorrectFromToRegex.Replace(stat, IncorrectFromToRename);
                var fromTo = stat.Split(new[] {" to "}, StringSplitOptions.None);
                yield return new XmlStat
                {
                    Name = tuple.Item2,
                    From = ParseFloat(fromTo[0]),
                    To = fromTo.Length > 1 ? ParseFloat(fromTo[1]) : ParseFloat(fromTo[0])
                };
            }
        }
    }
}