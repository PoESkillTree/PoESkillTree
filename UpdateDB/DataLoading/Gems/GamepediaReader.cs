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
using POESKillTree.Utils;
using POESKillTree.Utils.Extensions;
using POESKillTree.Model.Gems;

namespace UpdateDB.DataLoading.Gems
{
    // Reader for Unofficial Path of Exile Wiki @ Gamepedia.
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
        private static readonly Regex NumberRegex = new Regex(@"\d+(\.\d+)?");

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

            List<GemAttribute> attributes = new List<GemAttribute>();
            var tags = new List<string>();
            attributes.AddRange(ParseInfobox(doc, name, tags));
            attributes.AddRange(ParseProgressionTable(doc, name, tags));

            return new Gem { Name = gemName, Attributes = attributes, Tags = string.Join(", ", tags) };
        }

        private static IEnumerable<GemAttribute> ParseProgressionTable(HtmlDocument doc, string name, IReadOnlyCollection<string> tags)
        {
            HtmlNodeCollection found = doc.DocumentNode.SelectNodes("//table[contains(@class,'skill-progression-table')]");
            if (found == null)
            {
                Log.WarnFormat("Gem level table not found for {0}", name);
                return Enumerable.Empty<GemAttribute>();
            }

            HtmlNode table = found[0];
            bool hasHead = false;
            int levelColumn = 0;
            Dictionary<int, GemAttribute> columnAttribute = new Dictionary<int, GemAttribute>();

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
                                columnAttribute.Add(column, new GemAttribute { Name = tokens[0].Name, Values = new List<Value>() });
                            }
                        }

                        ++column;
                    }
                }
            }

            return columnAttribute.Values;
        }

        private static IEnumerable<GemAttribute> ParseInfobox(HtmlDocument doc, string name, ICollection<string> tags)
        {
            var found = doc.DocumentNode.SelectNodes("//span[contains(@class,'item-box -gem')]");
            if (found == null || found.Count == 0)
            {
                Log.Warn($"Gem infobox table not found for {name}");
                yield break;
            }

            var itemBox = WikiUtils.ParseItemBox(found[0]);
            var statGroups = itemBox.StatGroups;
            if (!statGroups.Any())
            {
                Log.Warn($"Gem infobox table for {name} is empty");
                yield break;
            }
            // Some attributes are both in first and fixed group. Don't allow such duplicates.
            var fixedAttrNames = new HashSet<string>();

            // Attributes in first group (property group)
            foreach (var wikiItemStat in statGroups[0])
            {
                var stats = wikiItemStat.Stats;
                if (stats.Length != 2 || stats[0].Item2 != WikiStatColor.Default
                    || stats[1].Item2 != WikiStatColor.Value)
                    continue;

                var attr = ParseSingleValueAttribute(wikiItemStat.StatsCombined);
                if (attr == null)
                    continue;
                if (fixedAttrNames.Add(attr.Name))
                    yield return attr;
            }

            // Tags
            if (statGroups[0].Any())
            {
                var tagStat = statGroups[0][0];
                if (tagStat.Stats.Length == 1)
                {
                    tagStat.StatsCombined.Split(new[] {", "}, StringSplitOptions.RemoveEmptyEntries).ForEach(tags.Add);
                }
            }

            // Per 1% Quality
            var qualityGroupIndex = -1;
            for (var i = 0; i < statGroups.Length; i++)
            {
                if (statGroups[i].Any()
                    && statGroups[i].First().StatsCombined.Contains("Per 1% Quality:"))
                {
                    qualityGroupIndex = i;
                    break;
                }
            }
            if (qualityGroupIndex < 0)
                yield break;
            var texts = statGroups[qualityGroupIndex].Skip(1).Select(s => s.StatsCombined);
            foreach (var token in ParseTokens(texts))
            {
                if (token.Value != null)
                {
                    yield return new GemAttribute
                    {
                        Name = token.Name,
                        Values = new List<Value> {new ValuePerQuality {Text = token.Value}}
                    };
                }
            }

            // Values fixed per level
            var fixedGroupIndex = qualityGroupIndex + 1;
            if (fixedGroupIndex < 1 || fixedGroupIndex >= statGroups.Length)
                yield break;
            var attrs = statGroups[fixedGroupIndex]
                .Select(s => s.StatsCombined)
                .Select(ParseSingleValueAttribute)
                .Where(a => a != null);
            foreach (var attr in attrs)
            {
                if (fixedAttrNames.Add(attr.Name))
                    yield return attr;
            }
        }

        private static GemAttribute ParseSingleValueAttribute(string text)
        {
            var numberMatches = NumberRegex.Matches(text);
            if (numberMatches.Count != 1)
                return null;
            return new GemAttribute
            {
                Name = NumberRegex.Replace(text, "#").Replace("\n", " "),
                Values =
                    new List<Value> {new ValueForLevelRange {From = 1, To = 30, Text = numberMatches[0].Value}}
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
                foreach (Match m in NumberRegex.Matches(text))
                    value += " " + m.Groups[0].Value;
                value = value.TrimStart();

                // Replace numbers with 'x'.
                string token = NumberRegex.Replace(text, "#");
                token = token.Replace("+x", "+#");
                token = token.Replace("x%", "#%");
                token = token.Replace("x-y", "# to #");
                token = token.Replace("x–y", "# to #"); //weird dash
                token = token.Replace("x to y", "# to #");
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
