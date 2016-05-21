using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using UpdateEquipment.Utils;

namespace UpdateEquipment.DataLoading
{
    public interface IDataLoader
    {
        Task LoadAndSaveAsync(CachedHttpClient httpClient, string savePath);
    }

    public abstract class DataLoader : IDataLoader
    {
        protected string SavePath { get; private set; }

        protected abstract bool SavePathIsFolder { get; }

        public async Task LoadAndSaveAsync(CachedHttpClient httpClient, string savePath)
        {
            SavePath = savePath;
            if (SavePathIsFolder)
            {
                Directory.CreateDirectory(savePath);
                var backupPath = savePath + "Backup";
                if (Directory.Exists(backupPath))
                    Directory.Delete(backupPath, true);
                Directory.CreateDirectory(backupPath);
                foreach (var filePath in Directory.GetFiles(savePath))
                {
                    File.Copy(filePath, filePath.Replace(savePath, backupPath), true);
                }
            }
            else
            {
                File.Copy(savePath, savePath + ".bak", true);
            }
            await LoadAsync(httpClient);
            await CompleteSavingAsync();
        }

        /// <summary>
        /// Extracts data from the web.
        /// </summary>
        protected abstract Task LoadAsync(CachedHttpClient httpClient);

        protected abstract Task CompleteSavingAsync();

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