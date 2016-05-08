using System.Globalization;
using System.IO;
using System.Xml.Serialization;

namespace UpdateEquipment
{
    /// <summary>
    /// Base class for classes that extract data from the web as the
    /// XML serializable data type <typeparamref name="T"/>.
    /// </summary>
    public abstract class DataLoader<T>
    {
        /// <summary>
        /// Gets or sets the data that was extracted. Must be set in <see cref="Load"/>.
        /// </summary>
        protected T Data { private get; set; }

        /// <summary>
        /// Extracts data from the web.
        /// </summary>
        public abstract void Load();

        /// <summary>
        /// Saves the extracted data via XML serialization to the file <paramref name="to"/>.
        /// </summary>
        /// <param name="to"></param>
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