using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Xml.Serialization;
using System.ComponentModel;
using POESKillTree.ViewModels;
using POESKillTree.Utils;

namespace POESKillTree.Model
{
    public class PersistentData : INotifyPropertyChanged
    {
        public Options Options { get; set; }
        public PoEBuild CurrentBuild { get; set; }
        public List<PoEBuild> Builds { get; set; }

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
        {
            var writer = new XmlSerializer(typeof (PersistentData));
            var file = new StreamWriter(path, false, System.Text.Encoding.UTF8);
            writer.Serialize(file, this);
            file.Close();
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
                reader.Close();
                OnPropertyChanged(null);
            }
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
