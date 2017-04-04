using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using log4net;
using POESKillTree.Model.Items.Affixes;
using POESKillTree.Utils.Extensions;

namespace UpdateDB.DataLoading
{
    using CSharpGlobalCode.GlobalCode_ExperimentalCode;
    /// <summary>
    /// Utility class to convert Wiki stat text (i.e. contents of "Has implicit stat text" and "Has explicit stat
    /// text") to XmlStats.
    /// </summary>
    public static class WikiStatTextUtils
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WikiStatTextUtils));

        // '((?<=\()-)?': '-' is only included in the first number of ranges (i.e. '(-#-#)')
        private static readonly Regex NumberRegex = new Regex(@"((?<=\()-)?\d+(\.\d+)?");
        private static readonly Regex PlaceholderRegex = new Regex(@"#|\(?#-#\)?");
        // Links in stat texts are replaced by their second group (first: linked page title, second: text)
        private static readonly Regex LinkRegex = new Regex(@"\[\[([\w\s\d]+\|)?([\w\s\d]+)\]\]");
        
        /// <summary>
        /// Converts the given stat text into XmlStats.
        /// </summary>
        public static IEnumerable<XmlStat> ConvertStatText(string statText)
        {
            // split text at "<br>", replace links, convert each to XmlStat
            return statText.Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => LinkRegex.Replace(s, "$2"))
                .Select(WebUtility.HtmlDecode)
                .Select(ConvertStat)
                .Where(s => s != null);
        }

        private static XmlStat ConvertStat(string stat)
        {
            // not interested in "Corrupted" stat
            if (stat == "<em class=\"tc -corrupted\">Corrupted</em>")
            {
                return null;
            }
            if (stat.Contains("#"))
            {
                Log.Warn($"Weird stat encountered: {stat}");
                return null;
            }

            var numberMatches = NumberRegex.Matches(stat);
            var statWithPlaceholders = NumberRegex.Replace(stat, "#");
            var placeholderMatches = PlaceholderRegex.Matches(statWithPlaceholders);

#if (PoESkillTree_UseSmallDec_ForAttributes)
            var from = new List<SmallDec>();
            var to = new List<SmallDec>();
#else
            var from = new List<float>();
            var to = new List<float>();
#endif
            var i = 0;
            foreach (Match match in placeholderMatches)
            {
                var placeholder = match.Value;
#if (PoESkillTree_UseSmallDec_ForAttributes)
                if (placeholder == "#")
                {
                    SmallDec value = numberMatches[i].Value;
                    from.Add(value);
                    to.Add(value);
                    i++;
                }
                else
                {
                    from.Add(numberMatches[i].Value);
                    i++;
                    to.Add(numberMatches[i].Value);
                    i++;
                }
#else
                if (placeholder == "#")
                {
                    var value = numberMatches[i].Value.ParseFloat();
                    from.Add(value);
                    to.Add(value);
                    i++;
                }
                else
                {
                    from.Add(numberMatches[i].Value.ParseFloat());
                    i++;
                    to.Add(numberMatches[i].Value.ParseFloat());
                    i++;
                }
#endif
            }
            return new XmlStat
            {
                From = from,
                To = to,
                Name = PlaceholderRegex.Replace(statWithPlaceholders, "#")
            };
        }
    }
}