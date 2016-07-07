using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using log4net;
using POESKillTree.Controls;
using POESKillTree.Utils.Extensions;

namespace POESKillTree.Model.Serialization
{
    /// <summary>
    /// Deserializes PersistentData using the old build saving structure and created without the versioning refactoring.
    /// Latest applicable release is 2.2.10. All commits after the release and before the versioning refactoring have
    /// used 2.2.10 as AppVersion.
    /// </summary>
    public class PersistentDataDeserializerUpTo2210 : AbstractPersistentDataDeserializer
    {
        [XmlRoot("PersistentData")]
        public class XmlPersistentData
        {
            [XmlElement]
            public Options Options { get; set; }

            [XmlElement]
            public PoEBuild CurrentBuild { get; set; }

            [XmlArray]
            public List<StashBookmark> StashBookmarks { get; set; }

            [XmlArray]
            public List<PoEBuild> Builds { get; set; }
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof(PersistentDataDeserializerUpTo2210));

        public PersistentDataDeserializerUpTo2210()
            : base(null, "2.2.10")
        {
        }

        protected override void DeserializePersistentDataFile(string xmlString)
        {
            var obj = SerializationUtils.DeserializeStringAs<XmlPersistentData>(xmlString);
            PersistentData.Options = obj.Options;
            PersistentData.CurrentBuild = obj.CurrentBuild ?? CreateDefaultCurrentBuild();
            obj.StashBookmarks?.ForEach(PersistentData.StashBookmarks.Add);
            PersistentData.RootBuild.Builds.AddRange(obj.Builds);

            if (!SelectCurrentBuildByName(PersistentData.CurrentBuild.Name))
            {
                PersistentData.RootBuild.Builds.Add(PersistentData.CurrentBuild);
            }
            PersistentData.RootBuild.Builds.Select(b => b as PoEBuild).Where(b => b != null).ForEach(b => b.KeepChanges());

            ImportLegacySavedBuilds();
        }

        private bool SelectCurrentBuildByName(string name)
        {
            var buildNameMatch =
                (from PoEBuild build in PersistentData.RootBuild.BuildsPreorder()
                    where build.Name == name
                    select build).FirstOrDefault();
            if (buildNameMatch == null)
                return false;
            PersistentData.CurrentBuild = buildNameMatch;
            return true;
        }

        /// <summary>
        /// Import builds from legacy build save file "savedBuilds" to PersistentData.xml.
        /// Warning: This will remove the "savedBuilds"
        /// </summary>
        private void ImportLegacySavedBuilds()
        {
            if (!File.Exists("savedBuilds"))
                return;
            try
            {
                var builds = File.ReadAllText("savedBuilds").Split('\n');
                foreach (var b in builds)
                {
                    var description = b.Split(';')[0].Split('|')[1];
                    var poeClass = description.Split(',')[0].Trim();
                    var pointsUsed = description.Split(',')[1].Trim().Split(' ')[0].Trim();

                    var build = new PoEBuild
                    {
                        Name = b.Split(';')[0].Split('|')[0],
                        Class = poeClass
                    };
                    uint points;
                    pointsUsed.TryParseUint(out points);
                    build.PointsUsed = points;
                    if (HasBuildNote(b))
                    {
                        build.TreeUrl = b.Split(';')[1].Split('|')[0];
                        build.Note = b.Split(';')[1].Split('|')[1];
                    }
                    else
                    {
                        build.TreeUrl = b.Split(';')[1];
                    }
                    PersistentData.RootBuild.Builds.Add(build);
                }
                File.Move("savedBuilds", "savedBuilds.old");
            }
            catch (Exception e)
            {
                Log.Error("Could not load legacy savedBuilds file", e);
            }
        }

        private static bool HasBuildNote(string b)
        {
            var buildNoteTest = b.Split(';')[1].Split('|');
            return buildNoteTest.Length > 1;
        }
    }
}