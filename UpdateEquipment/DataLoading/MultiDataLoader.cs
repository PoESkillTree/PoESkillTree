using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace UpdateEquipment.DataLoading
{
    public abstract class MultiDataLoader<T> : DataLoader
    {
        private readonly List<Task> _saveTasks = new List<Task>();

        protected override bool SavePathIsFolder
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
                    Console.WriteLine("Could not save to file " + fileName + ": " + e.Message);
            }
        }

        protected override async Task CompleteSavingAsync()
        {
            await Task.WhenAll(_saveTasks);
        }

        protected abstract Task SaveDataToStreamAsync(T data, Stream stream);
    }
}