using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace UpdateDB.DataLoading
{
    /// <summary>
    /// Base class for classes that extract data from the web as the
    /// XML serializable data type <typeparamref name="T"/>.
    /// </summary>
    public abstract class XmlDataLoader<T> : DataLoader
    {
        /// <summary>
        /// Gets or sets the data that was extracted. Must be set in <see cref="DataLoader.LoadAsync"/>.
        /// </summary>
        protected T Data { private get; set; }

        public override bool SavePathIsFolder
        {
            get { return false; }
        }

        protected override Task CompleteSavingAsync()
        {
            using (var writer = new StreamWriter(SavePath))
            {
                var ser = new XmlSerializer(typeof(T));
                ser.Serialize(writer, Data);
            }
            return Task.WhenAll();
        }
    }
}