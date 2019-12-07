using System.Collections.Generic;
using System.Xml.Serialization;

namespace PoESkillTree.Model.Serialization
{
    /// <summary>
    /// Represents the information of a <see cref="Model.Builds.BuildFolder"/> instance as stored in
    /// .buildfolder files.
    /// </summary>
    [XmlRoot("BuildFolder")]
    public class XmlBuildFolder
    {
        [XmlElement]
        public string Version { get; set; } = default!;

        [XmlElement]
        public bool IsExpanded { get; set; }

        [XmlArray]
        public List<string> Builds { get; set; } = default!;
    }
}