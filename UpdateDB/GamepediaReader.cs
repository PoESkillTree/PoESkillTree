using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Attribute = POESKillTree.SkillTreeFiles.ItemDB.Attribute;
using Gem = POESKillTree.SkillTreeFiles.ItemDB.Gem;
using Value = POESKillTree.SkillTreeFiles.ItemDB.Value;
using ValueAt = POESKillTree.SkillTreeFiles.ItemDB.ValueAt;
using ValuePerQuality = POESKillTree.SkillTreeFiles.ItemDB.ValuePerQuality;

namespace UpdateDB
{
    // Reader for Unofficial Path of Exile Wiki @ Gamepedia.
    // TODO: Parse for static modifiers as well (e.g. Spell Echo's "10% less Damage").
    public class GamepediaReader : Reader
    {
        // Parsed token.
        internal class Token
        {
            internal bool IsAttribute = false;
            internal string Name;
            internal string Value;
        }

        // Pattern to match numbers in attribute values.
        static Regex ReNumber = new Regex("(\\d+(\\.\\d+)?)");
        // Pattern for percent prefix removal.
        static Regex RePercent = new Regex(@"(^\+?(x%|x|%))");
        // Pattern to match one or more whitespaces.
        static Regex ReWhitespace = new Regex(@"[ \t\u00A0]+");
        // Set of tokens to ignore.
        static HashSet<string> IgnoreTokens = new HashSet<string>
        {
            "Required Level", "Required Strength", "Required Dexterity", "Required Intelligence",
            "Experience Needed to Level Up", "Total experience needed", "Per #% Quality:"
        };
        // Mapping of tokens to actual attributes.
        static Dictionary<string, string> Tokens = new Dictionary<string, string>
        {
            { "adds#-#chaosdamage", "Adds #-# Chaos Damage" },
            { "addsx–ycolddamage", "Adds #-# Cold Damage" },
            { "addsx–yfiredamage", "Adds #-# Fire Damage" },
            { "adds#-#lightningdamage", "Adds #-# Lightning Damage" },
            { "additionalphysicaldamage", "Adds #-# Physical Damage" },
            { "toaccuracyrating", "+# to Accuracy Rating" },
            { "deals#-#chaosdamage", "Deals #-# Chaos Damage" },
            { "deals#-#icedamage", "Deals #-# Cold Damage" },
            { "deals#-#colddamage", "Deals #-# Cold Damage" },
            { "deals#-#firedamage", "Deals #-# Fire Damage" },
            { "deals#-#lightningdamage", "Deals #-# Lightning Damage" },
            { "deals#-#physicaldamage", "Deals #-# Physical Damage" },
            { "adds#-#colddamagetospells", "Adds #-# Cold Damage to Spells" },
            { "adds#-#colddamagetoattacks", "Adds #-# Cold Damage to Attacks" },
            { "adds#-#firedamagetospells", "Adds #-# Fire Damage to Spells" },
            { "adds#-#firedamagetoattacks", "Adds #-# Fire Damage to Attacks" },
            { "adds#-#lightningdamagetospells", "Adds #-# Lightning Damage to Spells" },
            { "adds#-#lightningdamagetoattacks", "Adds #-# Lightning Damage to Attacks" },
            { "increasedattackspeed", "#% increased Attack Speed"},
            { "reducedattackspeed", "#% reduced Attack Speed" },
            { "increasedcastspeed", "#% increased Cast Speed" },
            { "reducedcastspeed", "#% reduced Cast Speed" },
            { "moreattackspeed", "#% more Attack Speed" },
            { "moremeleeattackspeed", "#% more Melee Attack Speed" },
            { "morecastspeed", "#% more Cast Speed" },
            { "increasedcriticalstrikechance", "#% increased Critical Strike Chance" },
            { "increasedcriticalstrikemultiplier", "#% increased Critical Strike Multiplier" },
            { "increaseddamage", "#% increased Damage" },
            { "increasedareadamage", "#% increased Area Damage" },
            { "increasedchaosdamage", "#% increased Chaos Damage" },
            { "increasedcolddamage", "#% increased Cold Damage" },
            { "increasedfiredamage", "#% increased Fire Damage" },
            { "increasedlightningdamage", "#% increased Lightning Damage" },
            { "increasedmeleephysicaldamage", "#% increased Melee Physical Damage" },
            { "increasedphysicaldamage", "#% increased Physical Damage" },
            { "increasedprojectiledamage", "#% increased Projectile Damage" },
            { "increasedspelldamage", "#% increased Spell Damage" },
            { "increasedelementaldamagewithweapons", "#% increased Elemental Damage with Weapons" },
            { "ofbasedamage", "Deals #% of Base Damage" },
            { "moredamage", "#% more Damage" },
            { "lessdamage", "#% less Damage" },
            { "moreareadamage", "#% more Area Damage" },
            { "multipliertomeleephysicaldamage", "#% more Melee Physical Damage" },
            { "moremeleephysicaldamagewhenonfulllife", "#% more Melee Physical Damage when on Full Life" },
            { "moremeleephysicaldamage", "#% more Melee Physical Damage" },
            { "morephysicalprojectileattackdamage", "#% more Physical Projectile Attack Damage" },
            { "moreweaponelementaldamage", "#% more Weapon Elemental Damage" },
            { "ofcoldaddedasfire", "Gain #% of Cold Damage Added as Extra Fire Damage" },
            { "gainx%ofphysicaldamageasextrachaosdamage", "Gain #% of Physical Damage as Extra Chaos Damage" },
            { "gainx%ofphysicaldamageasextrafiredamage", "Gain #% of Physical Damage as Extra Fire Damage" },
            { "gainx%ofphysicaldamageasextracolddamage", "Gain #% of Physical Damage as Extra Cold Damage" },
            { "gainx%ofphysicaldamageasextralightningdamage", "Gain #% of Physical Damage as Extra Lightning Damage" },
            { "manacost", "Mana Cost: #" },
            { "reducedmanacost", "#% reduced Mana Cost" },
            { "chainxtimes", "Chain +# Times" },
            { "basedurationisxseconds", "Base duration is # seconds" },
            { "dealsx%ofbasedamage", "Deals x% of Base Damage" }, 
            { "increasedburningdamage", "x% increased Burning Damage" },
            { "supportedattackshaveax%chancetocastsupportedspellswhenyoucritanenemy", "Supported Attacks have a #% chance to Cast Supported Spells when you Crit an Enemy" }, 
            { "moredamagewhiledead", "#% more Damage while Dead" },
            { "supportedtriggeredspellshavex%increasedspelldamage", "Supported Triggered Spells have #% increased Spell Damage" },
            { "castssupportedspellswhenyoutakeatotalofxdamage", "Casts Supported Spells when you take a total of # Damage" },
            { "lessdamage\nx%moredamage", "#% more Damage" },
            { "thisgemcanonlysupportskillgemsrequiringlevelxorlower", "This Gem can only Support Skill Gems requiring Level # or lower" },
            { "chancetocastsupportedspellswhenstunned", "#% chance to Cast Supported Spells when Stunned" },
            { "increasedareaofeffectradius", "#% increased Area of Effect radius" },
            { "gainx%ofcolddamageasextrafiredamage", "Gain #% of Cold Damage as Extra Fire Damage" },
            { "candeal#-#basefiredamage", "Can deal #-# base Fire damage" },
            { "candeal#-#colddamage", "Can deal #-# base Cold damage" },
            { "candeal#-#lightningdamage", "Can deal #-# base Lightning damage" }, 
            { "increasedaccuracyrating", "#% increased Accuracy Rating" }, 
            { "chancetoshockenemies", "#% chance to Shock enemies" }, 
            { "increasedignitedurationonenemies", "#% increased Ignite Duration on enemies" },
            { "increasedareaofeffectradiuswhiledead", "#% increased Area of Effect radius while Dead" },
            { "increasedprojectilespeed", "#% increased Projectile Speed" }, 
            { "lessprojectilespeed", "#% less Projectile Speed" }, 
            { "minionsdealx%increaseddamage", "Minions deal #% increased Damage" },
            { "increasedelementaldamage", "#% increased Elemental Damage" },
            { "chancetoigniteenemies", "#% chance to Ignite enemies"},
            { "explosiondeals#-#basefiredamageperfusecharge", "Explosion deals #-# Base Fire damage per Fuse Charge" },
            { "lessprojectiledamage", "#% less Projectile Damage" }, 
            { "moreprojectiledamage", "#% more Projectile Damage" },
            { "wallwillbexunitslong", "Wall will be # units long" }, 
            { "increasedduration", "#% increased Duration"},
            { "reducedduration", "#% reduced Duration"},
            { "chancetoknockenemiesbackonhit", "#% chance to Knock Enemies Back on hit" },
            { "increasedstundurationonenemies", "#% increased Stun Duration on enemies" },
            { "additionalprojectiles", "# additional Projectiles"},
            { "additionalarrows", "# additional Arrows"},
            { "increasedchilldurationonenemies", "#% increased Chill Duration on enemies" },
            { "increasedfreezedurationonenemies", "#% increased Freeze Duration on enemies" },
            { "tocriticalstrikemultiplier", "+#% to Critical Strike Multiplier" },
            { "tocriticalstrikechance", "+#% to Critical Strike Chance" },
            { "chanceofprojectilespiercing", "#% chance of Projectiles Piercing" },
            { "lessdamagetoothertargets", "#% less Damage to other targets" },
            { "moremeleesplashradius", "#% more Melee Splash Radius" },
            { "shieldsbreakafterxtotaldamageisprevented", "Shields break after # total Damage is prevented" },
            { "additionalarmour", "# additional Armour" },
            { "dealsxbasechaosdamagepersecond", "Deals # Base Chaos Damage per second" },
            { "increasedmovementspeed", "#% increased Movement Speed" },
            { "moredamageatmaximumchargedistance", "#% more Damage at Maximum Charge Distance" },
            { "increasedshockdurationonenemies", "#% increased Shock Duration on enemies" },
            { "deals #% of base damage", "Deals #% of Base Attack Damage"}
        };
        // The Wiki URL.
        static string URL = "http://pathofexile.gamepedia.com";
        // Translates gem attribute name (attribute on Wiki can have wrong name).
        static Dictionary<string, string[]> TranslateAttribute = new Dictionary<string, string[]>
        {
            // Gem name                            Wiki attribute name          In-game attribute name
            { "Cast on Melee Kill", new string[] { "#% increased Spell Damage", "Supported Triggered Spells have #% increased Spell Damage" } },
            { "Multistrike",        new string[] { "#% more Attack Speed",      "#% more Melee Attack Speed" } }
        };
        // Translates gem name to actual Wiki page (e.g. Iron Grip (support gem)).
        static Dictionary<string, string> TranslateName = new Dictionary<string, string>
        {
            { "Fortify", "Fortify_(support_gem)" },
            { "Life_Gain_on_Hit", "Life_Gain_on_Hit_(support_gem)" },
            { "Point_Blank", "Point_Blank_(support_gem)" },
            { "Blind", "Blind_(support_gem)" },
            { "Blood_Magic", "Blood_Magic_(support_gem)" },
            { "Culling_Strike", "Culling_Strike_(support_gem)" },
            { "Iron_Grip", "Iron_Grip_(support_gem)" },
            { "Knockback", "Knockback_(support_gem)" },
            { "Life_Leech", "Life_Leech_(support_gem)" },
            { "Mana_Leech", "Mana_Leech_(support_gem)" },
            { "Pierce", "Pierce_(support_gem)" },
            { "Poison", "Poison_(support_gem)" },
            { "Stun", "Stun_(support_gem)" },
            { "Trap", "Trap_(support_gem)" },
            { "Curse_on_Hit", "Curse_On_Hit" },
            { "Portal_(Gem)", "Portal_(gem)" },
            { "Power_Charge_on_Critical", "Power_Charge_On_Critical" }
        };

        // Fetches gem data.
        override public Gem FetchGem(string name)
        {
            string html;
            try
            {
                string url = URL + "/" + PathOf(name);
                Info("Fetching gem: " + name + " (" + url + ")");

                WebClient webClient = new WebClient();
                webClient.Encoding = Encoding.UTF8;
                html = webClient.DownloadString(url);
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                    Warning("HTTP " + ((int)((HttpWebResponse)e.Response).StatusCode) + " " + ((HttpWebResponse)e.Response).StatusDescription);
                else
                    Warning(e.ToString());

                return null;
            }

            HtmlDocument doc = new HtmlDocument();
            doc.OptionReadEncoding = false;
            doc.OptionDefaultStreamEncoding = Encoding.UTF8;
            doc.LoadHtml(html);

            // Get correct gem name.
            HtmlNode span = doc.DocumentNode.SelectSingleNode("//span[@itemprop='name']");
            string gemName = span == null ? name : span.InnerText.Trim();

            List<Attribute> attributes = new List<Attribute>();

            HtmlNodeCollection found = doc.DocumentNode.SelectNodes("//table[contains(@class,'skill-progression-table')]");
            if (found == null)
                Warning("Gem level table not found");
            else
            {
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
                            Warning("Level cell not found");
                            break;
                        }

                        int level;
                        if (!int.TryParse(cell.InnerText.Trim(), out level))
                        {
                            Warning("Level not an integer: " + cell.InnerText);
                            break;
                        }

                        foreach (int column in columnAttribute.Keys)
                        {
                            cell = row.SelectSingleNode("td[" + column + "]");
                            if (cell != null)
                            {
                                string text = System.Net.WebUtility.HtmlDecode(cell.InnerText).Trim();
                                text = text.Replace("%", "");
                                if (text.Length > 0)
                                    columnAttribute[column].Values.Add(new ValueAt { Level = level, Text = text });
                            }
                        }
                    }
                    else
                    {
                        hasHead = true;
                        string text;
                        int column = 0;

                        foreach (HtmlNode cell in row.SelectNodes("td|th"))
                        {
                            HtmlNode abbr = cell.SelectSingleNode("abbr");
                            if (abbr != null)
                            {
                                text = abbr.Attributes["title"].Value;
                            }
                            else
                            {
                                text = cell.InnerText;
                            }

                            text = System.Net.WebUtility.HtmlDecode(text);

                            if (text == "Level")
                            {
                                levelColumn = column;
                                Verbose("  [" + column + "] Level");
                            }
                            else
                            {
                                List<Token> tokens = ParseTokens(gemName, new List<string> { text });
                                if (tokens.Count == 1)
                                {
                                    if (!tokens[0].IsAttribute)
                                        Verbose("  [" + column + "] Unknown token: " + text);
                                    else
                                    {
                                        Verbose("  [" + column + "] " + tokens[0].Name);
                                        columnAttribute.Add(column, new Attribute { Name = tokens[0].Name, Values = new List<Value>() });
                                    }
                                }
                            }

                            ++column;
                        }
                    }
                }

                if (columnAttribute.Count > 0)
                    attributes.AddRange(columnAttribute.Values);
            }

            found = doc.DocumentNode.SelectNodes("//div[contains(@class,'itembox-gem')]");
            if (found.Count == 0)
                Warning("Gem infobox table not found");
            else
            {
                HtmlNode table = found[0];
                HtmlNode itemboxstats = table.SelectSingleNode("//span[contains(@class, 'itemboxstats')]");
                HtmlNode td = itemboxstats.SelectSingleNode("//span[contains(@class, 'text-mod')]");
                if (td != null)
                {
                    HtmlNodeCollection textNodes = td.SelectNodes(".//text()");
                    if (textNodes != null)
                    {
                        List<string> texts = new List<string>();
                        foreach (HtmlNode node in textNodes)
                            texts.Add(node.InnerText);

                        List<Token> tokens = ParseTokens(gemName, texts);
                        foreach (Token token in tokens)
                        {
                            if (token.IsAttribute)
                            {
                                Verbose("  [Per 1% Quality] " + token.Name);
                                if (token.Value != null)
                                    attributes.Add(new Attribute { Name = token.Name, Values = new List<Value> { new ValuePerQuality { Text = token.Value } } });
                            }
                            else
                                Verbose("  [Per 1% Quality] Unknown token(s): " + token.Name);
                        }
                    }
                }
            }

            return new Gem { Name = gemName, Attributes = attributes }; ;
        }

        // Returns attribute names for known tokens, ignored tokens or null if unknown tokens.
        static List<Token> ParseTokens(string gemName, List<string> texts)
        {
            List<Token> tokens = new List<Token>();

            // 1) Try each text as separate token.
            foreach (string text in new List<string>(texts))
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
                    texts.Remove(text);
                else if (Tokens.ContainsKey(token.ToLowerInvariant()))
                {
                    tokens.Add(new Token { IsAttribute = true, Name = XlateAttribute(gemName, Tokens[token.ToLowerInvariant()]), Value = value.Length == 0 ? null : value });
                    texts.Remove(text);
                }
                else
                {
                    tokens.Add(new Token { IsAttribute = true, Name = XlateAttribute(gemName, token), Value = value.Length == 0 ? null : value });
                    texts.Remove(text);
                }
            }

            // 2) Try concatenated texts as single token.
            if (texts.Count > 1)
            {
                string token = String.Concat(texts);

                // Parse values.
                string value = "";
                foreach (Match m in ReNumber.Matches(token))
                    value += " " + m.Groups[0].Value;
                value = value.TrimStart();

                // Replace numbers with 'x'.
                token = ReNumber.Replace(token, "x");
                // Remove whitespaces and lowercase token.
                token = ReWhitespace.Replace(token, "").ToLowerInvariant();
                // Try removing +x% or shorter version from beginning of token.
                token = RePercent.Replace(token, "");

                if (Tokens.ContainsKey(token))
                {
                    tokens.Add(new Token { IsAttribute = true, Name = XlateAttribute(gemName, Tokens[token]), Value = value.Length == 0 ? null : value });
                    texts.Clear();
                }
                else if (IgnoreTokens.Contains(token))
                    texts.Clear();
            }

            // 3) Unprocessed texts concat into unknown token.
            if (texts.Count > 0)
            {
                string token = String.Join(",", texts);
                // Replace numbers with 'x'.
                token = ReNumber.Replace(token, "x");
                // Remove whitespaces and lowercase token.
                token = ReWhitespace.Replace(token, "").ToLowerInvariant();

                tokens.Add(new Token { Name = token });
            }

            return tokens;
        }

        // Returns URL path of gem.
        static string PathOf(string gemName)
        {
            // Collapse whitespaces into single space.
            string name = ReWhitespace.Replace(gemName, " ");

            // Capitalize first letter of each word.
            name = System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(name);

            // Replace space with underscore.
            name = name.Replace(' ', '_');

            //Replace incorrectly capt words
            name = name.Replace("_On_", "_on_");
            name = name.Replace("_Of_", "_of_");
            name = name.Replace("_And_", "_and_");
            name = name.Replace("_To_", "_to_");

            // If gem has different page than it's name, translate it.
            if (TranslateName.ContainsKey(name))
                name = TranslateName[name];

            return name;
        }

        // Returns correct name of gem's attribute.
        static string XlateAttribute(string gemName, string attrName)
        {
            if (TranslateAttribute.ContainsKey(gemName))
            {
                string[] xlate = TranslateAttribute[gemName];
                for (int i = 0; i < xlate.Length; i += 2)
                    if (attrName == xlate[0])
                        return xlate[1];
            }

            return attrName;
        }
    }
}
