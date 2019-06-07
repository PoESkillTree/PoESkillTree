using System.Xml.Serialization;
using PoESkillTree.GameModel.Items;

namespace PoESkillTree.Model.Items
{
    // Contains the classes that allow serialization and deserialization of Items.xml

    [XmlRoot(Namespace = "", IsNullable = false, ElementName = "ItemList")]
    public class XmlItemList
    {
        [XmlElement("ItemBase")]
        public XmlItemBase[] ItemBases { get; set; }
    }
    

    public class XmlItemBase
    {
        public string[] Implicit { get; set; }

        public string[] Properties { get; set; }
        
        [XmlAttribute]
        public ItemClass ItemClass { get; set; }

        [XmlAttribute]
        public Tags Tags { get; set; }
        
        [XmlAttribute]
        public string Name { get; set; }
        
        [XmlAttribute]
        public int Level { get; set; }

        [XmlAttribute]
        public int Strength { get; set; }

        [XmlAttribute]
        public int Dexterity { get; set; }

        [XmlAttribute]
        public int Intelligence { get; set; }

        [XmlAttribute]
        public bool DropDisabled { get; set; }

        public bool ShouldSerializeDropDisabled() => DropDisabled;

        [XmlAttribute]
        public int InventoryHeight { get; set; }

        [XmlAttribute]
        public int InventoryWidth { get; set; }

        [XmlAttribute]
        public string MetadataId { get; set; }
    }
}