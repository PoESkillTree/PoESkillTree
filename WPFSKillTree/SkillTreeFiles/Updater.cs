using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Raven.Json.Linq;
using Ionic.Zip;
using POESKillTree.Localization;

namespace POESKillTree.SkillTreeFiles
{
    /* Update manager.
     * Provides API for semi-synchronous update process (downloading is asynchronous, while checking for updates and installation are synchronous tasks).
     */ 
    public class Updater
    {
        // Git API URL to fetch releases (the first one is latest one).
        private static readonly string GitAPILatestReleaseURL = "https://api.github.com/repos/EmmittJ/PoESkillTree/releases";
        // The flag whether check for updates was done and was successful.
        public static bool IsChecked = false;
        // The flag whether download is complete.
        public static bool IsDownloaded { get { return Latest != null && Latest.IsDownloaded; } }
        // The flag whether download is in progress.
        public static bool IsDownloading { get { return Latest != null && Latest.IsDownloading; } }
        // The flag whether installation completed.
        public static bool IsInstalled = false;
        // Latest release.
        private static Release Latest;
        // Regular expression for a released ZIP package file name.
        private static readonly Regex ReZipPackage = new Regex(@"PoESkillTree.*\.zip$", RegexOptions.IgnoreCase);
        // HTTP request timeout for release checks and downloads (in seconds).
        private const int REQUEST_TIMEOUT = 15;
        // Work directory of update process (relative to installation root).
        private static readonly string WorkDir = ".update";

        // Release informations.
        public class Release
        {
            // The web client instance of current download process.
            private WebClient Client;
            // The name.
            public string Name;
            // The description.
            public string Description;
            // The flag whether release was downloaded.
            public bool IsDownloaded { get { return Client == null && TemporaryFile != null; } }
            // The flag whether download is still in progress.
            public bool IsDownloading { get { return Client != null; } }
            // The flag whether release is a pre-release.
            public bool Prerelease;
            // The temporary file for package download.
            private string TemporaryFile;
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

                // Delete temporary file.
                if (TemporaryFile != null)
                {
                    try
                    {
                        File.Delete(TemporaryFile);
                        TemporaryFile = null;
                    }
                    catch (Exception e) { }
                }

                // Delete work directory.
                if (Directory.Exists(WorkDir))
                {
                    try
                    {
                        Directory.Delete(WorkDir, true);
                    }
                    catch (Exception e) {}
                }
            }

            /* Downloads release.
             * Throws UpdaterException if error occurs.
             */
            public void Download(AsyncCompletedEventHandler completedHandler, DownloadProgressChangedEventHandler progressHandler)
            {
                if (Client != null)
                    throw new UpdaterException(L10n.Message("Download already in progress"));
                if (TemporaryFile != null)
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

                    // Create temporary file.
                    TemporaryFile = Path.GetTempFileName();

                    // Start download.
                    Client.DownloadFileAsync(URI, TemporaryFile);
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

            // Returns source directory of an update.
            private string GetSourceDir()
            {
                DirectoryInfo work = new DirectoryInfo(WorkDir);
                DirectoryInfo[] dirs = work.GetDirectories();

                return dirs.Length == 0 ? null : dirs[0].FullName;
            }

            /* Installs release.
             * Throws UpdaterException if error occurs.
             */
            public void Install()
            {
                if (Client != null)
                    throw new UpdaterException(L10n.Message("Download still in progress"));

                if (TemporaryFile == null)
                    throw new UpdaterException(L10n.Message("No package downloaded"));

                try
                {
                    // Create empty work directory.
                    Directory.CreateDirectory(WorkDir);

                    // Extract package.
                    ZipFile zip = ZipFile.Read(TemporaryFile);
                    zip.ExtractAll(WorkDir);

                    // Copy content of first directory found in work directory to installation root.
                    string sourceDir = GetSourceDir();
                    if (sourceDir == null)
                        throw new UpdaterException(L10n.Message("Invalid package content"));
                    CopyTo(sourceDir, ".");

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
                Latest.Dispose();
            }
            Latest = null;
            IsChecked = IsInstalled = false;

            var webClient = new UpdaterWebClient();
            webClient.Encoding = Encoding.UTF8;

            try
            {
                string json = webClient.DownloadString(GitAPILatestReleaseURL);

                RavenJArray releases = RavenJArray.Parse(json);
                if (releases.Length < 1)
                    throw new UpdaterException(L10n.Message("No release found"));

                string current = GetCurrentVersion(); // Current version (tag).

                // Iterate thru avialable releases.
                foreach (RavenJObject release in (RavenJArray)releases)
                {
                    // Drafts are not returned by API, but just in case...
                    bool draft = release["draft"].Value<bool>();
                    if (draft) continue; // Ignore drafts.

                    // Check if there are assets attached.
                    RavenJArray assets = (RavenJArray)release["assets"];
                    if (assets.Length < 1) continue; // No assets, ignore it.

                    // Compare release tag with our version (tag).
                    // Assumption is that no one will make realease with older version tag.
                    // So, any different version should be newer.
                    string tag = release["tag_name"].Value<string>();
                    if (tag == current) // If we didn't find different tag till now, then there is no newer version.
                    {
                        IsChecked = true;

                        return null;
                    }

                    // Check if it is pre-release and we want to update to it.
                    bool prerelease = release["prerelease"].Value<bool>();
                    if (prerelease && !Prerelease) continue; // Found unwanted pre-release, ignore it.

                    // Find PoESkillTree ZIP package.
                    RavenJObject zipAsset = null;
                    foreach (RavenJObject asset in assets)
                    {
                        string content_type = asset["content_type"].Value<string>();
                        if (content_type != "application/zip") continue; // Not a ZIP, ignore it.

                        string name = asset["name"].Value<string>();
                        Match m = ReZipPackage.Match(name);
                        if (m.Success)
                        {
                            // Found ZIP package.
                            zipAsset = asset;
                            break;
                        }
                    }
                    if (zipAsset == null) continue; // No ZIP package found.

                    // This is newer release (probably).
                    IsChecked = true;
                    Latest = new Release
                    {
                        Name = release["name"].Value<string>(),
                        Description = release["body"].Value<string>(),
                        Prerelease = prerelease,
                        Version = tag,
                        URI = new Uri(zipAsset["browser_download_url"].Value<string>())
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

        // Copies files or directories to target directory recursively.
        public static void CopyTo(string sourcePath, string targetPath)
        {
            if (!Directory.Exists(targetPath))
                throw new DirectoryNotFoundException(String.Format(L10n.Message("No such directory: {0}"), targetPath));

            if (File.Exists(sourcePath))
            {
                FileInfo src = new FileInfo(sourcePath);
                src.CopyTo(Path.Combine(targetPath, src.Name), true);
            }
            else if (Directory.Exists(sourcePath))
            {
                DirectoryInfo src = new DirectoryInfo(sourcePath);

                foreach (FileInfo file in src.GetFiles())
                    file.CopyTo(Path.Combine(targetPath, file.Name), true);

                foreach (DirectoryInfo dir in src.GetDirectories())
                {
                    string subdir = Path.Combine(targetPath, dir.Name);
                    if (!Directory.Exists(subdir))
                        Directory.CreateDirectory(subdir);

                    CopyTo(dir.FullName, subdir);
                }
            }
            else
                throw new FileNotFoundException(String.Format(L10n.Message("No such file or directory: {0}"), sourcePath));
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

            IsChecked = IsInstalled = false;
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
        public static string GetCurrentVersion()
        {
            return Properties.Version.ProductVersion;
        }

        // Return latest release, or null if there is none or it wasn't checked for yet.
        public static Release GetLatestRelease()
        {
            return Latest;
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
                Latest = null;
                IsInstalled = true;
            }
        }

        // Restarts application.
        public static void RestartApplication()
        {
            Bootstrap.Restart();
        }
    }

    // Updater exception.
    public class UpdaterException : Exception
    {
        public UpdaterException()
            : base()
        {
        }

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
