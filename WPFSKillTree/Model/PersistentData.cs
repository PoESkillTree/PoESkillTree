using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.ComponentModel;
using JetBrains.Annotations;
using log4net;
using Newtonsoft.Json.Linq;
using POESKillTree.Controls;
using POESKillTree.Model.Items;
using POESKillTree.Model.Serialization;
using POESKillTree.Utils;

namespace POESKillTree.Model
{
    public interface IPersistentData : INotifyPropertyChanged, INotifyPropertyChanging
    {
        IOptions Options { get; }
        PoEBuild CurrentBuild { get; set; }
        PoEBuild SelectedBuild { get; set; }
        BuildFolder RootBuild { get; }
        EquipmentData EquipmentData { get; }
        ObservableCollection<Item> StashItems { get; }
        IDictionary<string, IEnumerable<StashBookmark>> LeagueStashes { get; }

        void SaveToFile();
    }

    public class PersistentData : Notifier, IPersistentData
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PersistentData));

        private string _appVersion;
        public string AppVersion
        {
            get { return _appVersion; }
            set { SetProperty(ref _appVersion, value); }
        }

        private Options _options = new Options();
        public Options Options
        {
            get { return _options; }
            private set { SetProperty(ref _options, value); }
        }

        IOptions IPersistentData.Options { get { return _options; } }

        private PoEBuild _currentBuild;
        public PoEBuild CurrentBuild
        {
            get { return _currentBuild; }
            set { SetProperty(ref _currentBuild, value); }
        }

        private PoEBuild _selectedBuild;
        public PoEBuild SelectedBuild
        {
            get { return _selectedBuild; }
            set { SetProperty(ref _selectedBuild, value); }
        }

        public ObservableCollection<StashBookmark> StashBookmarks { get; } = new ObservableCollection<StashBookmark>();

        public BuildFolder RootBuild { get; } = new BuildFolder {Name = "Root"};

        private const string FileName = "PersistentData";

        public ObservableCollection<Item> StashItems { get; } = new ObservableCollection<Item>();

        public IDictionary<string, IEnumerable<StashBookmark>> LeagueStashes { get; } =
            new Dictionary<string, IEnumerable<StashBookmark>>();

        private EquipmentData _equipmentData;
        public EquipmentData EquipmentData
        {
            get { return _equipmentData ?? (_equipmentData = new EquipmentData(Options)); }
        }

        public PersistentData(bool loadFromFile)
        {
            if (loadFromFile)
                LoadFromFile(AppData.GetFolder(true) + FileName + ".xml");
            if (CurrentBuild == null)
            {
                CurrentBuild = new PoEBuild {Name = "New Build"};
            }
            var buildNameMatch =
                (from PoEBuild build in RootBuild.BuildsPreorder()
                 where build.Name == CurrentBuild.Name
                 select build).FirstOrDefault();
            if (buildNameMatch != null)
            {
                CurrentBuild = buildNameMatch;
            }
            else
            {
                RootBuild.Builds.Add(CurrentBuild); // todo Might not be what we want
            }
        }

        // Creates empty file with language option set.
        [UsedImplicitly]
        public static void CreateSetupTemplate(string path, string language)
        {
            var data = new PersistentData(false) {Options = {Language = language}};
            data.SaveToFile(Path.Combine(path, FileName + ".xml"));
        }

        public void SaveToFile()
        {
            SaveToFile(AppData.GetFolder(true) + FileName + ".xml");
        }

        private void SaveToFile(string path)
        {
            if (File.Exists(path))
            {
                string pathBak = AppData.GetFolder(true) + FileName + ".bak";
                if (File.Exists(pathBak))
                    File.Delete(pathBak);
                File.Move(path, pathBak);
            }
            var stashes = new List<XmlLeagueStash>(LeagueStashes.Select(
                p => new XmlLeagueStash { Name = p.Key, Bookmarks = new List<StashBookmark>(p.Value) }));
            var xmlPersistentData = new XmlPersistentData
            {
                AppVersion = AppVersion,
                Builds = RootBuild.Builds.SelectMany(b => FlattenBuilds(b)).ToList(),
                CurrentBuild = CurrentBuild,
                Options = Options,
                SelectedBuild = SelectedBuild,
                StashBookmarks = StashBookmarks.ToList(),
                XmlLeagueStashes = stashes
            };
            var writer = new XmlSerializer(typeof(XmlPersistentData));
            using (var file = new StreamWriter(path, false, System.Text.Encoding.UTF8))
            {
                writer.Serialize(file, xmlPersistentData);
            }
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
                var folder = (BuildFolder) build;
                foreach (var child in folder.Builds)
                {
                    list.AddRange(FlattenBuilds(child, prefix + folder.Name));
                }
            }
            return list;
        }

        private void InitializeRootBuild(IEnumerable<PoEBuild> builds)
        {
            var folderDict = new Dictionary<string, BuildFolder>();
            foreach (var build in builds)
            {
                var parts = build.Name.Split('/');
                var prefix = "";
                var folder = RootBuild;
                for (var i = 0; i < parts.Length - 1; i++)
                {
                    var upperFolder = folder;
                    prefix += "/" + parts[i];
                    if (!folderDict.TryGetValue(prefix, out folder))
                    {
                        folder = new BuildFolder {Name = parts[i]};
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

        private void LoadFromFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var reader = new StreamReader(filePath);
                    var ser = new XmlSerializer(typeof(XmlPersistentData));
                    var obj = (XmlPersistentData) ser.Deserialize(reader);
                    Options = obj.Options;
                    InitializeRootBuild(obj.Builds);
                    CurrentBuild = obj.CurrentBuild;
                    StashBookmarks.Clear();
                    obj.StashBookmarks?.ForEach(StashBookmarks.Add);
                    AppVersion = obj.AppVersion;
                    LeagueStashes.Clear();
                    obj.XmlLeagueStashes?.ForEach(l => LeagueStashes[l.Name] = l.Bookmarks);
                    reader.Close();
                }
                DeserializeStash();
            }
            catch (Exception ex)
            {
                string pathBak = AppData.GetFolder(true) + FileName + ".bak";
                if (!filePath.Contains(FileName + ".bak") && File.Exists(pathBak))
                    LoadFromFile(pathBak);
                else 
                {
                    string pathBad = AppData.GetFolder(true) + FileName + "_Bad.xml";
                    if(File.Exists(pathBad))
                        File.Delete(pathBad);
                    File.Copy(filePath, pathBad);
                    if (File.Exists(AppData.GetFolder(true) + FileName + ".xml"))
                        File.Delete(AppData.GetFolder(true) + FileName + ".xml");
                    if (File.Exists(AppData.GetFolder(true) + FileName + ".bak"))
                        File.Delete(AppData.GetFolder(true) + FileName + ".bak");

                    throw new Exception(ex.Message + "\nYour PersistentData folder could not be loaded correctly. It has been moved to " + pathBad);
                }
            }
        }

        private void SerializeStash()
        {
            try
            {
                var arr = new JArray();
                foreach (var item in StashItems)
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

        private void DeserializeStash()
        {
            try
            {
                StashItems.Clear();
                var file = Path.Combine(AppData.GetFolder(), "stash.json");
                if (!File.Exists(file))
                    return;
                var arr = JArray.Parse(File.ReadAllText(file));
                foreach (var item in arr)
                {
                    var itm = new Item(this, (JObject)item);
                    StashItems.Add(itm);
                }
            }
            catch (Exception e)
            {
                Log.Error("Could not deserialize stash", e);
            }
        }
    }
}
