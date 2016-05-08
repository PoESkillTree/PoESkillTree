using System.Xml.Serialization;

namespace POESKillTree.Model.Items
{
    // Contains the classes that allow serialization and deserialization of AffixList.xml

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
        public string CraftedAs { get; set; }
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
    }

    public class XmlStat
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public float From { get; set; }

        [XmlAttribute]
        public float To { get; set; }
    }
}