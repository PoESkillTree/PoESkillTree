using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using POESKillTree.Model.Items.Affixes;
using POESKillTree.Utils.Extensions;

namespace UpdateDB.DataLoading
{
    /// <summary>
    /// Utility class to convert Wiki stat text (i.e. contents of "Has implicit stat text" and "Has explicit stat
    /// text") to XmlStats.
    /// </summary>
    public class WikiStatTextUtils
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WikiStatTextUtils));

        private static readonly Regex NumberRegex = new Regex(@"\d+(\.\d+)?");
        // Links in stat texts are replaced by their second group (first: linked page title, second: text)
        private static readonly Regex LinkRegex = new Regex(@"\[\[([\w\s\d]+\|)?([\w\s\d]+)\]\]");
        
        /// <summary>
        /// Converts the given stat text into XmlStats.
        /// </summary>
        public static IEnumerable<XmlStat> ConvertStatText(string statText)
        {
            // split text at "<br>", replace links, convert each to XmlStat
            return from raw in statText.Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries)
                   let filtered = LinkRegex.Replace(raw, "$2")
                   from s in ConvertStat(filtered)
                   select s;
        }

        private static IEnumerable<XmlStat> ConvertStat(string stat)
        {
            var matches = NumberRegex.Matches(stat);
            if (matches.Count <= 0)
            {
                // no numbers in stat, easy
                yield return new XmlStat { Name = stat };
                yield break;
            }

            stat = NumberRegex.Replace(stat, "#");
            // range: first value is From, second is To
            const string range = "(#-#)";
            // added damage: first value is minimum, second is maximum
            const string addNoRange = "# to #";
            // added damage with range
            const string addRange = range + " to " + range;
            if (stat.Contains(addNoRange))
            {
                if (matches.Count != 2)
                {
                    Log.Warn($"Could not parse {stat}");
                    yield break;
                }
                // stat contains "#1 to #2", convert to two stats:
                // "#1 minimum" and "#2 maximum"
                var from = matches[0].Value.ParseFloat();
                yield return new XmlStat
                {
                    From = from,
                    To = from,
                    Name = stat.Replace(addNoRange, "# minimum")
                };
                from = matches[1].Value.ParseFloat();
                yield return new XmlStat
                {
                    From = from,
                    To = from,
                    Name = stat.Replace(addNoRange, "# maximum")
                };
            }
            else if (stat.Contains(addRange))
            {
                if (matches.Count != 4)
                {
                    Log.Warn($"Could not parse {stat}");
                    yield break;
                }
                // stat contains "(#1-#2) to (#3-#4)" convert to two stats:
                // "(#1-#2) minimum" and "(#3-#4) maximum"
                yield return new XmlStat
                {
                    From = matches[0].Value.ParseFloat(),
                    To = matches[1].Value.ParseFloat(),
                    Name = stat.Replace(addRange, "# minimum")
                };
                yield return new XmlStat
                {
                    From = matches[2].Value.ParseFloat(),
                    To = matches[3].Value.ParseFloat(),
                    Name = stat.Replace(addRange, "# maximum")
                };
            }
            else
            {
                // stat contains "#1" or "(#1-#2)
                var from = matches[0].Value.ParseFloat();
                yield return new XmlStat
                {
                    From = from,
                    To = matches.Count > 1 ? matches[1].Value.ParseFloat() : from,
                    Name = stat.Replace(range, "#")
                };
            }
        }
    }
}