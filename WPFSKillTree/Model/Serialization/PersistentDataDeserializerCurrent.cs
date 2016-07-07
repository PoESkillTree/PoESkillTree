using System.Collections.Generic;
using System.Linq;

namespace POESKillTree.Model.Serialization
{
    /// <summary>
    /// Can deserialize PersistentData with the new build saving structure.
    /// </summary>
    public class PersistentDataDeserializerCurrent : AbstractPersistentDataDeserializer
    {
        // 2.2.10 was released as 2.2.10.957, this is for everything after that version
        public PersistentDataDeserializerCurrent()
            : base("2.2.10.958", "999.0")
        {
        }

        protected override void DeserializePersistentDataFile(string xmlString)
        {
            var obj = SerializationUtils.DeserializeStringAs<XmlPersistentData>(xmlString);
            PersistentData.Options = obj.Options;
            obj.StashBookmarks?.ForEach(PersistentData.StashBookmarks.Add);
            obj.LeagueStashes?.ForEach(l => PersistentData.LeagueStashes[l.Name] = l.Bookmarks);
            InitializeRootBuild(obj.Builds);

            var current = BuildForPath(obj.CurrentBuildPath) as PoEBuild;
            if (current == null)
            {
                current = CreateDefaultCurrentBuild();
                PersistentData.RootBuild.Builds.Add(current);
            }
            PersistentData.CurrentBuild = current;
            PersistentData.SelectedBuild = BuildForPath(obj.SelectedBuildPath) as PoEBuild;
        }

        private void InitializeRootBuild(IEnumerable<PoEBuild> builds)
        {
            var folderDict = new Dictionary<string, BuildFolder>();
            foreach (var build in builds)
            {
                var parts = build.Name.Split('/');
                var prefix = "";
                var folder = PersistentData.RootBuild;
                for (var i = 0; i < parts.Length - 1; i++)
                {
                    var upperFolder = folder;
                    prefix += "/" + parts[i];
                    if (!folderDict.TryGetValue(prefix, out folder))
                    {
                        folder = new BuildFolder { Name = parts[i] };
                        upperFolder.Builds.Add(folder);
                        folderDict[prefix] = folder;
                    }
                }
                var b = build.DeepClone();
                b.Name = parts[parts.Length - 1];
                b.KeepChanges();
                folder.Builds.Add(b);
            }
        }

        private IBuild BuildForPath(string path)
        {
            if (path == null)
                return null;
            IBuild build = PersistentData.RootBuild;
            foreach (var part in path.Split('/'))
            {
                var folder = build as BuildFolder;
                if (folder == null)
                    return null;
                build = folder.Builds.FirstOrDefault(child => child.Name == part);
                if (build == null)
                    return null;
            }
            return build;
        }
    }
}