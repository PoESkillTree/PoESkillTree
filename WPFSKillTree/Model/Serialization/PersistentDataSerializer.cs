using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using Newtonsoft.Json.Linq;
using POESKillTree.Controls;
using POESKillTree.Utils;

namespace POESKillTree.Model.Serialization
{
    public class PersistentDataSerializer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PersistentDataSerializer));

        private IPersistentData _persistentData;

        public void Serialize(IPersistentData persistentData, string filePath)
        {
            _persistentData = persistentData;

            var stashes = new List<XmlLeagueStash>(persistentData.LeagueStashes.Select(
                p => new XmlLeagueStash { Name = p.Key, Bookmarks = new List<StashBookmark>(p.Value) }));
            var xmlPersistentData = new XmlPersistentData
            {
                AppVersion = SerializationUtils.GetAssemblyFileVersion(),
                CurrentBuildPath = PathFor(persistentData.CurrentBuild),
                Options = persistentData.Options,
                SelectedBuildPath = PathFor(persistentData.SelectedBuild),
                StashBookmarks = persistentData.StashBookmarks.ToList(),
                LeagueStashes = stashes
            };
            SerializationUtils.Serialize(xmlPersistentData, filePath);
            SerializeStash();
        }

        private string PathFor(IBuild build)
        {
            return PathFor(build, _persistentData.RootBuild, "");
        }

        private static string PathFor(IBuild build, BuildFolder parent, string prefix)
        {
            foreach (var child in parent.Builds)
            {
                if (child == build)
                    return prefix + build.Name;
                var folder = child as BuildFolder;
                if (folder != null)
                {
                    var path = PathFor(build, folder, folder.Name + "/");
                    if (path != null)
                        return prefix + path;
                }
            }
            return null;
        }

        private void SerializeStash()
        {
            try
            {
                var arr = new JArray();
                foreach (var item in _persistentData.StashItems)
                {
                    arr.Add(item.JsonBase);
                }

                File.WriteAllText(Path.Combine(AppData.GetFolder(), "stash.json"), arr.ToString());
            }
            catch (Exception e)
            {
                Log.Error("Could not serialize stash", e);
            }
        }

        public void SerializeBuild(IBuild build, IPersistentData persistentData)
        {
            _persistentData = persistentData;
            // todo
            SerializeAllBuilds(persistentData);
        }

        public void SerializeAllBuilds(IPersistentData persistentData)
        {
            _persistentData = persistentData;

            var path = persistentData.Options.BuildsSavePath;
            var tmpPath = path + "Tmp";
            try
            {
                DirectoryEx.DeleteIfExists(tmpPath, true);
                SerializeFolder(tmpPath, persistentData.RootBuild);
            }
            catch (Exception e)
            {
                Log.Error($"Could not save builds to {tmpPath}", e);
                DirectoryEx.DeleteIfExists(tmpPath, true);
                return;
            }
            DirectoryEx.MoveOverwriting(tmpPath, path);
        }

        private static void SerializeFolder(string path, BuildFolder folder)
        {
            var xmlFolder = new XmlBuildFolder
            {
                IsExpanded = folder.IsExpanded,
                Builds = folder.Builds.Select(b => b.Name).ToList()
            };
            Directory.CreateDirectory(path);
            SerializationUtils.Serialize(xmlFolder, Path.Combine(path, SerializationConstants.BuildFolderFileName));
            foreach (var build in folder.Builds)
            {
                var buildPath = Path.Combine(path, SerializationUtils.EncodeFileName(build.Name));
                var b = build as PoEBuild;
                if (b != null)
                    SerializeBuild(buildPath, b);
                else
                    SerializeFolder(buildPath, (BuildFolder) build);
            }
        }

        private static void SerializeBuild(string path, PoEBuild build)
        {
            SerializationUtils.Serialize(build, path + SerializationConstants.BuildFileExtension);
            build.KeepChanges();
        }
    }
}