using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Xml.Serialization;
using POESKillTree.ViewModels;
using System.ComponentModel;
using POESKillTree.ViewModels.ItemAttribute;
using System.Collections.ObjectModel;
using Raven.Json.Linq;

namespace POESKillTree.Model
{
    public class PersistentData : INotifyPropertyChanged
    {
        public Options Options { get; set; }
        public PoEBuild CurrentBuild { get; set; }
        public List<PoEBuild> Builds { get; set; }


        [XmlIgnore]
        private ObservableCollection<Item> _stash;
        [XmlIgnore]
        public ObservableCollection<Item> Stash
        {
            get { return _stash; }
            set
            { 
                _stash = value;
                OnPropertyChanged("Stash");
            }
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

        public void SavePersistentDataToFile()
        {
            var writer = new XmlSerializer(typeof(PersistentData));
            using (var file = new StreamWriter(@"PersistentData.xml"))
            {
                writer.Serialize(file, this);
            }
            SerializeStash();
        }

        public void LoadPersistentDataFromFile()
        {
            if (File.Exists("PersistentData.xml"))
            {
                using (var reader = new StreamReader(@"PersistentData.xml"))
                {
                    var ser = new XmlSerializer(typeof(PersistentData));
                    var obj = (PersistentData)ser.Deserialize(reader);
                    Options = obj.Options;
                    Builds = obj.Builds;
                    CurrentBuild = obj.CurrentBuild;
                }
                OnPropertyChanged(null);
            }
            DeserializeStash();
        }

        private void SerializeStash()
        {
            try
            {

                RavenJArray arr = new RavenJArray();
                foreach (var item in Stash)
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
                Stash = new ObservableCollection<Item>();
                if (!File.Exists("stash.json"))
                    return;
                var arr = RavenJArray.Parse(File.ReadAllText("stash.json"));
                foreach (var item in arr)
                {
                    var itm = new Item((RavenJObject)item);
                    Stash.Add(itm);
                }
            }
            catch
            { }
        }

        public void SaveBuilds(ItemCollection items)
        {
            Builds = (from PoEBuild item in items select item).ToList();
            SavePersistentDataToFile();
        }

        private void OnPropertyChanged(string caller)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
