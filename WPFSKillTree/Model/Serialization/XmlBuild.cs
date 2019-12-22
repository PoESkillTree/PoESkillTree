using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using PoESkillTree.Model.Builds;

namespace PoESkillTree.Model.Serialization
{
    /// <summary>
    /// Represents the information of a <see cref="Builds.PoEBuild"/> instance as stored in
    /// *.pbuild files.
    /// </summary>
    [XmlRoot("PoEBuild")]
    public class XmlBuild
    {
        public string Name { get; set; } = default!;

        public string? Note { get; set; }

        public string? CharacterName { get; set; }

        public string? AccountName { get; set; }

        public string? League { get; set; }

        public Realm Realm { get; set; }

        public int Level { get; set; }

        [XmlElement("Url")]
        public string TreeUrl { get; set; } = default!;

        public string? ItemData { get; set; }

        public DateTime LastUpdated { get; set; }

        public List<string[]> CustomGroups { get; set; } = default!;

        public BanditSettings Bandits { get; set; } = default!;

        public List<ushort> CheckedNodeIds { get; set; } = default!;

        public List<ushort> CrossedNodeIds { get; set; } = default!;

        public List<(string, double?)> ConfigurationStats { get; set; } = default!;

        public string AdditionalData { get; set; } = default!;

        public string Version { get; set; } = default!;
    }
}