using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using PoESkillTree.Utils.Extensions;
using POESKillTree.Utils;
using POESKillTree.Utils.Extensions;

namespace POESKillTree.SkillTreeFiles
{
    /// <summary>
    /// Contains methods to download all assets required for the skill tree
    /// (skill tree file, opts file, asset images and sprite images) and methods
    /// to manage backups and temp folders for the files.
    /// </summary>
    public class AssetLoader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AssetLoader));

        private const string SpriteUrl = "http://www.pathofexile.com/image/build-gen/passive-skill-sprite/";

        private const string SkillTreeFile = "SkillTree.txt";
        private const string OptsFile = "Opts.txt";
        private const string AssetsFolder = "Assets/";

        private const string TempFolder = "Temp/";
        private const string BackupFolder = "Backup/";

        private readonly bool _useTempDir;
        private readonly string _path;

        private readonly string _skillTreePath;
        private readonly string _optsPath;
        private readonly string _assetsPath;

        private readonly string _tempSkillTreePath;
        private readonly string _tempOptsPath;
        private readonly string _tempAssetsPath;

        private readonly HttpClient _httpClient;

        /// <param name="httpClient">HttpClient instance used for downloading.</param>
        /// <param name="dataDirPath">The path to the "Data" folder were files are stored  (including "Data").</param>
        /// <param name="useTempDir">Whether to download the files to a temp folder instead of the provided one.
        /// </param>
        public AssetLoader(HttpClient httpClient, string dataDirPath, bool useTempDir)
        {
            _httpClient = httpClient;
            _path = dataDirPath.EnsureTrailingDirectorySeparator();
            _useTempDir = useTempDir;
            _skillTreePath = _path + SkillTreeFile;
            _optsPath = _path + OptsFile;
            _assetsPath = _path + AssetsFolder;
            var tempPath = _useTempDir ? _path + TempFolder : _path;
            _tempSkillTreePath = tempPath + SkillTreeFile;
            _tempOptsPath = tempPath + OptsFile;
            _tempAssetsPath = tempPath + AssetsFolder;
            Directory.CreateDirectory(tempPath);
        }

        /// <summary>
        /// Downloads the skill tree Json file asynchronously. Overwrites an existing file.
        /// </summary>
        /// <returns>The contents of the skill tree file.</returns>
        public async Task<string> DownloadSkillTreeToFileAsync()
        {
            var code = await _httpClient.GetStringAsync(Constants.TreeAddress);
            var regex = new Regex("var passiveSkillTreeData.*");
            var skillTreeObj = regex.Match(code).Value.Replace("\\/", "/");
            skillTreeObj = skillTreeObj.Substring(27, skillTreeObj.Length - 27 - 1).Replace("\"nodes\":{", "\"nodesDict\":{") + "";
            await FileEx.WriteAllTextAsync(_tempSkillTreePath, skillTreeObj);
            return skillTreeObj;
        }

        /// <summary>
        /// Downloads the opt Json file asynchronously. Overwrites an existing file.
        /// </summary>
        /// <returns>The contents of the opts file.</returns>
        public async Task<string> DownloadOptsToFileAsync()
        {
            var code = await _httpClient.GetStringAsync(Constants.TreeAddress);
            var regex = new Regex(@"ascClasses:.*");
            var optsObj = regex.Match(code).Value.Replace("ascClasses", "{ \"ascClasses\"");
            optsObj = optsObj.Substring(0, optsObj.Length - 1) + "}";
            await FileEx.WriteAllTextAsync(_tempOptsPath, optsObj);
            return optsObj;
        }

        /// <summary>
        /// Downloads the node sprite images mentioned in the provided tree asynchronously.
        /// Existing files are not overriden.
        /// </summary>
        /// <param name="inTree"></param>
        /// <param name="reportProgress">If specified, it is called to set this method's progress as a value
        /// from 0 to 1.</param>
        /// <returns></returns>
        internal async Task DownloadSkillNodeSpritesAsync(PoESkillTree inTree,
            Action<double> reportProgress = null)
        {
            Directory.CreateDirectory(_tempAssetsPath);
            var perSpriteProgress = 1.0 / inTree.skillSprites.Count;
            var progress = 0.0;
            foreach (var obj in inTree.skillSprites)
            {
                var sprite = obj.Value[Constants.AssetZoomLevel];
                var path = _tempAssetsPath + sprite.filename;
                var url = SpriteUrl + sprite.filename;
                if (path.Contains('?'))
                    path = path.Remove(path.IndexOf('?'));
                await DownloadAsync(url, path);
                progress += perSpriteProgress;
                reportProgress?.Invoke(progress);
            }
        }

        /// <summary>
        /// Downloads the asset images mentioned in the provided tree asynchronously.
        /// Existing files are not overriden.
        /// </summary>
        /// <param name="inTree"></param>
        /// <param name="reportProgress">If specified, it is called to set this method's progress as a value
        /// from 0 to 1.</param>
        /// <returns></returns>
        internal async Task DownloadAssetsAsync(PoESkillTree inTree, Action<double> reportProgress = null)
        {
            Directory.CreateDirectory(_tempAssetsPath);
            var zoomLevel = inTree.imageZoomLevels[Constants.AssetZoomLevel].ToString(CultureInfo.InvariantCulture);
            var perAssetProgress = 1.0 / inTree.assets.Count;
            var progress = 0.0;
            foreach (var asset in inTree.assets)
            {
                var path = _tempAssetsPath + asset.Key + ".png";
                var url = asset.Value.GetValueOrDefault(zoomLevel, () => asset.Value.Values.First());
                await DownloadAsync(url, path);
                progress += perAssetProgress;
                reportProgress?.Invoke(progress);
            }
        }

        private async Task DownloadAsync(string url, string path)
        {
            if (File.Exists(path))
                return;
            using (var writer = File.Create(path))
            using (var response = await _httpClient.GetAsync(url))
            {
                await response.Content.CopyToAsync(writer);
            }
        }

        /// <summary>
        /// Downloads all files asynchronously.
        /// </summary>
        public async Task DownloadAllAsync()
        {
            var skillTreeTask = DownloadSkillTreeToFileAsync();
            var optsTask = DownloadOptsToFileAsync();

            var treeString = await skillTreeTask;
            var inTree = JsonConvert.DeserializeObject<PoESkillTree>(treeString, new JsonSerializerSettings
            {
                Error = (sender, args) =>
                {
                    if (args.ErrorContext.Path != "groups.515.oo")
                        Log.Error("Exception while deserializing Json tree", args.ErrorContext.Error);
                    args.ErrorContext.Handled = true;
                }
            });
            var spritesTask = DownloadSkillNodeSpritesAsync(inTree);
            var assetsTask = DownloadAssetsAsync(inTree);

            await Task.WhenAll(optsTask, spritesTask, assetsTask);
        }

        /// <summary>
        /// Moves the existing files to a backup folder.
        /// </summary>
        public void MoveToBackup()
        {
            var backupPath = _path + BackupFolder;
            DirectoryEx.DeleteIfExists(backupPath, true);
            Directory.CreateDirectory(backupPath);
            DirectoryEx.MoveIfExists(_assetsPath, backupPath + AssetsFolder, true);
            FileEx.MoveIfExists(_skillTreePath, backupPath + SkillTreeFile, true);
            FileEx.MoveIfExists(_optsPath, backupPath + OptsFile, true);
        }

        /// <summary>
        /// Restores the backup folder created by <see cref="MoveToBackup"/>.
        /// Existing files in the data folder are overwritten by backup files.
        /// </summary>
        public void RestoreBackup()
        {
            var backupPath = _path + BackupFolder;
            DirectoryEx.MoveIfExists(backupPath + AssetsFolder, _assetsPath, true);
            FileEx.MoveIfExists(backupPath + SkillTreeFile, _skillTreePath, true);
            FileEx.MoveIfExists(backupPath + OptsFile, _optsPath, true);
            DirectoryEx.DeleteIfExists(backupPath);
        }

        /// <summary>
        /// Deletes the backup folder and its contents.
        /// </summary>
        public void DeleteBackup()
        {
            DirectoryEx.DeleteIfExists(_path + BackupFolder, true);
        }

        /// <summary>
        /// Deletes the temp folder and its contents.
        /// This instance must have been set to use temporary files.
        /// </summary>
        public void DeleteTemp()
        {
            if (!_useTempDir)
                throw new InvalidOperationException("This instance doesn't use temp directories");
            DirectoryEx.DeleteIfExists(_path + TempFolder, true);
        }

        /// <summary>
        /// Moves the files from the temp folder to the data folder.
        /// This instance must have been set to use temporary files.
        /// </summary>
        public void MoveTemp()
        {
            if (!_useTempDir)
                throw new InvalidOperationException("This instance doesn't use temp directories");
            DirectoryEx.MoveIfExists(_tempAssetsPath, _assetsPath, true);
            FileEx.MoveIfExists(_tempSkillTreePath, _skillTreePath, true);
            FileEx.MoveIfExists(_tempOptsPath, _optsPath, true);
            DirectoryEx.DeleteIfExists(_path + TempFolder);
        }
    }
}