using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using POESKillTree.Utils;
using UpdateEquipment.DataLoading;
using UpdateEquipment.Utils;

namespace UpdateEquipment
{
    public class Program : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        // current directory while debugging is UpdateEquipment/bin/Debug/
        private static readonly string PathForDebug = Path.Combine("..", "..", "..", "WPFSKillTree");

        private static readonly LoaderCollection LoaderDefinitions = new LoaderCollection
        {
            {"affixes", "AffixList.xml", new AffixDataLoader()},
            {"base items", "ItemList.xml", new ItemDataLoader()},
            {"base item images", "Assets", new ItemImageLoader(false)}
        };

        private readonly string _savePath = Path.Combine(Debugger.IsAttached ? PathForDebug : AppData.GetFolder(),
            "Data", "Equipment");

        private readonly CachingHttpClient _httpClient = new CachingHttpClient();

        public static void Main(string[] args)
        {
            using (var p = new Program())
            {
                p.LoadAllAsync().Wait();
            }
        }

        private async Task LoadAllAsync()
        {
            var tasks = LoaderDefinitions.Select(l => LoadAsync(l.Name, l.File, l.DataLoader));
            await Task.WhenAll(tasks);
        }

        private async Task LoadAsync(string name, string path, IDataLoader dataLoader)
        {
            Log.InfoFormat("Loading {0} ...", name);
            var fullPath = Path.Combine(_savePath, path);
            if (dataLoader.SavePathIsFolder)
            {
                if (Directory.Exists(fullPath))
                {
                    var backupPath = fullPath + "Backup";
                    if (Directory.Exists(backupPath))
                        Directory.Delete(backupPath, true);
                    Directory.CreateDirectory(backupPath);
                    foreach (var filePath in Directory.GetFiles(fullPath))
                    {
                        File.Copy(filePath, filePath.Replace(fullPath, backupPath), true);
                    }
                }
                Directory.CreateDirectory(fullPath);
            }
            else
            {
                if (File.Exists(fullPath))
                    File.Copy(fullPath, fullPath + ".bak", true);
            }
            await dataLoader.LoadAndSaveAsync(_httpClient, fullPath);
            Log.InfoFormat("Loaded {0}!", name);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }


        private class LoaderCollection : IEnumerable<LoaderDefinition>
        {
            private readonly List<LoaderDefinition> _loaderDefinitions = new List<LoaderDefinition>();

            public void Add(string name, string file, IDataLoader dataLoader)
            {
                _loaderDefinitions.Add(new LoaderDefinition
                {
                    Name = name,
                    File = file,
                    DataLoader = dataLoader
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
        }
    }
}
