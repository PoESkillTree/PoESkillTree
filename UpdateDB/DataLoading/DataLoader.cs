using System.Net.Http;
using System.Threading.Tasks;
using PoESkillTree.Engine.Utils.WikiApi;

namespace UpdateDB.DataLoading
{
    /// <summary>
    /// Interface for classes that download and save data from a web source.
    /// </summary>
    public interface IDataLoader
    {
        /// <summary>
        /// Returns true iff this class saves its data into a folder or as a file.
        /// </summary>
        bool SavePathIsFolder { get; }

        /// <summary>
        /// Sets the <see cref="HttpClient"/> used for downloading data.
        /// Settings this after <see cref="LoadAndSaveAsync"/> is called may have no effect.
        /// </summary>
        HttpClient HttpClient { set; }

        /// <summary>
        /// Downloads and saves data asynchronously.
        /// </summary>
        /// <param name="savePath">The path to which the data is saved. This is interpreted as a file if
        /// <see cref="SavePathIsFolder"/> is false and as a folder if it is true</param>
        /// <returns>A task that completes once the data is downloaded and saved.</returns>
        Task LoadAndSaveAsync(string savePath);
    }

    /// <summary>
    /// Abstract implementation of <see cref="IDataLoader"/> that splits <see cref="LoadAndSaveAsync"/> into two
    /// methods and provides utility methods.
    /// </summary>
    public abstract class DataLoader : IDataLoader
    {
        /// <summary>
        /// Returns the path (file or folder depending on <see cref="SavePathIsFolder"/> to which data should be saved.
        /// </summary>
        protected string SavePath { get; private set; }

        public HttpClient HttpClient { protected get; set; }

        private ApiAccessor _wikiApiAccessor;

        /// <summary>
        /// Gets a lazily created <see cref="ApiAccessor"/> instance.
        /// </summary>
        protected ApiAccessor WikiApiAccessor
        {
            get { return _wikiApiAccessor ?? (_wikiApiAccessor = new ApiAccessor(HttpClient)); }
        }

        public abstract bool SavePathIsFolder { get; }

        public async Task LoadAndSaveAsync(string savePath)
        {
            SavePath = savePath;
            await LoadAsync();
            await CompleteSavingAsync();
        }

        /// <summary>
        /// Loads data from the web asynchronously.
        /// The data may already be saved once the returned task completes.
        /// </summary>
        protected abstract Task LoadAsync();

        /// <summary>
        /// Returns a task that completes once all data has been saved.
        /// </summary>
        protected virtual Task CompleteSavingAsync()
        {
            return Task.WhenAll();
        }
    }
}