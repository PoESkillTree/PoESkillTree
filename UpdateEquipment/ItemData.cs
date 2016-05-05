using System;
using System.Xml.Serialization;

namespace UpdateEquipment
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false, ElementName = "ItemList")]
    public class XmlItemList
    {
        [XmlElement("ItemBase")]
        public XmlItemBase[] ItemBases { get; set; }
    }
    
    [Serializable]
    [XmlType(AnonymousType = true)]
    public class XmlItemBase
    {
        [XmlArrayItem("Stat", IsNullable = false)]
        public XmlStat[] Implicit { get; set; }
        
        [XmlArrayItem("Stat", IsNullable = false)]
        public XmlStat[] Properties { get; set; }
        
        [XmlAttribute]
        public ItemType ItemType { get; set; }
        
        [XmlAttribute]
        public string Name { get; set; }
        
        [XmlAttribute]
        public int Level { get; set; }
    }
}