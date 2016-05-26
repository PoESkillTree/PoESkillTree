using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using log4net;

namespace UpdateDB.DataLoading
{
    public abstract class MultiDataLoader<T> : DataLoader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MultiDataLoader<T>));

        private readonly List<Task> _saveTasks = new List<Task>();

        public override bool SavePathIsFolder
        {
            get { return true; }
        }

        protected void Save(string fileName, T data)
        {
            _saveTasks.Add(SaveAsync(fileName, data));
        }

        private async Task SaveAsync(string fileName, T data)
        {
            try
            {
                using (var f = File.Create(Path.Combine(SavePath, fileName)))
                {
                    await SaveDataToStreamAsync(data, f);
                }
            }
            catch (IOException e)
            {
                if (!fileName.Contains("Two-Stone Ring"))
                    Log.Error("Could not save to file " + fileName + ": " + e.Message);
            }
        }

        protected override async Task CompleteSavingAsync()
        {
            await Task.WhenAll(_saveTasks);
        }

        protected abstract Task SaveDataToStreamAsync(T data, Stream stream);
    }
}