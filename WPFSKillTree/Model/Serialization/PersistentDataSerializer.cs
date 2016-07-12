using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using Newtonsoft.Json.Linq;
using POESKillTree.Controls;
using POESKillTree.Model.Builds;
using POESKillTree.Utils;
using POESKillTree.Utils.Extensions;

using static POESKillTree.Model.Serialization.SerializationConstants;

namespace POESKillTree.Model.Serialization
{
    public class PersistentDataSerializer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PersistentDataSerializer));

        private readonly IPersistentData _persistentData;

        private readonly Dictionary<IBuild, BuildFolder> _parents = new Dictionary<IBuild, BuildFolder>();
        private readonly Dictionary<IBuild, string> _names = new Dictionary<IBuild, string>();
        private readonly Dictionary<IBuild, string> _markedForDeletion = new Dictionary<IBuild, string>();

        public PersistentDataSerializer(IPersistentData persistentData)
        {
            _persistentData = persistentData;
            InitParents();
            InitNames();
        }

        private void InitParents()
        {
            _parents.Clear();
            TreeTraverse((c, p) => _parents[c] = p, _persistentData.RootBuild, null);
        }

        private void InitNames()
        {
            _names.Clear();
            TreeTraverse(b => _names[b] = b.Name, _persistentData.RootBuild);
        }

        private static void TreeTraverse(Action<IBuild, BuildFolder> action, IBuild current, BuildFolder parent)
        {
            action(current, parent);
            var folder = current as BuildFolder;
            folder?.Builds.ForEach(b => TreeTraverse(action, b, folder));
        }

        private static void TreeTraverse(Action<IBuild> action, IBuild current)
        {
            action(current);
            var folder = current as BuildFolder;
            folder?.Builds.ForEach(b => TreeTraverse(action, b));
        }

        public void Serialize(string filePath)
        {
            var stashes = new List<XmlLeagueStash>(_persistentData.LeagueStashes.Select(
                p => new XmlLeagueStash { Name = p.Key, Bookmarks = new List<StashBookmark>(p.Value) }));
            var xmlPersistentData = new XmlPersistentData
            {
                AppVersion = SerializationUtils.AssembylFileVersion,
                CurrentBuildPath = PathFor(_persistentData.CurrentBuild, false),
                Options = _persistentData.Options,
                SelectedBuildPath = PathFor(_persistentData.SelectedBuild, false),
                StashBookmarks = _persistentData.StashBookmarks.ToList(),
                LeagueStashes = stashes
            };
            SerializationUtils.Serialize(xmlPersistentData, filePath);
            SerializeStash();
        }

        private string PathFor(IBuild build, bool asFilePath)
        {
            if (build == null)
                return null;
            if (build == _persistentData.RootBuild)
                return asFilePath ? Path.Combine(_persistentData.Options.BuildsSavePath) : "";

            var path = asFilePath ? SerializationUtils.EncodeFileName(_names[build]) : _names[build];
            var parent = _parents[build];
            while (parent != _persistentData.RootBuild)
            {
                path = asFilePath
                    ? Path.Combine(SerializationUtils.EncodeFileName(_names[parent]), path)
                    : _names[parent] + "/" + path;
                parent = _parents[parent];
            }
            return asFilePath ? Path.Combine(_persistentData.Options.BuildsSavePath, path) : path;
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

        // Folders have to be serialized right after being changed. Else _parents gets incorrect.
        // The folder builds are moved to has to be serialized before their old folder.
        public void SerializeBuild(IBuild build)
        {
            string oldName;
            _names.TryGetValue(build, out oldName);
            var name = build.Name;
            _names[build] = name;

            var path = PathFor(build, true);
            var oldPath = path.Remove(path.Length - SerializationUtils.EncodeFileName(name).Length)
                + SerializationUtils.EncodeFileName(oldName);
            var poeBuild = build as PoEBuild;
            if (poeBuild != null)
            {
                SerializeBuild(path, poeBuild);
                if (oldName != null && oldName != name)
                {
                    File.Delete(oldPath + BuildFileExtension);
                }
            }
            else
            {
                if (oldName != null && oldName != name)
                {
                    Directory.Move(oldPath, path);
                }
                var folder = (BuildFolder)build;

                // Move builds that are in folder.Builds but have _parents set to another folder
                // from their old folder to this one.
                foreach (var b in folder.Builds)
                {
                    BuildFolder parent;
                    _parents.TryGetValue(b, out parent);
                    if (_markedForDeletion.ContainsKey(b) || (parent != null && parent != folder))
                    {
                        var isBuild = b is PoEBuild;
                        var extension = isBuild ? BuildFileExtension : "";
                        string old;
                        if (_markedForDeletion.ContainsKey(b))
                        {
                            old = _markedForDeletion[b];
                            _markedForDeletion.Remove(b);
                        }
                        else
                        {
                            old = PathFor(b, true) + extension;
                        }

                        var newPath = Path.Combine(path,
                            SerializationUtils.EncodeFileName(_names[b]) + extension);
                        if (old == newPath)
                            continue;
                        if (isBuild)
                        {
                            File.Move(old, newPath);
                        }
                        else
                        {
                            Directory.Move(old, newPath);
                        }
                    }
                }

                // Mark builds that have folder as _parents entry but are no longer in folder.Builds.
                // These will either be moved when their new folder is saved or deleted when Delete is called.
                // Skip unsaved builds (those are not contained in _names)
                foreach (var parentsEntry in _parents)
                {
                    var b = parentsEntry.Key;
                    if (parentsEntry.Value != folder
                        || !_names.ContainsKey(b)
                        || folder.Builds.Contains(b))
                        continue;
                    var extension = b is PoEBuild ? BuildFileExtension : "";
                    _markedForDeletion[b] = PathFor(b, true) + extension;
                }

                var xmlFolder = new XmlBuildFolder
                {
                    Version = BuildVersion.ToString(),
                    IsExpanded = folder.IsExpanded,
                    Builds = folder.Builds.Select(b => b.Name).ToList()
                };
                Directory.CreateDirectory(path);
                SerializationUtils.Serialize(xmlFolder, Path.Combine(path, BuildFolderFileName));
            }

            // Just recreate these. Easier than handling all edge cases.
            InitParents();
        }

        public void SerializeAllBuilds()
        {
            InitParents();
            InitNames();
            _markedForDeletion.Clear();

            var path = _persistentData.Options.BuildsSavePath;
            var tmpPath = path + "Tmp";
            try
            {
                DirectoryEx.DeleteIfExists(tmpPath, true);
                SerializeRecursive(tmpPath, _persistentData.RootBuild);
            }
            catch (Exception e)
            {
                Log.Error($"Could not save builds to {tmpPath}", e);
                DirectoryEx.DeleteIfExists(tmpPath, true);
                return;
            }
            DirectoryEx.MoveOverwriting(tmpPath, path);
        }

        public void DeleteBuild(IBuild build)
        {
            // Return if build was not yet saved to the filesystem.
            if (!_names.ContainsKey(build))
                return;
            string path;
            if (!_markedForDeletion.TryGetValue(build, out path))
            {
                throw new ArgumentException(
                    "The build must have been removed from its parent folder and its parent folder must have been saved",
                    nameof(build));
            }
            if (build is PoEBuild)
                File.Delete(path);
            else
                Directory.Delete(path, true);
            _markedForDeletion.Remove(build);
            _names.Remove(build);
        }

        private static void SerializeRecursive(string path, BuildFolder folder)
        {
            var xmlFolder = new XmlBuildFolder
            {
                Version = BuildVersion.ToString(),
                IsExpanded = folder.IsExpanded,
                Builds = folder.Builds.Select(b => b.Name).ToList()
            };
            Directory.CreateDirectory(path);
            SerializationUtils.Serialize(xmlFolder, Path.Combine(path, BuildFolderFileName));

            foreach (var build in folder.Builds)
            {
                var buildPath = Path.Combine(path, SerializationUtils.EncodeFileName(build.Name));
                var b = build as PoEBuild;
                if (b != null)
                    SerializeBuild(buildPath, b);
                else
                    SerializeRecursive(buildPath, (BuildFolder) build);
            }
        }

        private static void SerializeBuild(string path, PoEBuild build)
        {
            build.Version = BuildVersion.ToString();
            SerializationUtils.Serialize(build, path + BuildFileExtension);
            build.KeepChanges();
        }
    }
}