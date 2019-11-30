using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using PoESkillTree.Localization;
using Newtonsoft.Json.Linq;
using PoESkillTree.Utils;

namespace PoESkillTree.SkillTreeFiles
{
    /* Update manager.
     * Provides API for semi-synchronous update process (downloading is asynchronous, while checking for updates and installation are synchronous tasks).
     */ 
    public static class Updater
    {
        // Git API URL to fetch releases (the first one is latest one).
        private const string GitApiLatestReleaseUrl = "https://api.github.com/repos/PoESkillTree/PoESkillTree/releases";

        // The language value name of Uninstall registry key.
        private const string InnoSetupUninstallLanguageValue = "Inno Setup: Language";
        // The suffix added to AppId to form Uninstall registry key for an application.
        private const string InnoSetupUninstallAppIdSuffix = "_is1";
        // Latest release.
        private static Release? _latest;
        // Asset content type of package.
        private const string PackageContentType = "application/x-msdownload";
        // Regular expression for a released package file name.
        private static readonly Regex RePackage = new Regex(@".*\.exe$", RegexOptions.IgnoreCase);
        // HTTP request timeout for release checks and downloads (in seconds).
        private const int RequestTimeout = 15;

        // Release informations.
        public class Release
        {
            // The web client instance of current download process.
            private WebClient? _client;
            // The destination file for package download.
            private string? _downloadFile;
            // The flag whether release was downloaded.
            public bool IsDownloaded => _client == null && _downloadFile != null;

            // The flag whether download is still in progress.
            public bool IsDownloading => _client != null;

            // The flag whether installation was successfuly started.
            public bool IsInstalled;
            // The flag whether release is a pre-release.
            public bool IsPreRelease;
            // The flag whether release is an update of this product.
            public bool IsUpdate;
            // The file name of package.
            public string PackageFileName = default!;
            // The URI of release package.
            public Uri Uri = default!;
            // The version string.
            public string Version = default!;

            ~Release()
            {
                try
                {
                    Dispose();
                }
                catch {}
            }

            // Cancels download.
            public void Cancel()
            {
                // Cancel download in progress.
                if (_client != null && _client.IsBusy)
                    _client.CancelAsync();
            }

            // Dispose of all resources.
            public void Dispose()
            {
                // Dispose web client.
                if (_client != null)
                {
                    _client.Dispose();
                    _client = null;
                }

                // Delete download file.
                if (_downloadFile != null)
                {
                    try
                    {
                        File.Delete(_downloadFile);
                        _downloadFile = null;
                    }
                    catch (Exception) { } // File won't be deleted while setup is running.
                }
            }

            /* Downloads release.
             * Throws UpdaterException if error occurs.
             */
            public void Download(AsyncCompletedEventHandler? completedHandler, DownloadProgressChangedEventHandler? progressHandler)
            {
                if (_client != null)
                    throw new UpdaterException(L10n.Message("Download already in progress"));
                if (_downloadFile != null)
                    throw new UpdaterException(L10n.Message("Download already completed"));

                try
                {
                    // Initialize web client.
                    _client = new UpdaterWebClient();
                    _client.DownloadFileCompleted += DownloadCompleted;
                    if (completedHandler != null)
                        _client.DownloadFileCompleted += completedHandler;
                    if (progressHandler != null)
                        _client.DownloadProgressChanged += progressHandler;

                    // Create download file.
                    _downloadFile = Path.Combine(Path.GetTempPath(), PackageFileName);

                    // Start download.
                    _client.DownloadFileAsync(Uri, _downloadFile);
                }
                catch (Exception e)
                {
                    Dispose();
                    throw new UpdaterException(e.Message, e);
                }
            }

            // Invoked when download completes, aborts or fails.
            private void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
            {
                // Dispose web client.
                _client?.Dispose();
                _client = null;

                // Dispose of resources so download can be retried.
                if (e.Cancelled || e.Error != null) Dispose();
            }

            /* Installs release.
             * Throws UpdaterException if error occurs.
             */
            public void Install()
            {
                if (_client != null)
                    throw new UpdaterException(L10n.Message("Download still in progress"));

                if (_downloadFile == null)
                    throw new UpdaterException(L10n.Message("No package downloaded"));

                try
                {
                    Process setup = new Process
                    {
                        StartInfo =
                        {
                            FileName = _downloadFile,
                            WorkingDirectory = Path.GetDirectoryName(_downloadFile),
                            CreateNoWindow = true,
                            UseShellExecute = false
                        }
                    };

                    // Perform silent setup if release is an update.
                    if (IsUpdate)
                    {
                        string arguments = "/SP- /SILENT /NOICONS /LANG=" + GetSetupLanguage();

                        // If running in portable mode, use our program directory as destination folder.
                        if (AppData.IsPortable)
                            arguments += " /PORTABLE=1 \"/DIR=" + AppData.ProgramDirectory + "\"";

                        setup.StartInfo.Arguments = arguments;
                    }

                    setup.Start();

                    // Indicate that installation is running.
                    IsInstalled = true;

                    Dispose();
                }
                catch (Exception e)
                {
                    Dispose();
                    throw new UpdaterException(e.Message, e);
                }
            }
        }

        // WebClient HTTP request override.
        class UpdaterWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                WebRequest request = base.GetWebRequest(address);

                if (request is HttpWebRequest httpRequest)
                {
                    httpRequest.UserAgent = "PoESkillTree";
                    httpRequest.KeepAlive = false;
                    httpRequest.Timeout = RequestTimeout * 1000;
                }

                return request;
            }
        }

        // Cancels download.
        public static void Cancel()
        {
            if (_latest != null && _latest.IsDownloading)
                _latest.Cancel();
        }

        /* Checks for updates and returns release informations when there is newer one.
         * Returns null if there is no newer release.
         * If includePreReleases argument is true, it will return also Pre-release, otherwise Pre-releases are ignored.
         * An existing last checked release will be discarded.
         * Throws UpdaterException if error occurs.
         */
        public static Release? CheckForUpdates(bool includePreReleases = true)
        {
            if (_latest != null)
            {
                if (_latest.IsDownloading)
                    throw new UpdaterException(L10n.Message("Download already in progress"));

                // If release was installed, report no update.
                if (_latest.IsInstalled) return null;

                Dispose();
            }

            if (IsNewerProductInstalled())
            {
                // Newer product is installed, there is no update.
                return null;
            }

            var webClient = new UpdaterWebClient {Encoding = Encoding.UTF8};

            try
            {
                string json = webClient.DownloadString(GitApiLatestReleaseUrl);
                JArray releases = JArray.Parse(json);
                if (releases.Count < 1)
                    throw new UpdaterException(L10n.Message("No release found"));

                var current = GetCurrentVersion(); // Current version (tag).

                // Iterate thru avialable releases.
                foreach (JObject release in releases)
                {
                    // Drafts are not returned by API, but just in case...
                    bool draft = release["draft"]!.Value<bool>();
                    if (draft) continue; // Ignore drafts.

                    // Check if there are assets attached.
                    JArray assets = (JArray)release["assets"]!;
                    if (assets.Count < 1) continue; // No assets, ignore it.

                    // Compare release tag with our version (tag).
                    string tag = release["tag_name"]!.Value<string>();
                    var version = SemanticVersion.Parse(tag);
                    if (version.CompareTo(current) <= 0)
                    {
                        // Same or older version.
                        return null;
                    }

                    // Check if it is pre-release and we want to update to it.
                    bool isPreRelease = release["prerelease"]!.Value<bool>();
                    if (isPreRelease && !includePreReleases) continue; // Found unwanted pre-release, ignore it.

                    // Find release package.
                    string? fileName = null;
                    JObject? pkgAsset = null;
                    foreach (JObject asset in assets)
                    {
                        // Check if asset upload completed.
                        if (asset["state"]!.Value<string>() != "uploaded")
                            continue;

                        string content_type = asset["content_type"]!.Value<string>();
                        if (content_type != PackageContentType) continue; // Not a package, ignore it.

                        fileName = asset["name"]!.Value<string>();
                        Match m = RePackage.Match(fileName);
                        if (m.Success)
                        {
                            // Found release package.
                            pkgAsset = asset;
                            break;
                        }
                    }
                    if (pkgAsset == null) continue; // No package found.

                    // This is newer release.
                    _latest = new Release
                    {
                        IsPreRelease = isPreRelease,
                        // A release is an update, if file name starts with our PackageName.
                        IsUpdate = fileName!.StartsWith(GetPackageName(AppData.ProductName) + "-"),
                        PackageFileName = fileName,
                        Version = tag,
                        Uri = new Uri(pkgAsset["browser_download_url"]!.Value<string>())
                    };

                    // We are done, exit loop.
                    break;
                }
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                    throw new UpdaterException("HTTP " + ((int)((HttpWebResponse)e.Response).StatusCode) + " " + ((HttpWebResponse)e.Response).StatusDescription);
                else
                    throw new UpdaterException(e.Message, e);
            }
            catch (Exception e)
            {
                throw new UpdaterException(e.Message, e);
            }

            return _latest;
        }

        // Dispose of current update process.
        public static void Dispose()
        {
            if (_latest != null)
            {
                if (_latest.IsDownloading)
                    throw new UpdaterException(L10n.Message("Download still in progress"));
                _latest.Dispose();
                _latest = null;
            }
        }

        /* Downloads latest release.
         * Throws UpdaterException if error occurs.
         */
        public static void Download(AsyncCompletedEventHandler? completedHandler = null, DownloadProgressChangedEventHandler? progressHandler = null)
        {
            if (_latest != null)
            {
                if (_latest.IsDownloaded || _latest.IsDownloading)
                    throw new UpdaterException(L10n.Message("Download completed or still in progress"));

                _latest.Download(completedHandler, progressHandler);
            }
        }

        // Returns current version.
        private static SemanticVersion GetCurrentVersion()
        {
            return SemanticVersion.Parse(AppData.ProductVersion);
        }

        // Return latest release, or null if there is none or it wasn't checked for yet.
        public static Release? GetLatestRelease()
        {
            return _latest;
        }

        // Returns PackageName according to ProductName.
        private static string GetPackageName(string productName)
        {
            foreach (char invalid in Path.GetInvalidFileNameChars())
                if (productName.Contains(invalid))
                    throw new Exception("ProductName contains invalid character(s)");

            // Remove space characters.
            return productName.Replace(" ", string.Empty);
        }

        // Returns language chosen at last setup.
        private static string? GetSetupLanguage()
        {
            if (AppData.IsPortable)
            {
                return AppData.GetIniValue("Setup", "Language");
            }
            else
                using (RegistryKey uninstallKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + AppData.ProductName + InnoSetupUninstallAppIdSuffix))
                {
                    if (uninstallKey == null)
                        throw new Exception(L10n.Message("The application is not correctly installed"));

                    var language = uninstallKey.GetValue(InnoSetupUninstallLanguageValue) as string;
                    if (!string.IsNullOrEmpty(language))
                        return language;
                }

            return null;
        }

        /* Installs downloaded release.
         * Throws UpdaterException if error occurs.
         */
        public static void Install()
        {
            if (_latest != null)
            {
                if (_latest.IsDownloading)
                    throw new UpdaterException(L10n.Message("Download still in progress"));
                if (!_latest.IsDownloaded)
                    throw new UpdaterException(L10n.Message("No package downloaded"));

                // If installation fails (exception will be thrown), latest release will be in ready to re-download state.
                _latest.Install();
            }
        }

        // Returns true if newer product is installed.
        private static bool IsNewerProductInstalled()
        {
            var current = GetCurrentVersion();

            using (RegistryKey uninstallKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall"))
            {
                foreach (string name in uninstallKey.GetSubKeyNames())
                {
                    using (RegistryKey key = uninstallKey.OpenSubKey(name))
                    {
                        var productName = key.GetValue("DisplayName") as string;
                        if (string.IsNullOrEmpty(productName)) continue; // Missing DisplayName value.

                        if (!productName.ToLowerInvariant().Contains("PoESkillTree")) continue; // Not our application.

                        if (productName != AppData.ProductName)
                        {
                            var version = SemanticVersion.Parse((string) key.GetValue("DisplayVersion"));
                            if (version.CompareTo(current) > 0)
                                return true;
                        }
                    }
                }
            }

            return false;
        }
    }

    public class UpdaterException : Exception
    {
        public UpdaterException(string message)
            : base(message)
        {
        }

        public UpdaterException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
