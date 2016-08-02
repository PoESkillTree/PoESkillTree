using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using log4net;
using POESKillTree.SkillTreeFiles;
using POESKillTree.Utils.Extensions;
using Attribute = POESKillTree.SkillTreeFiles.ItemDB.Attribute;
using Gem = POESKillTree.SkillTreeFiles.ItemDB.Gem;
using Value = POESKillTree.SkillTreeFiles.ItemDB.Value;
using ValueAt = POESKillTree.SkillTreeFiles.ItemDB.ValueAt;
using ValuePerQuality = POESKillTree.SkillTreeFiles.ItemDB.ValuePerQuality;

namespace UpdateDB.DataLoading.Gems
{
    // Reader for Unofficial Path of Exile Wiki @ Gamepedia.
    // TODO: Parse for static modifiers as well (e.g. Spell Echo's "10% less Damage").
    //       (partly done, someone should check if I missed something)
    public class GamepediaReader : IGemReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GamepediaReader));

        // Parsed token.
        private class Token
        {
            internal string Name;
            internal string Value;
        }

        // Pattern to match numbers in attribute values.
        static Regex ReNumber = new Regex("(\\d+(\\.\\d+)?)");
        // Set of tokens to ignore.
        static HashSet<string> IgnoreTokens = new HashSet<string>
        {
            "Required Level", "Required Strength", "Required Dexterity", "Required Intelligence",
            "Experience Needed to Level Up", "Total experience needed", "Per #% Quality:"
        };
        // Mapping of incorrect tokens to the correct ingame form.
        static Dictionary<string, string> Tokens = new Dictionary<string, string>
        {
            { "ManaCost", "Mana Cost: #" }
        };
        // Mapping of incorrect tokens to the correct ingame form.
        private static readonly Dictionary<string, Tuple<string, IReadOnlyList<string>>> TagSpecificTokens =
            new Dictionary<string, Tuple<string, IReadOnlyList<string>>>
            {
                {
                    "Deals #% of Base Damage",
                    new Tuple<string, IReadOnlyList<string>>("Deals #% of Base Attack Damage", new[] {"Attack"})
                }
            };
        // The Wiki URL.
        static string URL = "http://pathofexile.gamepedia.com";
        // Translates gem name to actual Wiki page (e.g. Iron Grip (support gem)).
        static Dictionary<string, string> TranslateName = new Dictionary<string, string>
        {
            { "Portal", "Portal_(gem)" }
        };

        public HttpClient HttpClient { private get; set; }

        // Used so the http client does not get bombarded with requests (which leads to timeouts).
        // HttpClient allows 2 concurrent requests per host per default, so 4 semaphore slots are be enough.
        private readonly SemaphoreSlim _clientSema = new SemaphoreSlim(4);

        // Fetches gem data.
        public async Task<Gem> FetchGemAsync(string name)
        {
            string html;
            string url = URL + "/" + PathOf(name);
            try
            {
                await _clientSema.WaitAsync();
                Log.Info("Fetching gem: " + name);
                html = await HttpClient.GetStringAsync(url);
            }
            catch (WebException e)
            {
                Log.WarnFormat("Web exception for gem {0} ({1}):", name, url);
                if (e.Status == WebExceptionStatus.ProtocolError)
                    Log.Warn("HTTP " + ((int) ((HttpWebResponse) e.Response).StatusCode) + " " +
                             ((HttpWebResponse) e.Response).StatusDescription);
                else
                    Log.Warn(e.ToString());

                return null;
            }
            finally
            {
                _clientSema.Release();
            }

            HtmlDocument doc = new HtmlDocument();
            doc.OptionReadEncoding = false;
            doc.OptionDefaultStreamEncoding = Encoding.UTF8;
            doc.LoadHtml(html);

            // Get correct gem name.
            HtmlNode span = doc.DocumentNode.SelectSingleNode("//span[@itemprop='name']");
            string gemName = span == null ? name : span.InnerText.Trim();

            List<Attribute> attributes = new List<Attribute>();
            var tags = new List<string>();
            attributes.AddRange(ParseInfobox(doc, name, tags));
            attributes.AddRange(ParseProgressionTable(doc, name, tags));

            return new Gem { Name = gemName, Attributes = attributes, Tags = string.Join(", ", tags) };
        }

        private static IEnumerable<Attribute> ParseProgressionTable(HtmlDocument doc, string name, IReadOnlyCollection<string> tags)
        {
            HtmlNodeCollection found = doc.DocumentNode.SelectNodes("//table[contains(@class,'skill-progression-table')]");
            if (found == null)
            {
                Log.WarnFormat("Gem level table not found for {0}", name);
                return Enumerable.Empty<Attribute>();
            }

            HtmlNode table = found[0];
            bool hasHead = false;
            int levelColumn = 0;
            Dictionary<int, Attribute> columnAttribute = new Dictionary<int, Attribute>();

            foreach (HtmlNode row in table.Elements("tr"))
            {
                if (hasHead)
                {
                    HtmlNode cell = row.SelectSingleNode("th[" + (levelColumn + 1) + "]");
                    if (cell == null)
                    {
                        Log.Warn("Level cell not found");
                        break;
                    }

                    int level;
                    if (!int.TryParse(cell.InnerText.Trim(), out level))
                    {
                        Log.Warn("Level not an integer: " + cell.InnerText);
                        break;
                    }

                    foreach (int column in columnAttribute.Keys)
                    {
                        cell = row.SelectSingleNode("td[" + column + "]");
                        if (cell != null)
                        {
                            string text = WebUtility.HtmlDecode(cell.InnerText).Trim();
                            text = text.Replace("%", "");
                            if (text.Length > 0)
                                columnAttribute[column].Values.Add(new ValueAt { Level = level, Text = text });
                        }
                    }
                }
                else
                {
                    hasHead = true;
                    int column = 0;

                    foreach (HtmlNode cell in row.SelectNodes("td|th"))
                    {
                        HtmlNode abbr = cell.SelectSingleNode("abbr");
                        var text = abbr != null ? abbr.Attributes["title"].Value : cell.InnerText;

                        text = WebUtility.HtmlDecode(text);

                        if (text == "Level")
                        {
                            levelColumn = column;
                            Log.Debug("  [" + column + "] Level");
                        }
                        else
                        {
                            List<Token> tokens = ParseTokens(new List<string> { text }, tags);
                            if (tokens.Count == 1)
                            {
                                Log.Debug("  [" + column + "] " + tokens[0].Name);
                                columnAttribute.Add(column, new Attribute { Name = tokens[0].Name, Values = new List<Value>() });
                            }
                        }

                        ++column;
                    }
                }
            }

            return columnAttribute.Values;
        }

        private static IEnumerable<Attribute> ParseInfobox(HtmlDocument doc, string name, ICollection<string> tags)
        {
            var found = doc.DocumentNode.SelectNodes("//span[contains(@class,'item-box -gem')]");
            if (found == null || found.Count == 0)
            {
                Log.WarnFormat("Gem infobox table not found for {0}", name);
                yield break;
            }

            var table = found[0];
            var infoBoxDefaults = ParseInfoBoxDefaults(table);
            if (infoBoxDefaults != null)
            {
                foreach (Attribute attribute in infoBoxDefaults)
                {
                    yield return attribute;
                }
            }

            var itemboxstats = table.SelectSingleNode("span[contains(@class, 'item-stats')]");

            var tagNodes = itemboxstats.SelectNodes("span/a[contains(@title, '(gem tag)')]");
            tagNodes?.Select(n => n.InnerHtml).ForEach(tags.Add);

            var td = itemboxstats.SelectNodes("span[contains(@class, '-mod')]");
            if (td == null) yield break;

            // Per 1% Quality
            var textNodes = td[0].SelectNodes(".//text()");
            if (textNodes != null)
            {
                List<string> texts = new List<string>();
                foreach (HtmlNode node in textNodes)
                    texts.Add(node.InnerText);

                List<Token> tokens = ParseTokens(texts);
                foreach (Token token in tokens)
                {
                    Log.Debug("  [Per 1% Quality] " + token.Name);
                    if (token.Value != null)
                    {
                        yield return new Attribute
                        {
                            Name = token.Name,
                            Values = new List<Value> {new ValuePerQuality {Text = token.Value}}
                        };
                    }
                }
            }

            // Values fixed per level
            if (td.Count <= 1) yield break;
            textNodes = td[1].SelectNodes(".//text()");
            if (textNodes == null) yield break;
            foreach (var attr in textNodes.Select(n => n.InnerHtml).Select(ParseFixedAttribute).Where(a => a != null))
            {
                yield return attr;
            }
        }

        private static Attribute[] ParseInfoBoxDefaults(HtmlNode table)
        {
            var attributes = new List<Attribute>();

            var textDefaults = table.SelectNodes(".//span[contains(@class, '-default')]");
            if (textDefaults == null)
                return null;
            foreach (var textDefault in textDefaults)
            {
                var textValue = textDefault.NextSibling;

                if (!textValue.GetAttributeValue("class", "").Contains("-value"))
                {
                    continue;
                }

                var value = textValue.InnerHtml;

                // If the mana cost is fixed it is not necessarily part of the level table.
                if (textDefault.InnerHtml.Contains("Mana Cost") && Regex.IsMatch(value, @"^\d+$"))
                {
                    attributes.Add(new Attribute
                    {
                        Name = "Mana Cost: #",
                        Values = new List<Value> { new ItemDB.ValueForLevelRange { From = 1, To = 30, Text = value } }
                    });
                }

                if (textDefault.InnerHtml.Contains("Cast Time"))
                {
                    Match match = Regex.Match(value, @"^(?<val>\d+(\.\d+)?) sec$");

                    if (match.Success)
                    {
                        attributes.Add(new Attribute
                        {
                            Name = "Cast Time: # sec",
                            Values = new List<Value> {new ItemDB.ValueForLevelRange {From = 1, To = 30, Text = match.Groups["val"].Value}}
                        });
                    }
                }

                if (textDefault.InnerHtml.Contains("Cooldown Time"))
                {
                    Match match = Regex.Match(value, @"^(?<val>\d+(\.\d+)?) sec$");

                    if (match.Success)
                    {
                        attributes.Add(new Attribute
                        {
                            Name = "Cooldown Time: # sec",
                            Values = new List<Value> {new ItemDB.ValueForLevelRange {From = 1, To = 30, Text = match.Groups["val"].Value}}
                        });
                    }
                }

                if (textDefault.InnerHtml.Contains("Critical Strike Chance"))
                {
                    Match match = Regex.Match(value, @"^(?<val>\d+(\.\d+)?)%$");

                    if (match.Success)
                    {
                        attributes.Add(new Attribute
                        {
                            Name = "Critical Strike Chance: #%",
                            Values = new List<Value> {new ItemDB.ValueForLevelRange {From = 1, To = 30, Text = match.Groups["val"].Value}}
                        });
                    }
                }

                if (textDefault.InnerHtml.Contains("Damage Effectiveness"))
                {
                    Match match = Regex.Match(value, @"^(?<val>\d+)%$");

                    if (match.Success)
                    {
                        attributes.Add(new Attribute
                        {
                            Name = "Damage Effectiveness: #%",
                            Values = new List<Value> {new ItemDB.ValueForLevelRange {From = 1, To = 30, Text = match.Groups["val"].Value}}
                        });
                    }
                }
            }
            return attributes.ToArray();
        }

        private static Attribute ParseFixedAttribute(string text)
        {
            var numberMatches = ReNumber.Matches(text);
            if (numberMatches.Count != 1)
                return null;

            var valueText = numberMatches[0].Value;
            return new Attribute
            {
                Name = text.Replace(valueText, "#").Replace("\n", " "),
                Values = new List<Value> {new ItemDB.ValueForLevelRange {From = 1, To = 30, Text = valueText } }
            };
        }

        // Returns attribute names for known tokens, ignored tokens or null if unknown tokens.
        static List<Token> ParseTokens(IEnumerable<string> texts, IReadOnlyCollection<string> tags = null)
        {
            List<Token> tokens = new List<Token>();

            foreach (string text in texts)
            {
                // Parse values.
                string value = "";
                foreach (Match m in ReNumber.Matches(text))
                    value += " " + m.Groups[0].Value;
                value = value.TrimStart();

                // Replace numbers with 'x'.
                string token = ReNumber.Replace(text, "#");
                token = token.Replace("+x", "+#");
                token = token.Replace("x%", "#%");
                token = token.Replace("x-y", "#-#");
                token = token.Replace("x–y", "#-#"); //weird dash
                token = token.Replace("x ", "# ");
                token = token.Replace(" x", " #");

                if (IgnoreTokens.Contains(token))
                    continue;
                if (tags != null && TagSpecificTokens.ContainsKey(token)
                    && TagSpecificTokens[token].Item2.Intersect(tags).Any())
                {
                    token = TagSpecificTokens[token].Item1;
                }
                var name = Tokens.ContainsKey(token) ? Tokens[token] : token;
                tokens.Add(new Token { Name = name, Value = value.Length == 0 ? null : value });
            }

            return tokens;
        }

        // Returns URL path of gem.
        static string PathOf(string gemName)
        {
            // Replace space with underscore.
            var name = gemName.Replace(' ', '_');

            // If gem has different page than it's name, translate it.
            if (TranslateName.ContainsKey(name))
                name = TranslateName[name];

            return name;
        }
    }
}
