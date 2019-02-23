using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using PoESkillTree.Utils;
using POESKillTree.Localization;
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

        // 2.3.0 was released as 2.3.0.1052, this is for everything after that version
        public PersistentDataDeserializerCurrent()
            : base("2.3.0.1053", "999.0")
        {
            DeserializesBuildsSavePath = true;
        }

        public override void DeserializePersistentDataFile(string xmlString)
        {
            var obj = XmlSerializationUtils.DeserializeString<XmlPersistentData>(xmlString);
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

        /// <summary>
        /// Clears all builds and deserializes them again.
        /// </summary>
        /// <returns></returns>
        public async Task ReloadBuildsAsync()
        {
            PersistentData.RootBuild.Builds.Clear();
            await DeserializeBuildsAsync();
            var current = SelectOrCreateCurrentBuild();
            PersistentData.CurrentBuild = current;
            PersistentData.SelectedBuild = current;
        }

        /// <summary>
        /// Imports the build located at <paramref name="buildPath"/>. <see cref="IPersistentData.SaveBuild"/> may be
        /// called by this method.
        /// </summary>
        public async Task ImportBuildFromFileAsync(string buildPath)
        {
            const string extension = SerializationConstants.BuildFileExtension;
            if (!File.Exists(buildPath) || Path.GetExtension(buildPath) != extension)
            {
                Log.Error($"Could not import build file from {buildPath}");
                var message = string.Format(
                    L10n.Message(
                        "Could not import build file, only existing files with the extension {0} can be imported."),
                    extension);
                await DialogCoordinator.ShowErrorAsync(PersistentData, message, title: L10n.Message("Import failed"));
                return;
            }

            var unifiedBuildsSavePath = PersistentData.Options.BuildsSavePath.Replace(Path.AltDirectorySeparatorChar,
                Path.DirectorySeparatorChar);
            var unifiedPath = buildPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            if (unifiedPath.StartsWith(unifiedBuildsSavePath))
            {
                // If BuildsSavePath is part of buildPath, just set it as current and selected build
                // Remove BuildsSavePath
                var relativePath = unifiedPath.Remove(0, unifiedBuildsSavePath.Length + 1);
                // Remove extension
                var path = relativePath.Remove(relativePath.Length - extension.Length);
                var build = BuildForPath(path) as PoEBuild;
                if (build == null)
                {
                    Log.Warn($"Import failed, build with path {path} not found");
                    return;
                }

                PersistentData.CurrentBuild = build;
                PersistentData.SelectedBuild = build;
            }
            else
            {
                // Else, proper import
                var build = await ImportBuildAsync(
                    async () => await XmlSerializationUtils.DeserializeFileAsync<XmlBuild>(buildPath));
                if (build != null)
                {
                    build.Name = Util.FindDistinctName(build.Name, PersistentData.RootBuild.Builds.Select(b => b.Name));
                    PersistentData.RootBuild.Builds.Add(build);
                    PersistentData.CurrentBuild = build;
                    PersistentData.SelectedBuild = build;
                    PersistentData.SaveBuild(PersistentData.RootBuild);
                }
            }
        }

        /// <summary>
        /// Imports the build given as a xml string. <see cref="IPersistentData.SaveBuild"/> may be
        /// called by this method.
        /// </summary>
        public Task<PoEBuild> ImportBuildFromStringAsync(string buildXml)
        {
            return ImportBuildAsync(
                () => Task.FromResult(XmlSerializationUtils.DeserializeString<XmlBuild>(buildXml)));
        }

        private async Task<PoEBuild> ImportBuildAsync(Func<Task<XmlBuild>> supplier)
        {
            try
            {
                var xmlBuild = await supplier();
                var build = ConvertFromXmlBuild(xmlBuild);
                if (!CheckVersion(xmlBuild.Version))
                {
                    Log.Warn($"Build is of an old version and can't be imported (version {xmlBuild.Version})");
                    await DialogCoordinator.ShowWarningAsync(PersistentData,
                        L10n.Message("Build is of an old version and can't be imported"),
                        title: L10n.Message("Import failed"));
                    return null;
                }
                return build;
            }
            catch (Exception e)
            {
                Log.Error("Error while importing build", e);
                await DialogCoordinator.ShowErrorAsync(PersistentData, L10n.Message("Could not import build"),
                        e.Message, L10n.Message("Import failed"));
                return null;
            }
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
            await DeserializeFolderAsync(path, PersistentData.RootBuild);
        }

        private static async Task DeserializeBuildsAsync(string buildFolderPath, BuildFolder folder, IEnumerable<string> buildNames)
        {
            var buildTasks = new List<Task<IBuild>>();
            foreach (var directoryPath in Directory.EnumerateDirectories(buildFolderPath))
            {
                var fileName = Path.GetFileName(directoryPath);
                var build = new BuildFolder { Name = SerializationUtils.DecodeFileName(fileName) };
                buildTasks.Add(DeserializeFolderAsync(directoryPath, build));
            }
            foreach (var filePath in Directory.EnumerateFiles(buildFolderPath))
            {
                if (Path.GetExtension(filePath) != SerializationConstants.BuildFileExtension)
                    continue;
                buildTasks.Add(DeserializeBuildAsync(filePath));
            }

            var builds = new Dictionary<string, IBuild>();
            foreach (var buildTask in buildTasks)
            {
                var build = await buildTask;
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

        private static async Task<IBuild> DeserializeFolderAsync(string path, BuildFolder folder)
        {
            var fullPath = Path.Combine(path, SerializationConstants.BuildFolderFileName);
            XmlBuildFolder xmlFolder;
            if (File.Exists(fullPath))
            {
                xmlFolder = await DeserializeAsync<XmlBuildFolder>(fullPath);
            }
            else
            {
                xmlFolder = null;
                Log.Info($"Build folder file {fullPath} does not exist. Contained builds will be ordered arbitrarily.");
            }
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

        private static async Task<IBuild> DeserializeBuildAsync(string path)
        {
            var xmlBuild = await DeserializeAsync<XmlBuild>(path).ConfigureAwait(false);
            var build = ConvertFromXmlBuild(xmlBuild);
            if (build != null && CheckVersion(xmlBuild.Version))
            {
                build.KeepChanges();
                return build;
            }

            var backupPath = path + ".bad";
            Log.Warn($"Moving build file {path} to {backupPath} as it could not be deserialized");
            FileUtils.DeleteIfExists(backupPath);
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
                return await XmlSerializationUtils.DeserializeFileAsync<T>(path, true).ConfigureAwait(false);
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
            foreach (var part in path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            {
                var folder = build as BuildFolder;
                if (folder == null)
                    return null;
                var name = SerializationUtils.DecodeFileName(part);
                build = folder.Builds.FirstOrDefault(child => child.Name == name);
                if (build == null)
                    return null;
            }
            return build;
        }
    }
}