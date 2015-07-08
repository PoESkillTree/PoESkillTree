using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Xml.Serialization;
using POESKillTree.ViewModels;
using System.ComponentModel;
using System.Collections.ObjectModel;
using POESKillTree.ViewModels.Items;
using Newtonsoft.Json.Linq;
using MB.Algodat;
using POESKillTree.Controls;

namespace POESKillTree.Model
{
    public class PersistentData : INotifyPropertyChanged
    {

        public List<StashBookmark> StashBookmarks { get; set; }
        public Options Options { get; set; }
        public PoEBuild CurrentBuild { get; set; }
        public List<PoEBuild> Builds { get; set; }


        [XmlIgnore]
        private ObservableCollection<Item> _stash = new ObservableCollection<Item>();

        [XmlIgnore]
        public ObservableCollection<Item> StashItems
        {
            get { return _stash; }
        }

        public PersistentData()
        {

            Options = new Options();
            CurrentBuild = new PoEBuild
            {
                Url = "http://www.pathofexile.com/passive-skill-tree/AAAAAgMA",
                Level = "1"
            };
            Builds = new List<PoEBuild>();
        }

        // Creates empty file with language option set.
        public static void CreateSetupTemplate(string path, string language)
        {
            PersistentData data = new PersistentData();
            data.Options.Language = language;
            data.SavePersistentDataToFileEx(Path.Combine(path, "PersistentData.xml"));
        }

        public void SavePersistentDataToFile()
        {
            SavePersistentDataToFileEx(AppData.GetFolder(true) + "PersistentData.xml");
        }

        public void SavePersistentDataToFileEx(string path)
        {            var writer = new XmlSerializer(typeof (PersistentData));
            var file = new StreamWriter(path, false, System.Text.Encoding.UTF8);
            writer.Serialize(file, this);
            file.Close();
            SerializeStash();
        }

        public void LoadPersistentDataFromFile()
        {
            string filePath = AppData.GetFolder(true) + "PersistentData.xml";

            if (File.Exists(filePath))
            {
                var reader = new StreamReader(filePath);
                var ser = new XmlSerializer(typeof (PersistentData));
                var obj = (PersistentData)ser.Deserialize(reader);
                Options = obj.Options;
                Builds = obj.Builds;
                CurrentBuild = obj.CurrentBuild;
                StashBookmarks = obj.StashBookmarks;
                reader.Close();
                OnPropertyChanged(null);
            }

            DeserializeStash();
        }

        private void SerializeStash()
        {
            try
            {

                JArray arr = new JArray();
                foreach (var item in StashItems)
                {
                    arr.Add(item.JSONBase);
                }

                File.WriteAllText("stash.json", arr.ToString());
            }
            catch
            { }
        }

        private void DeserializeStash()
        {
            try
            {
                StashItems.Clear();
                if (!File.Exists("stash.json"))
                    return;
                var arr = JArray.Parse(File.ReadAllText("stash.json"));
                foreach (var item in arr)
                {
                    var itm = new Item((JObject)item);
                    StashItems.Add(itm);
                }
            }
            catch
            { }
        }

        public void SetBuilds(ItemCollection items)
        {
            Builds = (from PoEBuild item in items select item).ToList();
        }

        private void OnPropertyChanged(string caller)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
