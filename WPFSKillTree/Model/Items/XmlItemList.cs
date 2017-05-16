using System.Xml.Serialization;
using POESKillTree.Model.Items.Enums;

namespace POESKillTree.Model.Items
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
        [XmlArrayItem("Mod")]
        public XmlMod[] Implicit { get; set; }
        
        [XmlArrayItem("Property")]
        public XmlProperty[] Properties { get; set; }
        
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


    public class XmlProperty
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public float From { get; set; }

        [XmlAttribute]
        public float To { get; set; }
    }


    public class XmlMod
    {
        [XmlAttribute]
        public string Id { get; set; }
    }
}