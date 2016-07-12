using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using POESKillTree.Model.Builds;
using POESKillTree.Utils;

namespace POESKillTree.Model.Serialization
{
    /// <summary>
    /// Can deserialize PersistentData with the new build saving structure.
    /// </summary>
    public class PersistentDataDeserializerCurrent : AbstractPersistentDataDeserializer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PersistentDataDeserializerCurrent));

        // 2.2.10 was released as 2.2.10.957, this is for everything after that version
        public PersistentDataDeserializerCurrent()
            : base("2.2.10.958", "999.0")
        {
        }

        protected override void DeserializeWithoutPersistentDataFile()
        {
            DeserializeBuilds();
            // If there are builds saved, take the first one, if not, create a new one.
            var current = PersistentData.RootBuild.BuildsPreorder().FirstOrDefault();
            if (current == null)
            {
                current = CreateDefaultCurrentBuild();
                PersistentData.RootBuild.Builds.Add(current);
            }
            PersistentData.CurrentBuild = current;
        }

        protected override void DeserializePersistentDataFile(string xmlString)
        {
            var obj = SerializationUtils.DeserializeStringAs<XmlPersistentData>(xmlString);
            PersistentData.Options = obj.Options;
            obj.StashBookmarks?.ForEach(PersistentData.StashBookmarks.Add);
            obj.LeagueStashes?.ForEach(l => PersistentData.LeagueStashes[l.Name] = l.Bookmarks);
            DeserializeBuilds();

            var current = BuildForPath(obj.CurrentBuildPath) as PoEBuild;
            if (current == null)
            {
                current = PersistentData.RootBuild.BuildsPreorder().FirstOrDefault();
                if (current == null)
                {
                    current = CreateDefaultCurrentBuild();
                    PersistentData.RootBuild.Builds.Add(current);
                }
            }
            PersistentData.CurrentBuild = current;
            PersistentData.SelectedBuild = BuildForPath(obj.SelectedBuildPath) as PoEBuild;
        }

        private void DeserializeBuilds()
        {
            var path = PersistentData.Options.BuildsSavePath;
            if (!Directory.Exists(path))
                return;
            var folder = PersistentData.RootBuild;
            var xmlFolder = Deserialize<XmlBuildFolder>(Path.Combine(path, SerializationConstants.BuildFolderFileName));
            if (xmlFolder != null)
            {
                folder.IsExpanded = xmlFolder.IsExpanded;
                DeserializeBuilds(path, folder, xmlFolder.Builds);
            }
            else
            {
                DeserializeBuilds(path, folder, Enumerable.Empty<string>());
            }
        }

        private static void DeserializeBuilds(string buildFolderPath, BuildFolder folder, IEnumerable<string> buildNames)
        {
            var builds = new Dictionary<string, IBuild>();
            foreach (var directoryPath in Directory.EnumerateDirectories(buildFolderPath))
            {
                var build = DeserializeFolder(directoryPath, Path.GetFileName(directoryPath));
                if (build != null)
                    builds[build.Name] = build;
            }
            foreach (var filePath in Directory.EnumerateFiles(buildFolderPath))
            {
                if (Path.GetExtension(filePath) != SerializationConstants.BuildFileExtension)
                    continue;

                var build = DeserializeBuild(filePath);
                if (build != null)
                    builds[build.Name] = build;
            }

            // Add the builds ordered by buildNames
            foreach (var buildName in buildNames)
            {
                if (builds.ContainsKey(buildName))
                {
                    folder.Builds.Add(builds[buildName]);
                    builds.Remove(buildName);
                }
            }
            // Add builds not in buildNames last
            foreach (var build in builds.Values)
            {
                folder.Builds.Add(build);
            }
        }

        private static BuildFolder DeserializeFolder(string path, string fileName)
        {
            var folder = new BuildFolder {Name = SerializationUtils.DecodeFileName(fileName)};
            var xmlFolder = Deserialize<XmlBuildFolder>(Path.Combine(path, SerializationConstants.BuildFolderFileName));
            if (xmlFolder != null && CheckVersion(xmlFolder.Version))
            {
                folder.IsExpanded = xmlFolder.IsExpanded;
                DeserializeBuilds(path, folder, xmlFolder.Builds);
            }
            else
            {
                DeserializeBuilds(path, folder, Enumerable.Empty<string>());
            }
            return folder;
        }

        private static PoEBuild DeserializeBuild(string path)
        {
            var build = Deserialize<PoEBuild>(path);
            if (build != null && CheckVersion(build.Version))
                return build;

            var backupPath = path + ".bad";
            Log.Warn($"Moving build file {path} to {backupPath} as it could not be deserialized");
            FileEx.DeleteIfExists(backupPath);
            File.Move(path, backupPath);
            return null;
        }

        private static bool CheckVersion(string buildVersion)
        {
            var version = new Version(buildVersion);
            var compare = SerializationConstants.BuildVersion.CompareTo(version);
            if (compare > 0)
            {
                throw new InvalidOperationException("Build of old version found, a converter needs to be implemented.");
            }
            if (compare < 0)
            {
                Log.Error(
                    "Build has higher version than supported "
                    + $"(version {buildVersion}, supported is {SerializationConstants.BuildVersion})\n");
                return false;
            }
            return true;
        }

        private static T Deserialize<T>(string path)
        {
            try
            {
                return SerializationUtils.DeserializeFileAs<T>(path);
            }
            catch (Exception e)
            {
                Log.Error($"Could not deserialize file from {path} as type {typeof(T)}", e);
                return default(T);
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