using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace UpdateEquipment
{
    public class ItemDataLoader : DataLoader<XmlItemList>
    {
        private static readonly HashSet<string> IgnoredColumns = new HashSet<string> { "Damage per Second" };

        private static readonly Regex NumberRegex = new Regex(@"\d+(\.\d+)?");

        private const string WikiUrlPrefix = "http://pathofexile.gamepedia.com/";

        private static readonly IReadOnlyDictionary<string, IReadOnlyList<ItemType>> WikiUrls = new Dictionary<string, IReadOnlyList<ItemType>>
        {
            {"List_of_axes", new []{ItemType.OneHandAxes, ItemType.TwoHandAxes}},
            {"List_of_bows", new []{ItemType.Bows}},
            {"List_of_claws", new []{ItemType.Claws}},
            {"List_of_daggers", new []{ItemType.Daggers}},
            {"List_of_maces", new []{ItemType.OneHandMaces, ItemType.Sceptres, ItemType.TwoHandMaces}},
            {"List_of_staves", new []{ItemType.Staves}},
            {"List_of_swords", new []{ItemType.OneHandSwords, ItemType.ThrustingOneHandSwords, ItemType.TwoHandSwords}},
            {"List_of_wands", new []{ItemType.Wands}},

            {"List_of_amulets", new []{ItemType.Amulets}},
            {"List_of_belts", new []{ItemType.Belts}},
            {"List_of_quivers", new []{ItemType.Quivers}},
            {"List_of_rings", new []{ItemType.Rings}},

            {"Body_armour", ItemGroup.BodyArmour.Types()},
            {"Boots", ItemGroup.Boots.Types()},
            {"Gloves", ItemGroup.Gloves.Types()},
            {"Helmet", ItemGroup.Helmet.Types()},
            {"Shield", ItemGroup.Shield.Types()}
        };

        private static readonly IReadOnlyDictionary<ItemType, XmlStat> HiddenImplicits;

        private static readonly IEnumerable<XmlItemBase> Jewels =
            ItemGroup.Jewel.Types().Select(t => new XmlItemBase { ItemType = t, Name = t.ToString().Replace("Jewels", " Jewel") }).ToList();

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
            dict[ItemType.BodyArmoursArmour] = penalty8;
            dict[ItemType.BodyArmoursArmourEnergyShield] = penalty8;
            foreach (var itemType in ItemGroup.Shield.Types())
            {
                dict[itemType] = penalty3;
            }
            HiddenImplicits = dict;
        }

        public override void Load()
        {
            using (var client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;

                var bases = new List<XmlItemBase>();
                foreach (var pair in WikiUrls)
                {
                    var doc = new HtmlDocument
                    {
                        OptionDefaultStreamEncoding = Encoding.UTF8
                    };
                    doc.LoadHtml(client.DownloadString(WikiUrlPrefix + pair.Key));

                    var itemTypes = pair.Value;
                    var tables =
                        doc.DocumentNode.SelectNodes("//table[contains(@class, 'wikitable')]")
                            .Where(node => node.SelectNodes("tr[1]/th[1 and . = \"Name\"]") != null).ToList();
                    if (itemTypes.Count > tables.Count)
                    {
                        Console.WriteLine("Not enough tables found in " + pair.Key +
                                          " for the number of item types that should be there.");
                        Console.WriteLine("Skipping item types " + itemTypes);
                        continue;
                    }
                    // Only the first itemType.Count tables are parsed.
                    bases.AddRange(tables.Take(itemTypes.Count).Zip(itemTypes, ParseTable).SelectMany(l => l));
                }
                Data = new XmlItemList
                {
                    ItemBases = bases.Union(Jewels).ToArray()
                };
            }
        }

        private static IEnumerable<XmlItemBase> ParseTable(HtmlNode table, ItemType itemType)
        {
            var isFirstRow = true;
            var nameColumn = -1;
            var lvlColumn = -1;
            var implicitColumn = -1;
            var propertyColumns = new Dictionary<int, string>();
            foreach (var row in table.Elements("tr"))
            {
                if (isFirstRow)
                {
                    var i = 0;
                    foreach (var cell in row.Elements("th"))
                    {
                        if (cell.InnerHtml == "Name")
                        {
                            nameColumn = i;
                        }
                        else if (cell.InnerHtml == "Modifiers")
                        {
                            implicitColumn = i;
                        }
                        else if (!cell.InnerHtml.Contains("<"))
                        {
                            if (!IgnoredColumns.Contains(cell.InnerHtml))
                                propertyColumns[i] = cell.InnerHtml;
                        }
                        else if (cell.FirstChild.GetAttributeValue("title", "") == "Required Level")
                        {
                            lvlColumn = i;
                        }
                        i++;
                    }
                    isFirstRow = false;
                }
                else
                {
                    var implicits = new List<XmlStat>();
                    if (HiddenImplicits.ContainsKey(itemType))
                    {
                        implicits.Add(HiddenImplicits[itemType]);
                    }
                    var implicitMultiplier = 1F;
                    if (implicitColumn >= 0)
                    {
                        var impl = ParseImplicit(row.ChildNodes[implicitColumn]);
                        if (impl != null)
                        {
                            implicits.Add(impl);
                            implicitMultiplier += impl.From / 100;
                        }
                    }

                    yield return new XmlItemBase
                    {
                        Level = ParseCell(row.ChildNodes[lvlColumn], 0),
                        ItemType = itemType,
                        Name = row.ChildNodes[nameColumn].GetAttributeValue("data-sort-value", ""),
                        Properties = ParseProperties(row, propertyColumns, implicitMultiplier).ToArray(),
                        Implicit = implicits.ToArray()
                    };
                }
            }
        }

        private static XmlStat ParseImplicit(HtmlNode implicitCell)
        {
            if (IsNotApplicableCell(implicitCell)) return null;

            var mod = FindContent(implicitCell);
            var matches = NumberRegex.Matches(mod);
            if (matches.Count > 0)
            {
                var from = ParseFloat(matches[0].Value);
                return new XmlStat
                {
                    From = from,
                    To = matches.Count > 1 ? ParseFloat(matches[1].Value) : @from,
                    Name = NumberRegex.Replace(mod, "#").Replace("(# to #)", "#")
                };
            }
            return null;
        }

        private static IEnumerable<XmlStat> ParseProperties(HtmlNode row, IReadOnlyDictionary<int, string> propertyColumns, float implicitMultiplier)
        {
            foreach (var propertyColumn in propertyColumns)
            {
                var cell = GetCell(row, propertyColumn.Key);
                var modified = cell.FirstChild.GetAttributeValue("class", "").Contains("text-mod");
                var inner = cell.InnerHtml;
                var childInner = cell.FirstChild.InnerHtml;
                float from;
                float to;
                var name = propertyColumn.Value;
                var success = true;
                if (TryParseCell(cell, out from) || (modified && TryParseFloat(childInner, out from)))
                {
                    to = from;
                }
                else if ((inner.EndsWith("%") && TryParseFloat(inner.Replace('%', ' '), out from))
                    || (modified && childInner.EndsWith("%") && TryParseFloat(childInner.Replace('%', ' '), out from)))
                {
                    to = from;
                    name += " %";
                }
                else if (cell.FirstChild.GetAttributeValue("class", "").Contains("text-physical"))
                {
                    var split = WebUtility.HtmlDecode(cell.FirstChild.InnerHtml).Split('–');
                    success = TryParseFloat(split[0], out from) & TryParseFloat(split[1], out to);
                }
                else
                {
                    to = -1;
                    success = false;
                }
                if (!success)
                {
                    Console.WriteLine("Could not parse floats from cell " + cell.InnerHtml);
                    continue;
                }
                if (modified)
                {
                    from /= implicitMultiplier;
                    to /= implicitMultiplier;
                }
                yield return new XmlStat
                {
                    From = from,
                    To = to,
                    Name = name
                };
            }
        }

        private static string FindContent(HtmlNode cell)
        {
            while (cell.InnerHtml.Contains("<"))
            {
                cell = cell.FirstChild;
            }
            return cell.InnerHtml;
        }

        private static bool IsNotApplicableCell(HtmlNode cell)
        {
            return cell.GetAttributeValue("class", "").Contains("table-na");
        }

        private static int ParseCell(HtmlNode cell, int @default)
        {
            if (IsNotApplicableCell(cell))
                return @default;
            int value;
            return TryParseInt(cell.InnerHtml, out value) ? value : @default;
        }

        private static float ParseCell(HtmlNode cell, float @default)
        {
            float value;
            return TryParseCell(cell, out value) ? value : @default;
        }

        private static bool TryParseCell(HtmlNode cell, out float value)
        {
            if (IsNotApplicableCell(cell))
            {
                value = 0;
                return false;
            }
            return TryParseFloat(cell.InnerHtml, out value);
        }

        private static HtmlNode GetCell(HtmlNode row, int index)
        {
            return row.ChildNodes[index];
        }
    }
}