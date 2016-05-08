using System.Xml.Serialization;

namespace POESKillTree.Model.Items
{
    // Contains the classes that allow serialization and deserialization of ItemList.xml

    [XmlRoot(Namespace = "", IsNullable = false, ElementName = "ItemList")]
    public class XmlItemList
    {
        [XmlElement("ItemBase")]
        public XmlItemBase[] ItemBases { get; set; }
    }
    
    public class XmlItemBase
    {
        [XmlArrayItem("Stat")]
        public XmlStat[] Implicit { get; set; }
        
        [XmlArrayItem("Stat")]
        public XmlStat[] Properties { get; set; }
        
        [XmlAttribute]
        public ItemType ItemType { get; set; }
        
        [XmlAttribute]
        public string Name { get; set; }
        
        [XmlAttribute]
        public int Level { get; set; }
    }
}