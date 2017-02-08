using System.Xml.Serialization;

namespace UnitTests.TestBuilds.Utils
{
    /// <summary>
    /// Represents xml-serializable default tag.
    /// </summary>
    public class DefaultUrlItem : UrlItem
    {
        [XmlIgnore]
        public override bool IsDefault => true;
    }
}