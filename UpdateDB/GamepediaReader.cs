using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
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
            "requiredlevel", "requiredstrength", "requireddexterity", "requiredintelligence",
            "experienceneededtolevelup"
        };
        // Mapping of tokens to actual attributes.
        static Dictionary<string, string> Tokens = new Dictionary<string, string>
        {
            { "additionalchaosdamage", "Adds #-# Chaos Damage" },
            { "additionalcolddamage", "Adds #-# Cold Damage" },
            { "additionalfiredamage", "Adds #-# Fire Damage" },
            { "additionallightningdamage", "Adds #-# Lightning Damage" },
            { "additionalphysicaldamage", "Adds #-# Physical Damage" },
            { "toaccuracyrating", "+# to Accuracy Rating" },
            { "chaosdamage", "Deals #-# Chaos Damage" },
            { "colddamage", "Deals #-# Cold Damage" },
            { "icedamage", "Deals #-# Cold Damage" },
            { "firedamage", "Deals #-# Fire Damage" },
            { "lightningdamage", "Deals #-# Lightning Damage" },
            { "physicaldamage", "Deals #-# Physical Damage" },
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
            { "moremeleephysicaldamageonfulllife", "#% more Melee Physical Damage when on Full Life" },
            { "morephysicalprojectileattackdamage", "#% more Physical Projectile Attack Damage" },
            { "moreweaponelementaldamage", "#% more Weapon Elemental Damage" },
            { "ofcoldaddedasfire", "Gain #% of Cold Damage Added as Extra Fire Damage" },
            { "gainx%ofphysicaldamageasextrachaosdamage", "Gain #% of Physical Damage as Extra Chaos Damage" },
            { "gainx%ofphysicaldamageasextrafiredamage", "Gain #% of Physical Damage as Extra Fire Damage" },
            { "manacost", "Mana Cost: #%" },
            { "reducedmanacost", "#% reduced Mana Cost" },

            // blood magic
            { "manacostmultiplier", "Mana cost multiplier #%"},
            // reduced mana
            { "manamultiplier", "Mana cost multiplier #%"},
            // curse on hit
            { "reducedcurseduration", "Curse duration reduced by #%"},
            // empower
            { "levelofsupportedactiveskillgems", "+# Level of Supported Active Skill Gems"},
            // generosity
            { "increasedeffectofauras", "#% increased effect of Auras you Cast"},
            //auras

            // aoe
            { "increasedareaofeffectradius", "#% increased Area of Effect radius" },
            //anger
            { "additionalfiredamagewithattacksforyouandnearbyallies", "You and nearby allies deal #–# additional Fire Damage with attacks" },
            //clarity
            { "youandnearbyalliesregeneratexmanapersecond", "You and nearby allies regenerate # Mana per second" }, 
            { "manaregen.per1manareserved(manaregen./manareserved)", "Manaregen efficiency #" },
            { "manareserved", "# Mana Reserved" },
            // determination
            { "youandnearbyalliesgainx%morearmour", "You and nearby allies gain #% more Armour"}, 
            // discipline
            { "youandnearbyalliesgainxadditionalenergyshield", "You and nearby allies gain #% additional Energy Shield" }, 
            { "energyshieldperpercentageofmanareserved", "Energy Shield efficiency #%" }, 
            // grace
            { "youandnearbyalliesgainxadditionalevasionrating", "You and nearby allies gain # additional Evasion rating" }, 
            // haste
            { "increasedmovementspeed", "You and nearby allies gain #% increased Movement Speed" }, 
            //{ "increasedattackspeed", "You and nearby allies gain #% increased Attack Speed" },
            //{ "increasedcastspeed", "You and nearby allies gain #% increased Cast Speed" },
            // hatred
            { "amountofphysicaldamageaddedascolddamageforyouandnearbyallies", "You and nearby allies add #% of your Physical Damage as Cold Damage" }, 
            // purity of elements
            { "additionalelementalresistancesforyouandnearbyallies", "You and nearby allies gain +#% to all Elemental Resistances" }, 
            // purity of fire
            { "additionalfireresistanceforyouandnearbyallies", "You and nearby allies gain #% additional Fire Resistance" }, 
            { "additionalmaximumfireresistanceforyouandnearbyallies", "You and nearby allies gain #% additional maximum Fire Resistance" },
            // puritfy of ice
            { "additionalcoldresistanceforyouandnearbyallies", "You and nearby allies gain #% additional Cold Resistance" },
            { "additionalmaximumcoldresistanceforyouandnearbyallies", "You and nearby allies gain #% additional maximum Cold Resistance" },
            // puritfy of lightning
            { "additionallightningresistanceforyouandnearbyallies", "You and nearby allies gain #% additional Lightning Resistance" },
            { "additionalmaximumlightningresistanceforyouandnearbyallies", "You and nearby allies gain #% additional maximum Lightning Resistance" },
            // vitality
            { "youandnearbyalliesregeneratex%lifepersecond", "You and nearby allies regenerate #% Life per second" }, 
            // wrath
            { "additionallightningdamagewithattacksforyouandnearbyallies", "You and nearby allies deal #-# additional Lightning Damage with attacks" }, 
            //herald of ice
            //{ "colddamage", "Deals #-# Cold Damage" },
            {"addedcolddamagetospells", "Adds #-# Cold Damage to Spells"},
            {"addedcolddamagetoattacks", "Adds #-# Cold Damage to Attacks"},
            // herald of thunder
            //{"lightningdamage","Deals #-# Lightning Damage"},
            {"addedlightningdamagetoattacksandspells", "Adds #-# Lightning Damage to Attacks and Spells"},
            // herald of ash
            {"ignitesforpercentageofoverkilldamage", "Ignites for #% of Overkill Damage"},
            // blood magic

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
            { "Blind", "Blind_(support_gem)" },
            { "Blood_Magic", "Blood_Magic_(support_gem)" },
            { "Cast_On_Melee_Kill", "Cast_on_Melee_Kill" },
            { "Cast_When_Damage_Taken", "Cast_when_Damage_Taken" },
            { "Cold_To_Fire", "Cold_to_Fire" },
            { "Iron_Grip", "Iron_Grip_(support_gem)" },
            { "Knockback", "Knockback_(support_gem)" },
            { "Life_Leech", "Life_Leech_(support_gem)" },
            { "Mana_Leech", "Mana_Leech_(support_gem)" },
            { "Melee_Damage_On_Full_Life", "Melee_Damage_on_Full_Life" },
            { "Pierce", "Pierce_(support_gem)" },
            { "Point_Blank", "Point_Blank_(support_gem)" },
            { "Physical_To_Lightning", "Physical_to_Lightning" },
            { "Rain_Of_Arrows", "Rain_of_Arrows" },
            { "Stun", "Stun_(support_gem)" },
            { "Trap", "Trap_(support_gem)" },
            //auras
            { "Purity_Of_Elements", "Purity_of_Elements" },
            { "Purity_Of_Fire", "Purity_of_Fire" },
            { "Purity_Of_Ice", "Purity_of_Ice" },
            { "Purity_Of_Lightning", "Purity_of_Lightning" },
            //heralds
            {"Herald_Of_Ice", "Herald_of_Ice"},
            {"Herald_Of_Ash", "Herald_of_Ash"},
            {"Herald_Of_Thunder", "Herald_of_Thunder"},
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
            List<string> keywords = new List<string>();
            string manaReseverd = null;
            string manaMultiplier = null;
            string gemIconPath = null;
            string skillIconPath = null;

            #region GemLevelTable: Gem level progression table
            HtmlNodeCollection found = doc.DocumentNode.SelectNodes("//table[contains(@class,'GemLevelTable')]");
            if (found == null)//*[@id="mw-content-text"]/table[2]
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
                            if ((abbr != null) && (abbr.Attributes["title"].Value.Length != 0))
                            {
                                text = abbr.Attributes["title"].Value;
                            }
                            else
                            {
                                text = cell.InnerText;
                            }

                            text = System.Net.WebUtility.HtmlDecode(text).Replace(" ", "").ToLowerInvariant();

                            if (text == "level")
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
            #endregion

            #region GemInfoboxContainer

            found = doc.DocumentNode.SelectNodes("//div[contains(@class,'GemInfoboxHeader')]");
            if (found.Count == 0)
                Warning("GemInfoboxHeader not found");
            else
            {
                System.IO.Directory.CreateDirectory("icons");
                HtmlNode table = found[0];
                HtmlNodeCollection gemImages = table.SelectNodes(".//a[contains(@class,'image')]/img");
                if (gemImages != null)
                {
                    using (WebClient client = new WebClient())
                    {
                        gemIconPath = PathOf(name) + "_gem_icon.png";
                        client.DownloadFile(new Uri(gemImages[0].Attributes["src"].Value), gemIconPath);
                    }
                    if (gemImages.Count > 1) {
                        using (WebClient client = new WebClient())
                        {
                            skillIconPath = PathOf(name) + "_skill_icon.png";
                            client.DownloadFile(new Uri(gemImages[1].Attributes["src"].Value), skillIconPath);
                        }
                    }
                }
            }

            found = doc.DocumentNode.SelectNodes("//table[contains(@class,'GemInfoboxInfo')]");
            if (found.Count == 0)
                Warning("GemInfoboxInfo table not found");
            else
            {
                HtmlNode table = found[0];

                HtmlNode td = table.SelectSingleNode("tr[td/text()='Per 1% Quality']/td[2]");
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
                // parse keywords
                keywords = ParseGemInfoboxContainerAttribute(table, "Keywords").Where(keyword => keyword != ",").ToList();

                // from the gameplay point of view heralds aren't auras, but from mechanic point of view they are
                if (gemName.IndexOf("herald", StringComparison.InvariantCultureIgnoreCase) > -1)
                {
                    keywords.Add("Aura");
                }

                keywords.Sort();

                // parse Mana Reserved
                manaReseverd = ParseGemInfoboxContainerAttribute(table, "Mana Reserved").FirstOrDefault();

                // parse Mana Multiplier. Blood magic's multiplier varies and is stored in attributes
                if (gemName != "Blood Magic")
                    manaMultiplier = ParseGemInfoboxContainerAttribute(table, "Mana Multiplier").FirstOrDefault();
            }
            #endregion

            return new Gem { 
                Name = gemName, 
                Attributes = attributes, 
                Keywords = keywords, 
                ManaReserved = manaReseverd, 
                ManaMultiplier = manaMultiplier,
                GemIcon = gemIconPath,
                SkillIcon = skillIconPath,
            }; ;
        }

        private List<string> ParseGemInfoboxContainerAttribute(HtmlNode gemInfoBoxTable, string attributeName)
        {
            List<string> result = new List<string>();
            HtmlNode td = gemInfoBoxTable.SelectSingleNode("tr[td/text()='" + attributeName + "']/td[2]");
            if (td != null)
            {
                HtmlNodeCollection textNodes = td.SelectNodes(".//text()");
                if (textNodes != null)
                {
                    foreach (HtmlNode node in textNodes)
                    {
                        string tmpStr = node.InnerText.Trim(' ', '\t');
                        if (tmpStr != "")
                        {
                            result.Add(tmpStr);
                        }
                    }
                }
            }
            return result;
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
                string token = ReNumber.Replace(text, "x");
                // Remove whitespaces and lowercase token.
                token = ReWhitespace.Replace(token, "").ToLowerInvariant();
                // Try removing +x% or shorter version from beginning of token.
                token = RePercent.Replace(token, "");

                if (Tokens.ContainsKey(token))
                {
                    string name = Tokens[token];
                    tokens.Add(new Token { IsAttribute = true, Name = XlateAttribute(gemName, Tokens[token]), Value = value.Length == 0 ? null : value });
                    texts.Remove(text);
                }
                else if (IgnoreTokens.Contains(token))
                    texts.Remove(text);
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
