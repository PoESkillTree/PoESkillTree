using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

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
        /// Returns the argument keys supported by this data loader.
        /// </summary>
        IEnumerable<string> SupportedArguments { get; }

        /// <summary>
        /// Adds an argument which controls this data loader's behaviour.
        /// key and value are case-insensitive.
        /// </summary>
        /// <exception cref="ArgumentException">If <paramref name="key"/> is not supported.</exception>
        void AddArgument(string key, string value = null);

        /// <summary>
        /// Downloads and saves data asynchronously.
        /// </summary>
        /// <param name="httpClient"><see cref="HttpContent"/> used for downloading data.</param>
        /// <param name="savePath">The path to which the data is saved. This is interpreted as a file if
        /// <see cref="SavePathIsFolder"/> is false and as a folder if it is true</param>
        /// <returns>A task that completes once the data is downloaded and saved.</returns>
        Task LoadAndSaveAsync(HttpClient httpClient, string savePath);
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

        public abstract bool SavePathIsFolder { get; }

        public virtual IEnumerable<string> SupportedArguments
        {
            get { return Enumerable.Empty<string>(); }
        }

        private readonly Dictionary<string, string> _suppliedArguments = new Dictionary<string, string>();

        /// <summary>
        /// Gets a dictionary containing all arguments supplied to this data loader. The key is the name
        /// of the argument and the value an optional value that may be null if it was not specified.
        /// </summary>
        protected IReadOnlyDictionary<string, string> SuppliedArguments
        {
            get { return _suppliedArguments; }
        }

        public void AddArgument(string key, string value = null)
        {
            if (!SupportedArguments.Contains(key.ToLowerInvariant()))
                throw new ArgumentException("Key " + key + " is not supported.", "key");
            _suppliedArguments[key.ToLowerInvariant()] = value;
        }

        public async Task LoadAndSaveAsync(HttpClient httpClient, string savePath)
        {
            SavePath = savePath;
            await LoadAsync(httpClient);
            await CompleteSavingAsync();
        }

        /// <summary>
        /// Loads data from the web asynchronously using the given <see cref="HttpContent"/>.
        /// The data may already be saved once the returned task completes.
        /// </summary>
        protected abstract Task LoadAsync(HttpClient httpClient);

        /// <summary>
        /// Returns a task that completes once all data has been saved.
        /// </summary>
        protected abstract Task CompleteSavingAsync();

        /// <summary>
        /// Equal to <code>int.Parse(s, CultureInfo.InvariantCulture)</code>
        /// </summary>
        protected static int ParseInt(string s)
        {
            return int.Parse(s, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Equal to <code>int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out i)</code>
        /// </summary>
        protected static bool TryParseInt(string s, out int i)
        {
            return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out i);
        }

        /// <summary>
        /// Equal to <code>float.Parse(s, CultureInfo.InvariantCulture)</code>
        /// </summary>
        protected static float ParseFloat(string s)
        {
            return float.Parse(s, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Equal to <code>float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out f)</code>
        /// </summary>
        protected static bool TryParseFloat(string s, out float f)
        {
            return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out f);
        }
    }
}