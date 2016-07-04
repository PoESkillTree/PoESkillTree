using System.Collections.Generic;
using System.Xml.Serialization;
using POESKillTree.Controls;

namespace POESKillTree.Model.Serialization
{
    public class XmlLeagueStash
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlElement]
        public List<StashBookmark> Bookmarks { get; set; }
    }

    [XmlRoot("PersistentData")]
    public class XmlPersistentData
    {
        [XmlElement]
        public string AppVersion { get; set; }

        [XmlElement]
        public Options Options { get; set; }

        [XmlElement]
        public PoEBuild CurrentBuild { get; set; }

        [XmlElement]
        public PoEBuild SelectedBuild { get; set; } // todo this is new

        [XmlArray]
        public List<StashBookmark> StashBookmarks { get; set; }

        [XmlArray]
        public List<PoEBuild> Builds { get; set; }

        [XmlArray]
        public List<XmlLeagueStash> XmlLeagueStashes { get; set; }
    }
}