using System.Xml.Serialization;

namespace POESKillTree.Model.Serialization
{
    /// <summary>
    /// Only used to get the AppVersion stored in PersistentData.xml.
    /// </summary>
    [XmlRoot("PersistentData")]
    public class XmlPersistentDataVersion
    {
        [XmlElement]
        public string AppVersion { get; set; }
    }
}