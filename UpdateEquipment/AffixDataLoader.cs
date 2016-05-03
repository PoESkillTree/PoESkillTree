using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;

namespace UpdateEquipment
{
    public class AffixDataLoader
    {
        private const string Url = "http://www.exilemods.com/js/data.js";

        private const string Root = "htmlObject";

        private static readonly Regex AffixNameLineRegex = new Regex(@"<tr><td colspan='3'>\((.+?)\) (.*?)</td></tr>");

        private static readonly Regex AddedDamageRegex = new Regex(@"Adds # minimum (.+) Damage, Adds # maximum .+ Damage");

        private const string AddedDamageRename = "Adds #-# $1 Damage";

        private static readonly Regex IncorrectFromToRegex = new Regex(@"(\d+), (\d+), .+");

        private const string IncorrectFromToRename = "$1 to $2";

        private AffixList _affixList = new AffixList();

        public void Load()
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
            
            var affixes = new List<Affix>();
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
                    var affix = new Affix
                    {
                        ModType = modType,
                        Global = IsGlobal(nameLine),
                        ItemType = itemType,
                        Name = affixName
                    };
                    if (AddedDamageRegex.IsMatch(affixName))
                    {
                        affix.CraftedAs = AddedDamageRegex.Replace(affixName, AddedDamageRename);
                    }
                    
                    var tierRows = lines[++i].Replace("</table>", "").Replace("<tr>", "")
                        .Split(new [] {"</tr>"}, StringSplitOptions.RemoveEmptyEntries);
                    var tierList = new List<AffixTier>();
                    foreach (var tierRow in tierRows)
                    {
                        var columns = tierRow.Replace("<td>", "").Split(new[] { "</td>" }, StringSplitOptions.None);
                        tierList.Add(new AffixTier
                        {
                            ItemLevel = int.Parse(columns[0]),
                            Stat = ExtractStats(columns[1]).ToArray(),
                            Name = columns[2]
                        });
                    }
                    affix.Tier = tierList.ToArray();

                    affixes.Add(affix);
                }
            }

            _affixList = new AffixList
            {
                Affix = affixes.ToArray()
            };
        }

        public void Save(string to)
        {
            using (TextWriter writer = new StreamWriter(to))
            {
                var ser = new XmlSerializer(typeof(AffixList));
                ser.Serialize(writer, _affixList);
            }
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
            var replaced = jsonType
                .Replace("1h", "OneHand")
                .Replace("2h", "TwoHand")
                .Replace("_and_", "_")
                .Replace("energy_shield", "energy")
                .Replace("body_armours", "chests")
                .Replace("helmets", "helms")
                .Replace("_", "");
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

        private static IEnumerable<AffixTierStat> ExtractStats(string statColumn)
        {
            foreach (var stat in statColumn.Split(new []{" / "}, StringSplitOptions.None))
            {
                var correct = stat;
                if (IncorrectFromToRegex.IsMatch(stat))
                    correct = IncorrectFromToRegex.Replace(stat, IncorrectFromToRename);
                var fromTo = correct.Split(new[] {" to "}, StringSplitOptions.None);
                yield return new AffixTierStat
                {
                    From = float.Parse(fromTo[0]),
                    To = fromTo.Length > 1 ? float.Parse(fromTo[1]) : float.Parse(fromTo[0])
                };
            }
        }
    }
}