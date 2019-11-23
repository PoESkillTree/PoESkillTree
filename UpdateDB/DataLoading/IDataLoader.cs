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
        /// Downloads and saves data asynchronously.
        /// </summary>
        /// <param name="httpClient">The client used for downloading data</param>
        /// <param name="savePath">The path to which the data is saved. This is interpreted as a file if
        /// <see cref="SavePathIsFolder"/> is false and as a folder if it is true</param>
        /// <returns>A task that completes once the data is downloaded and saved.</returns>
        Task LoadAndSaveAsync(HttpClient httpClient, string savePath);
    }
}