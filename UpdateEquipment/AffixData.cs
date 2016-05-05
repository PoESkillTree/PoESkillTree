using System;
using System.Xml.Serialization;

namespace UpdateEquipment
{
    public enum ModType
    {
        Prefix,
        Suffix
    }

    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false, ElementName = "AffixList")]
    public class XmlAffixList
    {
        [XmlElement("Affix")]
        public XmlAffix[] Affixes { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true)]
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

    [Serializable]
    [XmlType(AnonymousType = true)]
    public class XmlTier
    {
        [XmlElement("Stat")]
        public XmlStat[] Stats { get; set; }

        [XmlAttribute]
        public int ItemLevel { get; set; }

        [XmlAttribute]
        public string Name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true)]
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