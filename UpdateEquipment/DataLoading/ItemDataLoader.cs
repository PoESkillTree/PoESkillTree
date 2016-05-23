using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using log4net;
using POESKillTree.Model.Items;
using POESKillTree.Model.Items.Affixes;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Utils;

namespace UpdateEquipment.DataLoading
{
    /// <summary>
    /// Extracts item bases from the wiki as a <see cref="XmlItemList"/>.
    /// </summary>
    public class ItemDataLoader : XmlDataLoader<XmlItemList>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ItemDataLoader));

        /// <summary>
        /// Contains all property columns in wiki tables that must be ignored because they contain redundant information.
        /// </summary>
        private static readonly HashSet<string> IgnoredColumns = new HashSet<string> { "Damage per Second" };

        private static readonly Regex NumberRegex = new Regex(@"\d+(\.\d+)?");

        /// <summary>
        /// Contains all item types that have hidden implicits that are not listed in the wiki tables.
        /// </summary>
        private static readonly IReadOnlyDictionary<ItemType, XmlStat> HiddenImplicits;

        private static readonly IEnumerable<XmlItemBase> Jewels =
            ItemGroup.Jewel.Types().Select(t => new XmlItemBase { ItemType = t, Name = t.ToString().Replace("Jewel", " Jewel") }).ToList();

        /// <summary>
        /// Contains properties that have to be renamed because the Wiki's naming is incorrect.
        /// </summary>
        private static readonly IReadOnlyDictionary<string, string> PropertyRenaming = new Dictionary<string, string>
        {
            {"Damage", "Physical Damage"},
            {"Armour Rating", "Armour"}
        };

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

        private IEnumerable<XmlItemBase> ParseTable(HtmlNode table, ItemType itemType)
        {
            var isFirstRow = true;
            var nameColumn = -1;
            var lvlColumn = -1;
            var implicitColumn = -1;
            var strColumn = -1;
            var dexColumn = -1;
            var intColumn = -1;
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
                        else if (cell.FirstChild.GetAttributeValue("title", "") == "Required Strength")
                        {
                            strColumn = i;
                        }
                        else if (cell.FirstChild.GetAttributeValue("title", "") == "Required Dexterity")
                        {
                            dexColumn = i;
                        }
                        else if (cell.FirstChild.GetAttributeValue("title", "") == "Required Intelligence")
                        {
                            intColumn = i;
                        }
                        i++;
                    }
                    isFirstRow = false;
                }
                else
                {
                    var implicits = new List<XmlStat>();
                    var implicitMultiplier = 1F;
                    if (implicitColumn >= 0)
                    {
                        implicits.AddRange(ParseImplicit(row.ChildNodes[implicitColumn]));
                        if (implicits.Any())
                        {
                            implicitMultiplier += implicits[0].From / 100;
                        }
                    }
                    if (HiddenImplicits.ContainsKey(itemType))
                    {
                        implicits.Add(HiddenImplicits[itemType]);
                    }

                    yield return new XmlItemBase
                    {
                        Level = ParseCell(row.ChildNodes[lvlColumn], 0),
                        Strength = strColumn >= 0 ? ParseCell(row.ChildNodes[strColumn], 0) : 0,
                        Dexterity = dexColumn >= 0 ? ParseCell(row.ChildNodes[dexColumn], 0) : 0,
                        Intelligence = intColumn >= 0 ? ParseCell(row.ChildNodes[intColumn], 0) : 0,
                        ItemType = itemType,
                        Name = WebUtility.HtmlDecode(row.ChildNodes[nameColumn].GetAttributeValue("data-sort-value", "")),
                        Properties = ParseProperties(row, propertyColumns, implicitMultiplier).ToArray(),
                        Implicit = implicits.Any() ? implicits.ToArray() : null
                    };
                }
            }
        }

        private IEnumerable<XmlStat> ParseImplicit(HtmlNode implicitCell)
        {
            if (IsNotApplicableCell(implicitCell)) yield break;

            var mod = WebUtility.HtmlDecode(FindContent(implicitCell));
            var matches = NumberRegex.Matches(mod);
            if (matches.Count <= 0) yield break;

            mod = NumberRegex.Replace(mod, "#").Replace("–", "-");
            if (mod.Contains("#-#"))
            {
                if (matches.Count != 2)
                {
                    Log.Warn("Could not parse implicit " + FindContent(implicitCell));
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

        private IEnumerable<XmlStat> ParseProperties(HtmlNode row, IReadOnlyDictionary<int, string> propertyColumns, float implicitMultiplier)
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
                if (PropertyRenaming.ContainsKey(name))
                    name = PropertyRenaming[name];
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
                    Log.Warn("Could not parse floats from cell " + cell.InnerHtml);
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