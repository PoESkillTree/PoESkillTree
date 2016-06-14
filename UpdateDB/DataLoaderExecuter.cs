using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;
using POESKillTree.Utils;
using UpdateDB.DataLoading;
using UpdateDB.DataLoading.Gems;

namespace UpdateDB
{
    /// <summary>
    /// Runs <see cref="DataLoader"/> instances as specified via <see cref="IArguments"/>.
    /// </summary>
    public class DataLoaderExecutor : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DataLoaderExecutor));

        private readonly LoaderCollection _loaderDefinitions = new LoaderCollection
        {
            {"affixes", "Equipment/AffixList.xml", new AffixDataLoader(), LoaderCategories.VersionControlled, "Affixes"},
            {"base items", "Equipment/ItemList.xml", new ItemDataLoader(), LoaderCategories.VersionControlled, "Items"},
            {"base item images", "Equipment/Assets", new ItemImageLoader(false), LoaderCategories.NotVersionControlled, "ItemImages"},
            {"skill tree assets", "", new SkillTreeLoader(), LoaderCategories.NotVersionControlled, "TreeAssets"},
            {"gems", "ItemDB/GemList.xml", new GemLoader(new GamepediaReader()), LoaderCategories.VersionControlled, "Gems"}
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
            switch (arguments.OutputDirectory)
            {
                case OutputDirectory.AppData:
                    _savePath = AppData.GetFolder();
                    break;
                case OutputDirectory.SourceCode:
                    _savePath = Regex.Replace(Directory.GetCurrentDirectory(),
                        @"PoESkillTree((/|\\).*?)?$", "PoESkillTree/WPFSKillTree");
                    break;
                case OutputDirectory.Current:
                    _savePath = Directory.GetCurrentDirectory();
                    break;
                case OutputDirectory.Specified:
                    _savePath = arguments.SpecifiedOutputDirectory;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Log.InfoFormat("Using output directory {0}.", _savePath);
            _savePath = Path.Combine(_savePath, "Data");

            // The Affix file is big enough to be starved by other requests sometimes.
            _httpClient.Timeout = TimeSpan.FromSeconds(120);
        }

        /// <summary>
        /// Returns true iff the given flag identifies a DataLoader (case-insensitive).
        /// </summary>
        public bool IsLoaderFlagRecognized(string flag)
        {
            return _loaderDefinitions.Any(l => EqualsInvariantIgnoreCase(l.Flag, flag));
        }

        /// <summary>
        /// Returns true iff the given argument is supported by the data loader identifed by
        /// <paramref name="loaderFlag"/> (both case-insensitive).
        /// </summary>
        public bool IsArgumentSupported(string loaderFlag, string argumentKey)
        {
            var forFlag = _loaderDefinitions.FirstOrDefault(l => EqualsInvariantIgnoreCase(l.Flag, loaderFlag));
            return forFlag != null &&
                   forFlag.DataLoader.SupportedArguments.Any(a => EqualsInvariantIgnoreCase(a, argumentKey));
        }

        private static bool EqualsInvariantIgnoreCase(string s1, string s2)
        {
            return s1.Equals(s2, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Adds the given argument (consisting of a key and an optional value) to the loader
        /// identified by <paramref name="loaderFlag"/>.
        /// </summary>
        /// <exception cref="ArgumentException">If either the loader is unrecognized or the argument is
        /// unsupported.</exception>
        public void AddArgument(string loaderFlag, string key, string value = null)
        {
            if (!IsLoaderFlagRecognized(loaderFlag))
                throw new ArgumentException("Unrecognized loader flag", "loaderFlag");
            if (!IsArgumentSupported(loaderFlag, key))
                throw new ArgumentException("Unsupported argument for loader", key);
            _loaderDefinitions.First(l => EqualsInvariantIgnoreCase(l.Flag, loaderFlag))
                .DataLoader.AddArgument(key, value);
        }

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
                        where loader.Category.HasFlag(_arguments.ActivatedLoaders)
                            || explicitlyActivated.Contains(loader.Flag)
                        select LoadAsync(loader.Name, loader.File, loader.DataLoader);
            await Task.WhenAll(tasks);
            Log.Info("Completed loading!");
        }

        private async Task LoadAsync(string name, string path, IDataLoader dataLoader)
        {
            Log.InfoFormat("Loading {0} ...", name);
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
                    FileEx.MoveOverwriting(tmpPath, fullPath);
            }
            else
            {
                // This is for SkillTreeLoader which writes to multiple files/folders and does the tmp stuff itself
                await dataLoader.LoadAndSaveAsync(_httpClient, fullPath);
            }
            Log.InfoFormat("Loaded {0}!", name);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }


        /// <summary>
        /// Collection of <see cref="LoaderDefinition"/>s that supports easy initialization.
        /// </summary>
        private class LoaderCollection : IEnumerable<LoaderDefinition>
        {
            private readonly List<LoaderDefinition> _loaderDefinitions = new List<LoaderDefinition>();

            public void Add(string name, string file, IDataLoader dataLoader, LoaderCategories category, string flag)
            {
                _loaderDefinitions.Add(new LoaderDefinition
                {
                    Name = name,
                    File = file,
                    DataLoader = dataLoader,
                    Category = category,
                    Flag = flag
                });
            }

            public IEnumerator<LoaderDefinition> GetEnumerator()
            {
                return _loaderDefinitions.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }


        /// <summary>
        /// Defines a DataLoader.
        /// </summary>
        private class LoaderDefinition
        {
            /// <summary>
            /// The name that is used for console output.
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// The file/folder to which the loader saves its output.
            /// </summary>
            public string File { get; set; }
            /// <summary>
            /// The actual DataLoader instance.
            /// </summary>
            public IDataLoader DataLoader { get; set; }
            /// <summary>
            /// The category to which this loader belongs.
            /// </summary>
            public LoaderCategories Category { get; set; }
            /// <summary>
            /// A flag that identifies this loader.
            /// </summary>
            public string Flag { get; set; }
        }
    }
}