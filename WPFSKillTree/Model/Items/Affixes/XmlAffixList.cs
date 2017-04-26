using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Utils.Extensions;

namespace POESKillTree.Model.Items.Affixes
{
    // Contains the classes that allow serialization and deserialization of Affixes.xml

    [XmlRoot(Namespace = "", IsNullable = false, ElementName = "AffixList")]
    public class XmlAffixList
    {
        [XmlElement("Affix")]
        public XmlAffix[] Affixes { get; set; }
    }

    public class XmlAffix
    {
        [XmlElement("Tier")]
        public XmlTier[] Tiers { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public ModType ModType { get; set; }

        [XmlAttribute]
        public bool Global { get; set; }

        [XmlAttribute]
        public ItemType ItemType { get; set; }

        [XmlAttribute]
        public ItemGroup ItemGroup { get; set; }
    }

    public class XmlTier
    {
        [XmlElement("Stat")]
        public XmlStat[] Stats { get; set; }

        [XmlAttribute]
        public int ItemLevel { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public int Tier { get; set; }

        [XmlAttribute]
        public ModGroup ModGroup { get; set; }
    }

    public class XmlStat
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlIgnore]
        public IReadOnlyList<float> From { get; set; }

        [XmlAttribute(AttributeName = "From")]
        public string FromAsString
        {
            get { return Join(From); }
            set { From = Split(value); }
        }

        [XmlIgnore]
        public IReadOnlyList<float> To { get; set; }

        [XmlAttribute(AttributeName = "To")]
        public string ToAsString
        {
            get { return Join(To); }
            set { To = Split(value); }
        }

        private static string Join(IEnumerable<float> values)
        {
            return string.Join(" ", values.Select(f => f.ToString(CultureInfo.InvariantCulture)));
        }

        private static IReadOnlyList<float> Split(string value)
        {
            return value
                .Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.ParseFloat()).ToList();
        }
    }
}