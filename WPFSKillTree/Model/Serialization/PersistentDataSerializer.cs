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
            var builds = persistentData.RootBuild.Builds.SelectMany(b => FlattenBuilds(b)).ToList();
            var xmlPersistentData = new XmlPersistentData
            {
                AppVersion = SerializationUtils.GetAssemblyFileVersion(),
                Builds = builds,
                CurrentBuildPath = PathFor(persistentData.CurrentBuild),
                Options = persistentData.Options,
                SelectedBuildPath = PathFor(persistentData.SelectedBuild),
                StashBookmarks = persistentData.StashBookmarks.ToList(),
                LeagueStashes = stashes
            };
            SerializationUtils.Serialize(xmlPersistentData, filePath);
            SerializeStash();
        }

        private static IEnumerable<PoEBuild> FlattenBuilds(IBuild build, string parentNames = null)
        {
            var list = new List<PoEBuild>();
            var prefix = string.IsNullOrEmpty(parentNames) ? "" : parentNames + "/";
            var b = build as PoEBuild;
            if (b != null)
            {
                b = b.DeepClone();
                b.Name = prefix + b.Name;
                list.Add(b);
            }
            else
            {
                var folder = (BuildFolder)build;
                foreach (var child in folder.Builds)
                {
                    list.AddRange(FlattenBuilds(child, prefix + folder.Name));
                }
            }
            return list;
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
    }
}