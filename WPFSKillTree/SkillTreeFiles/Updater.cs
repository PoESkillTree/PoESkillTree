using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Raven.Json.Linq;
using Ionic.Zip;

namespace POESKillTree.SkillTreeFiles
{
    public class Updater
    {
        // Work directory of update process (relative to installation root).
        private static readonly string WorkDir = @".update";

        // Release informations.
        public class Release
        {
            public string Name;
            public string Description;
            public string Version;
            public string URL;
            public string TemporaryFile;

            ~Release()
            {
                // Delete temporary file.
                if (TemporaryFile != null)
                {
                    try
                    {
                        File.Delete(TemporaryFile);
                    }
                    catch (Exception e) {}
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
                }

                return request;
            }
        }

        // Git API URL to fetch releases (the first one is latest one).
        private static readonly string GitApiLatestReleaseURL = "https://api.github.com/repos/l0g0sys/PoESkillTree/releases";

        /**
         * Checks for updates and returns release informations when there is newer one.
         * Returns null if there is no newer release.
         * Throws UpdaterException if error occurs.
         */
        public static Release CheckForUpdates()
        {
            Release release = null;

            var webClient = new UpdaterWebClient();
            webClient.Encoding = Encoding.UTF8;

            try
            {
                string json = webClient.DownloadString(GitApiLatestReleaseURL);

                RavenJArray releases = RavenJArray.Parse(json);
                if (releases.Length < 1) // No releases.
                    throw new UpdaterException("No releases found");

                RavenJObject latest = (RavenJObject)releases[0];
                RavenJArray assets = (RavenJArray)latest["assets"];
                if (assets.Length < 1)
                    throw new UpdaterException("Archive for release is missing");

                string current = GetCurrentVersion();
                string tag = latest["tag_name"].Value<string>();
                if (tag == current)
                    return null;

                string url = ((RavenJObject)assets[0])["browser_download_url"].Value<string>();

                release = new Release
                {
                    Name = latest["name"].Value<string>(),
                    Description = latest["body"].Value<string>(),
                    Version = tag,
                    URL = url
                };
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

            return release;
        }

        // Copies files or directories to target directory recursively.
        public static void CopyTo(string sourcePath, string targetPath)
        {
            if (!Directory.Exists(targetPath))
                throw new DirectoryNotFoundException("No such directory: " + targetPath);

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
                throw new FileNotFoundException("No such file or directory: " + sourcePath);
        }

        /**
         * Downloads release.
         * Throws UpdaterException if error occurs.
         */
        public static void Download(Release release)
        {
            try
            {
                // Create temporary file name.
                release.TemporaryFile = Path.GetTempFileName();

                // Download release.
                var webClient = new UpdaterWebClient();
                webClient.DownloadFile(release.URL, release.TemporaryFile);
            }
            catch (Exception e)
            {
                throw new UpdaterException(e.Message, e);
            }
        }

        // Returns current version.
        public static string GetCurrentVersion ()
        {
            return Properties.Version.AppVersionString;
        }

        /**
         * Updates application to specified release.
         * Throws UpdaterException if error occurs.
         */
        public static void UpdateTo(Release release)
        {
            if (release.TemporaryFile == null)
                Download(release);

            try
            {
                // Create empty work directory.
                if (Directory.Exists(WorkDir))
                    Directory.Delete(WorkDir, true);
                Directory.CreateDirectory(WorkDir);

                // Extract archive to work directory.
                ZipFile zip = ZipFile.Read(release.TemporaryFile);
                zip.ExtractAll(WorkDir);

                // Copy content of first directory found in work directory to installation root.
                DirectoryInfo work = new DirectoryInfo(WorkDir);
                DirectoryInfo[] dirs = work.GetDirectories();
                if (dirs.Length == 0)
                    throw new UpdaterException("Incorrect archive content");
                string sourceDir = dirs[0].FullName;

                CopyTo(sourceDir, ".");

                Directory.Delete(WorkDir, true);
            }
            catch (Exception e)
            {
                throw new UpdaterException(e.Message, e);
            }
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
