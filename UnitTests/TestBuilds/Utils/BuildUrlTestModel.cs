using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace UnitTests.TestBuilds.Utils
{
    /// <summary>
    /// Represents build url describing model, serializable from xml.
    /// </summary>
    public class BuildUrlTestModel
    {
        private string _description;

        public string DefaultUrl => Urls.FirstOrDefault(x => x.IsDefault)?.Value;

        public ICollection<string> AlternativeUrls => Urls.Where(x => !x.IsDefault).Select(x => x.Value).ToList();

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("treeVersion")]
        public int TreeVersion { get; set; }

        [XmlAttribute("characterClassId")]
        public byte CharacterClassId { get; set; }

        [XmlAttribute("ascendancyClassId")]
        public byte AscendancyClassId { get; set; }

        [XmlAttribute("points")]
        public int Points { get; set; }

        [XmlAttribute("ascendancyPoints")]
        public int AscendancyPoints { get; set; }

        [XmlArray("urls")]
        [XmlArrayItem("default", typeof(DefaultUrlItem))]
        [XmlArrayItem("alternative", typeof(UrlItem))]
        public List<UrlItem> Urls { get; set; }

        [XmlElement("description")]
        public string Description
        {
            get
            {
                return string.IsNullOrEmpty(_description)
                    ? _description
                    : new Regex(@"\s{2,}").Replace(_description.Trim(), "");
            }
            set { _description = value; }
        }

        [XmlElement("bandits")]
        public TestBanditSettings Bandits { get; set; }

        [XmlElement("tags")]
        public TagsCollection Tags { get; set; }

        public int GetTotalPoints(bool includeClassPoints = false)
        {
            var totalPoints = Points + AscendancyPoints;

            if (includeClassPoints)
            {
                // +1 for class root node
                totalPoints++;

                if (AscendancyClassId > 0)
                {
                    totalPoints++;
                }
            }

            return totalPoints;
        }

        public string GetAlternativeUrl(string urlName)
        {
            return Urls.FirstOrDefault(url =>
                !url.IsDefault && url.Name.Equals(urlName, StringComparison.Ordinal))?.Value;
        }
    }
}
