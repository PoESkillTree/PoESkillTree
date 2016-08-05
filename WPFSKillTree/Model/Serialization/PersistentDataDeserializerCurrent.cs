using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        private string _currentBuildPath;
        private string _selectedBuildPath;

        // 2.2.10 was released as 2.2.10.957, this is for everything after that version
        public PersistentDataDeserializerCurrent()
            : base("2.2.10.958", "999.0")
        {
        }

        public override void DeserializePersistentDataFile(string xmlString)
        {
            var obj = SerializationUtils.DeserializeString<XmlPersistentData>(xmlString);
            PersistentData.Options = obj.Options;
            obj.StashBookmarks?.ForEach(PersistentData.StashBookmarks.Add);
            obj.LeagueStashes?.ForEach(l => PersistentData.LeagueStashes[l.Name] = l.Bookmarks);

            _currentBuildPath = obj.CurrentBuildPath;
            _selectedBuildPath = obj.SelectedBuildPath;
        }

        protected override async Task DeserializeAdditionalFilesAsync()
        {
            await DeserializeBuildsAsync();
            var current = BuildForPath(_currentBuildPath) as PoEBuild ?? SelectOrCreateCurrentBuild();
            PersistentData.CurrentBuild = current;
            PersistentData.SelectedBuild = BuildForPath(_selectedBuildPath) as PoEBuild;
        }

        public async Task ReloadBuildsAsync()
        {
            PersistentData.RootBuild.Builds.Clear();
            await DeserializeBuildsAsync();
            var current = SelectOrCreateCurrentBuild();
            PersistentData.CurrentBuild = current;
            PersistentData.SelectedBuild = current;
        }

        private PoEBuild SelectOrCreateCurrentBuild()
        {
            var current = PersistentData.RootBuild.BuildsPreorder().FirstOrDefault();
            if (current == null)
            {
                current = CreateDefaultCurrentBuild();
                current.Name = Util.FindDistinctName(current.Name, PersistentData.RootBuild.Builds.Select(b => b.Name));
                PersistentData.RootBuild.Builds.Add(current);
            }
            return current;
        }

        private async Task DeserializeBuildsAsync()
        {
            var path = PersistentData.Options.BuildsSavePath;
            if (!Directory.Exists(path))
                return;
            var folder = PersistentData.RootBuild;
            var xmlFolder = await DeserializeAsync<XmlBuildFolder>(Path.Combine(path, SerializationConstants.BuildFolderFileName));
            if (xmlFolder != null)
            {
                folder.IsExpanded = xmlFolder.IsExpanded;
                await DeserializeBuildsAsync(path, folder, xmlFolder.Builds);
            }
            else
            {
                await DeserializeBuildsAsync(path, folder, Enumerable.Empty<string>());
            }
        }

        private static async Task DeserializeBuildsAsync(string buildFolderPath, BuildFolder folder, IEnumerable<string> buildNames)
        {
            var builds = new Dictionary<string, IBuild>();
            foreach (var directoryPath in Directory.EnumerateDirectories(buildFolderPath))
            {
                var build = await DeserializeFolderAsync(directoryPath, Path.GetFileName(directoryPath));
                if (build != null)
                    builds[build.Name] = build;
            }
            foreach (var filePath in Directory.EnumerateFiles(buildFolderPath))
            {
                if (Path.GetExtension(filePath) != SerializationConstants.BuildFileExtension)
                    continue;

                var build = await DeserializeBuildAsync(filePath);
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

        private static async Task<BuildFolder> DeserializeFolderAsync(string path, string fileName)
        {
            var folder = new BuildFolder {Name = SerializationUtils.DecodeFileName(fileName)};
            var xmlFolder = await DeserializeAsync<XmlBuildFolder>(Path.Combine(path, SerializationConstants.BuildFolderFileName));
            if (xmlFolder != null && CheckVersion(xmlFolder.Version))
            {
                folder.IsExpanded = xmlFolder.IsExpanded;
                await DeserializeBuildsAsync(path, folder, xmlFolder.Builds);
            }
            else
            {
                await DeserializeBuildsAsync(path, folder, Enumerable.Empty<string>());
            }
            return folder;
        }

        private static async Task<PoEBuild> DeserializeBuildAsync(string path)
        {
            var build = await DeserializeAsync<PoEBuild>(path);
            if (build != null && CheckVersion(build.Version))
            {
                build.KeepChanges();
                return build;
            }

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

        private static async Task<T> DeserializeAsync<T>(string path)
        {
            try
            {
                return await SerializationUtils.DeserializeFileAsync<T>(path);
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