using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using NLog;
using PoESkillTree.Engine.Utils;
using PoESkillTree.Engine.Utils.Extensions;
using PoESkillTree.Utils;
using UpdateDB.DataLoading;

namespace UpdateDB
{
    /// <summary>
    /// Runs <see cref="IDataLoader"/> instances as specified via <see cref="IArguments"/>.
    /// </summary>
    public sealed class DataLoaderExecutor : IDisposable
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly IReadOnlyList<LoaderDefinition> _loaderDefinitions = new[]
        {
            new LoaderDefinition("base item images", "Equipment/Assets", new ItemImageLoader(), "ItemImages"),
            new LoaderDefinition("skill tree assets", "", new SkillTreeLoader(), "TreeAssets"),
        };

        private readonly IArguments _arguments;

        private readonly string _savePath;

        private readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// Creates an instance and sets it up using <paramref name="arguments"/>.
        /// </summary>
        /// <param name="arguments">The arguments that define how this instance behaves. Only
        /// <see cref="IArguments.OutputDirectory"/> and <see cref="IArguments.SpecifiedOutputDirectory"/> are
        /// consumed in the constructor.</param>
        public DataLoaderExecutor(IArguments arguments)
        {
            _arguments = arguments;
            _savePath = arguments.OutputDirectory switch
            {
                OutputDirectory.AppData => AppData.GetFolder(),
                OutputDirectory.Current => Directory.GetCurrentDirectory(),
                OutputDirectory.Specified => arguments.SpecifiedOutputDirectory,
                _ => throw new ArgumentOutOfRangeException()
            };
            Log.Info("Using output directory {0}.", _savePath);
            _savePath = Path.Combine(_savePath, "Data");

            _httpClient.DefaultRequestHeaders.Add("User-Agent",
                "PoESkillTree UpdateDB (https://github.com/PoESkillTree/PoESkillTree/tree/master/UpdateDB)");
        }

        /// <summary>
        /// Returns true iff the given flag identifies a DataLoader (case-insensitive).
        /// </summary>
        public bool IsLoaderFlagRecognized(string flag)
            => _loaderDefinitions.Any(l => EqualsInvariantIgnoreCase(l.Flag, flag));

        private static bool EqualsInvariantIgnoreCase(string s1, string s2)
            => s1.Equals(s2, StringComparison.InvariantCultureIgnoreCase);

        /// <summary>
        /// Runs all DataLoader instances asynchronously.
        /// </summary>
        /// <returns>A task that completes once all DataLoaders completed.</returns>
        public async Task LoadAllAsync()
        {
            Log.Info("Starting loading ...");
            Directory.CreateDirectory(_savePath);
            var explicitlyActivated = _arguments.LoaderFlags.ToList();
            var tasks = from loader in _loaderDefinitions
                        where explicitlyActivated.IsEmpty()
                            || explicitlyActivated.Contains(loader.Flag)
                        select LoadAsync(loader.Name, loader.File, loader.DataLoader);
            await Task.WhenAll(tasks);
            Log.Info("Completed loading!");
        }

        private async Task LoadAsync(string name, string path, IDataLoader dataLoader)
        {
            Log.Info("Loading {0} ...", name);
            var fullPath = Path.Combine(_savePath, path);

            if (path.Any())
            {
                var isFolder = dataLoader.SavePathIsFolder;
                var tmpPath = fullPath + (isFolder ? "Tmp" : ".tmp");
                if (isFolder)
                {
                    Directory.CreateDirectory(tmpPath);
                }

                await dataLoader.LoadAndSaveAsync(_httpClient, tmpPath);

                if (isFolder)
                    DirectoryEx.MoveOverwriting(tmpPath, fullPath);
                else
                    FileUtils.MoveOverwriting(tmpPath, fullPath);
            }
            else
            {
                // This is for SkillTreeLoader which writes to multiple files/folders and does the tmp stuff itself
                await dataLoader.LoadAndSaveAsync(_httpClient, fullPath);
            }
            Log.Info("Loaded {0}!", name);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }


        /// <summary>
        /// Defines a DataLoader.
        /// </summary>
        private class LoaderDefinition
        {
            public LoaderDefinition(string name, string file, IDataLoader dataLoader, string flag)
            {
                Name = name;
                File = file;
                DataLoader = dataLoader;
                Flag = flag;
            }


            /// <summary>
            /// The name that is used for console output.
            /// </summary>
            public string Name { get; }
            /// <summary>
            /// The file/folder to which the loader saves its output.
            /// </summary>
            public string File { get; }
            /// <summary>
            /// The actual DataLoader instance.
            /// </summary>
            public IDataLoader DataLoader { get; }
            /// <summary>
            /// A flag that identifies this loader.
            /// </summary>
            public string Flag { get; }
        }
    }
}