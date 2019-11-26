#nullable disable
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
    public class Updater
    {
        // Git API URL to fetch releases (the first one is latest one).
        private static readonly string GitAPILatestReleaseURL = "https://api.github.com/repos/PoESkillTree/PoESkillTree/releases";
        // The language value name of Uninstall registry key.
        private const string InnoSetupUninstallLanguageValue = "Inno Setup: Language";
        // The suffix added to AppId to form Uninstall registry key for an application.
        private const string InnoSetupUninstallAppIdSuffix = "_is1";
        // Latest release.
        private static Release Latest;
        // Asset content type of package.
        private const string PackageContentType = "application/x-msdownload";
        // Regular expression for a released package file name.
        private static readonly Regex RePackage = new Regex(@".*\.exe$", RegexOptions.IgnoreCase);
        // HTTP request timeout for release checks and downloads (in seconds).
        private const int REQUEST_TIMEOUT = 15;

        // Release informations.
        public class Release
        {
            // The web client instance of current download process.
            private WebClient Client;
            // The destination file for package download.
            private string DownloadFile;
            // The flag whether release was downloaded.
            public bool IsDownloaded { get { return Client == null && DownloadFile != null; } }
            // The flag whether download is still in progress.
            public bool IsDownloading { get { return Client != null; } }
            // The flag whether installation was successfuly started.
            public bool IsInstalled;
            // The flag whether release is a pre-release.
            public bool IsPrerelease;
            // The flag whether release is an update of this product.
            public bool IsUpdate;
            // The file name of package.
            public string PackageFileName;
            // The URI of release package.
            public Uri URI;
            // The version string.
            public string Version;

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
                if (Client.IsBusy)
                    Client.CancelAsync();
            }

            // Dispose of all resources.
            public void Dispose()
            {
                // Dispose web client.
                if (Client != null)
                {
                    Client.Dispose();
                    Client = null;
                }

                // Delete download file.
                if (DownloadFile != null)
                {
                    try
                    {
                        File.Delete(DownloadFile);
                        DownloadFile = null;
                    }
                    catch (Exception) { } // File won't be deleted while setup is running.
                }
            }

            /* Downloads release.
             * Throws UpdaterException if error occurs.
             */
            public void Download(AsyncCompletedEventHandler completedHandler, DownloadProgressChangedEventHandler progressHandler)
            {
                if (Client != null)
                    throw new UpdaterException(L10n.Message("Download already in progress"));
                if (DownloadFile != null)
                    throw new UpdaterException(L10n.Message("Download already completed"));

                try
                {
                    // Initialize web client.
                    Client = new UpdaterWebClient();
                    Client.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadCompleted);
                    if (completedHandler != null)
                        Client.DownloadFileCompleted += new AsyncCompletedEventHandler(completedHandler);
                    if (progressHandler != null)
                        Client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(progressHandler);

                    // Create download file.
                    DownloadFile = Path.Combine(Path.GetTempPath(), PackageFileName);

                    // Start download.
                    Client.DownloadFileAsync(URI, DownloadFile);
                }
                catch (Exception e)
                {
                    Dispose();
                    throw new UpdaterException(e.Message, e);
                }
            }

            // Invoked when download completes, aborts or fails.
            private void DownloadCompleted(Object sender, AsyncCompletedEventArgs e)
            {
                // Dispose web client.
                Client.Dispose();
                Client = null;

                // Dispose of resources so download can be retried.
                if (e.Cancelled || e.Error != null) Dispose();
            }

            /* Installs release.
             * Throws UpdaterException if error occurs.
             */
            public void Install()
            {
                if (Client != null)
                    throw new UpdaterException(L10n.Message("Download still in progress"));

                if (DownloadFile == null)
                    throw new UpdaterException(L10n.Message("No package downloaded"));

                try
                {
                    Process setup = new Process();
                    setup.StartInfo.FileName = DownloadFile;
                    setup.StartInfo.WorkingDirectory = Path.GetDirectoryName(DownloadFile);
                    setup.StartInfo.CreateNoWindow = true;
                    setup.StartInfo.UseShellExecute = false;

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

                if (request is HttpWebRequest)
                {
                    HttpWebRequest httpRequest = (HttpWebRequest)request;
                    httpRequest.UserAgent = "PoESkillTree";
                    httpRequest.KeepAlive = false;
                    httpRequest.Timeout = REQUEST_TIMEOUT * 1000;
                }

                return request;
            }
        }

        // Cancels download.
        public static void Cancel()
        {
            if (Latest != null && Latest.IsDownloading)
                Latest.Cancel();
        }

        /* Checks for updates and returns release informations when there is newer one.
         * Returns null if there is no newer release.
         * If Prerelease argument is true, it will return also Pre-release, otherwise Pre-releases are ignored.
         * An existing last checked release will be discarded.
         * Throws UpdaterException if error occurs.
         */
        public static Release CheckForUpdates(bool Prerelease = true)
        {
            if (Latest != null)
            {
                if (Latest.IsDownloading)
                    throw new UpdaterException(L10n.Message("Download already in progress"));

                // If release was installed, report no update.
                if (Latest.IsInstalled) return null;

                Dispose();
            }

            if (IsNewerProductInstalled())
            {
                // Newer product is installed, there is no update.
                return null;
            }

            var webClient = new UpdaterWebClient();
            webClient.Encoding = Encoding.UTF8;

            try
            {
                string json = webClient.DownloadString(GitAPILatestReleaseURL);
                JArray releases = JArray.Parse(json);
                if (releases.Count < 1)
                    throw new UpdaterException(L10n.Message("No release found"));

                var current = GetCurrentVersion(); // Current version (tag).

                // Iterate thru avialable releases.
                foreach (JObject release in (JArray)releases)
                {
                    // Drafts are not returned by API, but just in case...
                    bool draft = release["draft"].Value<bool>();
                    if (draft) continue; // Ignore drafts.

                    // Check if there are assets attached.
                    JArray assets = (JArray)release["assets"];
                    if (assets.Count < 1) continue; // No assets, ignore it.

                    // Compare release tag with our version (tag).
                    string tag = release["tag_name"].Value<string>();
                    var version = SemanticVersion.Parse(tag);
                    if (version.CompareTo(current) <= 0)
                    {
                        // Same or older version.
                        return null;
                    }

                    // Check if it is pre-release and we want to update to it.
                    bool prerelease = release["prerelease"].Value<bool>();
                    if (prerelease && !Prerelease) continue; // Found unwanted pre-release, ignore it.

                    // Find release package.
                    string fileName = null;
                    JObject pkgAsset = null;
                    foreach (JObject asset in assets)
                    {
                        // Check if asset upload completed.
                        if (asset["state"].Value<string>() != "uploaded")
                            continue;

                        string content_type = asset["content_type"].Value<string>();
                        if (content_type != PackageContentType) continue; // Not a package, ignore it.

                        fileName = asset["name"].Value<string>();
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
                    Latest = new Release
                    {
                        IsPrerelease = prerelease,
                        // A release is an update, if file name starts with our PackageName.
                        IsUpdate = fileName.StartsWith(GetPackageName(AppData.ProductName) + "-"),
                        PackageFileName = fileName,
                        Version = tag,
                        URI = new Uri(pkgAsset["browser_download_url"].Value<string>())
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

            return Latest;
        }

        // Dispose of current update process.
        public static void Dispose()
        {
            if (Latest != null)
            {
                if (Latest.IsDownloading)
                    throw new UpdaterException(L10n.Message("Download still in progress"));
                Latest.Dispose();
                Latest = null;
            }
        }

        /* Downloads latest release.
         * Throws UpdaterException if error occurs.
         */
        public static void Download(AsyncCompletedEventHandler completedHandler = null, DownloadProgressChangedEventHandler progressHandler = null)
        {
            if (Latest != null)
            {
                if (Latest.IsDownloaded || Latest.IsDownloading)
                    throw new UpdaterException(L10n.Message("Download completed or still in progress"));

                Latest.Download(completedHandler, progressHandler);
            }
        }

        // Returns current version.
        private static SemanticVersion GetCurrentVersion()
        {
            return SemanticVersion.Parse(AppData.ProductVersion);
        }

        // Return latest release, or null if there is none or it wasn't checked for yet.
        public static Release GetLatestRelease()
        {
            return Latest;
        }

        // Returns PackageName according to ProductName.
        public static string GetPackageName(string productName)
        {
            foreach (char invalid in Path.GetInvalidFileNameChars())
                if (productName.Contains(invalid))
                    throw new Exception("ProductName contains invalid character(s)");

            // Remove space characters.
            return productName.Replace(" ", string.Empty);
        }

        // Returns language chosen at last setup.
        public static string GetSetupLanguage()
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

                    string language = uninstallKey.GetValue(InnoSetupUninstallLanguageValue) as string;
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
            if (Latest != null)
            {
                if (Latest.IsDownloading)
                    throw new UpdaterException(L10n.Message("Download still in progress"));
                if (!Latest.IsDownloaded)
                    throw new UpdaterException(L10n.Message("No package downloaded"));

                // If installation fails (exception will be thrown), latest release will be in ready to re-download state.
                Latest.Install();
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
                        string productName = key.GetValue("DisplayName") as string;
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
