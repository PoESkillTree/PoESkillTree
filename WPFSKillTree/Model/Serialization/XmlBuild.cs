using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace PoESkillTree.Model.Serialization
{
    /// <summary>
    /// Represents the information of a <see cref="Builds.PoEBuild"/> instance as stored in
    /// *.pbuild files.
    /// </summary>
    [XmlRoot("PoEBuild")]
    public class XmlBuild
    {
        public string Name { get; set; }

        public string Note { get; set; }

        public string CharacterName { get; set; }

        public string AccountName { get; set; }

        public string League { get; set; }

        public int Level { get; set; }

        [XmlElement("Url")]
        public string TreeUrl { get; set; }

        public string ItemData { get; set; }

        public DateTime LastUpdated { get; set; }

        public List<string[]> CustomGroups { get; set; }

        public BanditSettings Bandits { get; set; }

        public List<ushort> CheckedNodeIds { get; set; }

        public List<ushort> CrossedNodeIds { get; set; }

        public List<(string, double?)> ConfigurationStats { get; set; }

        public string AdditionalData { get; set; }

        public string Version { get; set; }
    }
}