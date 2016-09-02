using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using log4net;

namespace UpdateDB.DataLoading
{
    /// <summary>
    /// Base class for classes that loads multiple data instances of type
    /// <typeparamref name="T"/>.
    /// </summary>
    public abstract class MultiDataLoader<T> : DataLoader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MultiDataLoader<T>));

        private readonly List<Task> _saveTasks = new List<Task>();

        public override bool SavePathIsFolder
        {
            get { return true; }
        }

        /// <summary>
        /// Start a task that saves the data instance <paramref name="data"/> to <see cref="fileName"/>.
        /// This must be called in <see cref="DataLoader.LoadAsync"/> to save the loaded data.
        /// <para>
        /// The task calls <see cref="SaveDataToStreamAsync"/>.
        /// </para>
        /// </summary>
        protected void AddSaveTask(string fileName, T data)
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

        /// <summary>
        /// Saves the data instance <paramref name="data"/> to the given stream asynchronously.
        /// </summary>
        /// <returns>A task that completes once <paramref name="data"/> is fully written onto
        /// <paramref name="stream"/></returns>
        protected abstract Task SaveDataToStreamAsync(T data, Stream stream);
    }
}