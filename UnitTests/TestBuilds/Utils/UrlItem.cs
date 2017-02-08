using System.Xml.Serialization;

namespace UnitTests.TestBuilds.Utils
{
    /// <summary>
    /// Represents xml-serializable url tag.
    /// </summary>
    public class UrlItem
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlText]
        public virtual string Value { get; set; }

        [XmlIgnore]
        public virtual bool IsDefault => false;
    }
}