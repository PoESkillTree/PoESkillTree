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

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("treeVersion")]
        public int TreeVersion { get; set; }

        [XmlAttribute("characterClassId")]
        public byte CharacterClassId { get; set; }

        [XmlAttribute("ascendancyClassId")]
        public byte AscendancyClassId { get; set; }

        [XmlAttribute("nodes")]
        public int Nodes { get; set; }

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

        public string GetAlternativeUrl(string urlName)
        {
            return Urls.FirstOrDefault(url =>
                !url.IsDefault && url.Name.Equals(urlName, StringComparison.Ordinal))?.Value;
        }
    }
}
