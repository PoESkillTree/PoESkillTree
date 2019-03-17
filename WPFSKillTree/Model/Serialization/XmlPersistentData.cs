using System.Collections.Generic;
using System.Xml.Serialization;
using PoESkillTree.Controls;

namespace PoESkillTree.Model.Serialization
{
    public class XmlLeagueStash
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlElement]
        public List<StashBookmark> Bookmarks { get; set; }
    }

    /// <summary>
    /// Represents the information of <see cref="IPersistentData"/> as stored in PersistentData.xml
    /// </summary>
    [XmlRoot("PersistentData")]
    public class XmlPersistentData
    {
        [XmlElement]
        public string AppVersion { get; set; }

        [XmlElement]
        public Options Options { get; set; }

        [XmlElement]
        public string CurrentBuildPath { get; set; }

        [XmlElement]
        public string SelectedBuildPath { get; set; }

        [XmlArray]
        public List<StashBookmark> StashBookmarks { get; set; }

        [XmlArray]
        public List<XmlLeagueStash> LeagueStashes { get; set; }
    }
}