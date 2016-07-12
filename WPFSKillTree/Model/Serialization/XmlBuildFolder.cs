using System.Collections.Generic;
using System.Xml.Serialization;

namespace POESKillTree.Model.Serialization
{
    [XmlRoot("BuildFolder")]
    public class XmlBuildFolder
    {
        [XmlElement]
        public string Version { get; set; }

        [XmlElement]
        public bool IsExpanded { get; set; }

        [XmlArray]
        public List<string> Builds { get; set; }
    }
}