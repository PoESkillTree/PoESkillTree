using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace POESKillTree.Model
{
    public class PersistentData
    {
        public Options Options { get; set; }

        public void SavePersistentDataToFile()
        {
            var writer = new XmlSerializer(typeof (PersistentData));
            var file = new StreamWriter(@"PersistentData.xml");
            writer.Serialize(file, this);
            file.Close();
        }

        public void LoadPersistentDataFromFile()
        {
            if (File.Exists("PersistentData.xml"))
            {
                var reader = XmlReader.Create(new StreamReader(@"PersistentData.xml"));
                var ser = new XmlSerializer(typeof (PersistentData));
                var obj = (PersistentData) ser.Deserialize(reader);
                Options = obj.Options;
            }
        }
    }
}
