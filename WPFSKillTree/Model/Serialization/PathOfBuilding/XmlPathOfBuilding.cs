using System.Collections.Generic;
using System.Xml.Serialization;
using PoESkillTree.Engine.GameModel;

namespace PoESkillTree.Model.Serialization.PathOfBuilding
{
    [XmlRoot("PathOfBuilding")]
    public class XmlPathOfBuilding
    {
        public XmlPathOfBuildingBuild Build { get; set; } = default!;
        public XmlPathOfBuildingImport Import { get; set; } = default!;
        public XmlPathOfBuildingSkills Skills { get; set; } = default!;
        public XmlPathOfBuildingTree Tree { get; set; } = default!;
        public string? Notes { get; set; }
        public XmlPathOfBuildingItems Items { get; set; } = default!;
        public List<XmlPathOfBuildingConfigInput> Config { get; } = new List<XmlPathOfBuildingConfigInput>();
    }

    public class XmlPathOfBuildingBuild
    {
        [XmlAttribute("level")]
        public int Level { get; set; }
        [XmlAttribute("bandit")]
        public Bandit Bandit { get; set; }
        [XmlAttribute("className")]
        public string ClassName { get; set; } = default!;
        [XmlAttribute("ascendClassName")]
        public string AscendancyClassName { get; set; } = default!;
        [XmlAttribute("mainSocketGroup")]
        public int MainSocketGroup { get; set; }
    }

    public class XmlPathOfBuildingImport
    {
        [XmlAttribute("lastAccountHash")]
        public string? LastAccountHash { get; set; }
        [XmlAttribute("lastRealm")]
        public string? LastRealm { get; set; }
        [XmlAttribute("lastCharacterName")]
        public string? LastCharacterHash { get; set; }
    }

    public class XmlPathOfBuildingSkills
    {
        [XmlElement("Skill")]
        public List<XmlPathOfBuildingSkill> Skills { get; } = new List<XmlPathOfBuildingSkill>();
    }

    public class XmlPathOfBuildingSkill
    {
        [XmlAttribute("enabled")]
        public bool Enabled { get; set; }
        [XmlAttribute("slot")]
        public string? Slot { get; set; }
        [XmlElement("Gem")]
        public List<XmlPathOfBuildingGem> Gems { get; } = new List<XmlPathOfBuildingGem>();
    }

    public class XmlPathOfBuildingGem
    {
        [XmlAttribute("quality")]
        public int Quality { get; set; }
        [XmlAttribute("level")]
        public int Level { get; set; }
        [XmlAttribute("gemId")]
        public string MetadataId { get; set; } = default!;
        [XmlAttribute("enabled")]
        public bool Enabled { get; set; }
    }

    public class XmlPathOfBuildingTree
    {
        [XmlAttribute("activeSpec")]
        public int ActiveSpec { get; set; }
        [XmlElement("Spec")]
        public List<XmlPathOfBuildingTreeSpec> Specs { get; } = new List<XmlPathOfBuildingTreeSpec>();
    }

    public class XmlPathOfBuildingTreeSpec
    {
        [XmlAttribute("title")]
        public string? Title { get; set; }
        [XmlAttribute("treeVersion")]
        public string? TreeVersion { get; set; }
        [XmlElement("URL")]
        public string Url { get; set; } = default!;
        public List<XmlPathOfBuildingTreeSocket> Sockets { get; } = new List<XmlPathOfBuildingTreeSocket>();
    }

    [XmlType(TypeName = "Socket")]
    public class XmlPathOfBuildingTreeSocket
    {
        [XmlAttribute("nodeId")]
        public int NodeId { get; set; }
        [XmlAttribute("itemId")]
        public int ItemId { get; set; }
    }

    public class XmlPathOfBuildingItems
    {
        [XmlElement("Item")]
        public List<XmlPathOfBuildingItem> Items { get; } = new List<XmlPathOfBuildingItem>();
        [XmlElement("Slot")]
        public List<XmlPathOfBuildingSlot> Slots { get; } = new List<XmlPathOfBuildingSlot>();
    }

    public class XmlPathOfBuildingItem
    {
        [XmlAttribute("id")]
        public int Id { get; set; }
        [XmlAttribute("variant")]
        public int Variant { get; set; }
        [XmlAttribute("variantAlt")]
        public int VariantAlt { get; set; }
        [XmlText]
        public string Data { get; set; } = default!;
    }

    public class XmlPathOfBuildingSlot
    {
        [XmlAttribute("name")]
        public string Name { get; set; } = default!;
        [XmlAttribute("active")]
        public bool Active { get; set; }
        [XmlAttribute("itemId")]
        public int ItemId { get; set; }
    }
    
    [XmlType(TypeName = "Input")]
    public class XmlPathOfBuildingConfigInput
    {
        [XmlAttribute("name")]
        public string Name { get; set; } = default!;
        [XmlAttribute("boolean")]
        public bool Boolean { get; set; }
        public bool BooleanSpecified { get; set; }
        [XmlAttribute("number")]
        public double Number { get; set; }
        public bool NumberSpecified { get; set; }
        [XmlAttribute("string")]
        public string? String { get; set; }
    }
}