using System.Globalization;
using System.IO;
using System.Xml.Serialization;

namespace UpdateEquipment
{
    public abstract class DataLoader<T>
    {
        protected T Data { private get; set; }

        public abstract void Load();

        public void Save(string to)
        {
            using (TextWriter writer = new StreamWriter(to))
            {
                var ser = new XmlSerializer(typeof(T));
                ser.Serialize(writer, Data);
            }
        }

        protected static int ParseInt(string s)
        {
            return int.Parse(s, CultureInfo.InvariantCulture);
        }
        
        protected static bool TryParseInt(string s, out int i)
        {
            return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out i);
        }

        protected static float ParseFloat(string s)
        {
            return float.Parse(s, CultureInfo.InvariantCulture);
        }

        protected static bool TryParseFloat(string s, out float f)
        {
            return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out f);
        }
    }
}