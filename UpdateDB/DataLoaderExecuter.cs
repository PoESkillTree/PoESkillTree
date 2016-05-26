using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using log4net;
using POESKillTree.Utils;
using UpdateDB.DataLoading;
using UpdateDB.DataLoading.Gems;

namespace UpdateDB
{
    public class DataLoaderExecutor : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        private static readonly LoaderCollection LoaderDefinitions = new LoaderCollection
        {
            {"affixes", "Equipment/AffixList.xml", new AffixDataLoader(), LoaderCategories.VersionControlled},
            {"base items", "Equipment/ItemList.xml", new ItemDataLoader(), LoaderCategories.VersionControlled},
            {"base item images", "Equipment/Assets", new ItemImageLoader(false), LoaderCategories.NotVersionControlled},
            {"skill tree assets", "", new SkillTreeLoader(), LoaderCategories.NotVersionControlled},
            {"gems", "Equipment/GemList.xml", new GemLoader(new GamepediaReader()), LoaderCategories.VersionControlled}
        };

        private readonly IArguments _arguments;

        private readonly string _savePath;

        private readonly HttpClient _httpClient = new HttpClient();

        public DataLoaderExecutor(IArguments arguments)
        {
            _arguments = arguments;
            switch (arguments.OutputDirectory)
            {
                case OutputDirectory.AppData:
                    _savePath = AppData.GetFolder();
                    break;
                case OutputDirectory.SourceCode:
                    _savePath = "../../../WPFSKillTree";
                    break;
                case OutputDirectory.Current:
                    _savePath = Directory.GetCurrentDirectory();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            AppData.SetApplicationData(_savePath); // necessary for SkillTreeLoader
            _savePath = Path.Combine(_savePath, "Data");

            // The Affix file is big enough to be starved by other requests sometimes.
            _httpClient.Timeout = TimeSpan.FromSeconds(120);
        }

        public async Task LoadAllAsync()
        {
            Log.Debug("Test");
            Log.Info("Starting loading ...");
            Directory.CreateDirectory(_savePath);
            var tasks = from loader in LoaderDefinitions
                        where loader.Categories.HasFlag(_arguments.ActivatedLoader)
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
                var task = dataLoader.LoadAndSaveAsync(_httpClient, tmpPath);

                if (_arguments.CreateBackup)
                    Backup(fullPath, isFolder);

                await task;
                MoveTmpToTarget(tmpPath, fullPath, isFolder);
            }
            else
            {
                // This is for SkillTreeLoader which has no dedicated file/folder and can't really be configured
                await dataLoader.LoadAndSaveAsync(_httpClient, fullPath);
            }
            Log.InfoFormat("Loaded {0}!", name);
        }

        private static void Backup(string path, bool isFolder)
        {
            if (isFolder && Directory.Exists(path))
            {
                var backupPath = path + "Backup";
                if (Directory.Exists(backupPath))
                    Directory.Delete(backupPath, true);

                Directory.CreateDirectory(backupPath);
                foreach (var filePath in Directory.GetFiles(path))
                {
                    File.Copy(filePath, filePath.Replace(path, backupPath), true);
                }
            }
            else if (!isFolder && File.Exists(path))
            {
                File.Copy(path, path + ".bak", true);
            }
        }

        private static void MoveTmpToTarget(string tmpPath, string targetPath, bool isFolder)
        {
            if (isFolder)
            {
                if (Directory.Exists(targetPath))
                    Directory.Delete(targetPath, true);
                Directory.Move(tmpPath, targetPath);
            }
            else
            {
                if (File.Exists(targetPath))
                    File.Delete(targetPath);
                File.Move(tmpPath, targetPath);
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }


        private class LoaderCollection : IEnumerable<LoaderDefinition>
        {
            private readonly List<LoaderDefinition> _loaderDefinitions = new List<LoaderDefinition>();

            public void Add(string name, string file, IDataLoader dataLoader, LoaderCategories categories)
            {
                _loaderDefinitions.Add(new LoaderDefinition
                {
                    Name = name,
                    File = file,
                    DataLoader = dataLoader,
                    Categories = categories
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


        private class LoaderDefinition
        {
            public string Name { get; set; }
            public string File { get; set; }
            public IDataLoader DataLoader { get; set; }
            public LoaderCategories Categories { get; set; }
        }
    }
}