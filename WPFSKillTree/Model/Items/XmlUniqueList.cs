using System.Xml.Serialization;

namespace POESKillTree.Model.Items
{
    // Contains the classes that allow serialization and deserialization of Uniques.xml

    [XmlRoot(Namespace = "", IsNullable = false, ElementName = "UniqueList")]
    public class XmlUniqueList
    {
        [XmlElement("Unique")]
        public XmlUnique[] Uniques { get; set; }
    }

    public class XmlUnique
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public int Level { get; set; }

        [XmlAttribute]
        public bool DropDisabled { get; set; }

        public bool ShouldSerializeDropDisabled() => DropDisabled;

        [XmlAttribute]
        public string BaseMetadataId { get; set; }

        public string[] Explicit { get; set; }

        public string[] Properties { get; set; }

    }
}