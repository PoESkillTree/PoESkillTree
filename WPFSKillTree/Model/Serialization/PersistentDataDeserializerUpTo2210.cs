using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using log4net;
using POESKillTree.Controls;
using POESKillTree.Model.Builds;
using POESKillTree.Utils;
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

        public override void DeserializePersistentDataFile(string xmlString)
        {
            var obj = SerializationUtils.DeserializeString<XmlPersistentData>(xmlString);
            PersistentData.Options = obj.Options;
            PersistentData.CurrentBuild = obj.CurrentBuild ?? CreateDefaultCurrentBuild();
            obj.StashBookmarks?.ForEach(PersistentData.StashBookmarks.Add);
            PersistentData.RootBuild.Builds.AddRange(obj.Builds);

            if (!SelectCurrentBuildByName(PersistentData.CurrentBuild.Name))
            {
                PersistentData.RootBuild.Builds.Add(PersistentData.CurrentBuild);
            }
            PersistentData.RootBuild.Builds.Select(b => b as PoEBuild).Where(b => b != null).ForEach(b => b.KeepChanges());
            RenameBuilds();
        }

        protected override Task DeserializeAdditionalFilesAsync()
        {
            return ImportLegacySavedBuilds();
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

        private void RenameBuilds()
        {
            var builds = PersistentData.RootBuild.Builds;
            var names = builds.ToDictionary(b => b.Name);
            foreach (var build in builds)
            {
                if (names[build.Name] == build)
                    continue;
                build.Name = Util.FindDistinctName(build.Name, names.Keys);
                names[build.Name] = build;
            }
        }

        /// <summary>
        /// Import builds from legacy build save file "savedBuilds" to PersistentData.xml.
        /// Warning: This will remove the "savedBuilds"
        /// </summary>
        private async Task ImportLegacySavedBuilds()
        {
            if (!File.Exists("savedBuilds"))
                return;
            try
            {
                var text = await FileEx.ReadAllTextAsync("savedBuilds");
                foreach (var b in text.Split('\n'))
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