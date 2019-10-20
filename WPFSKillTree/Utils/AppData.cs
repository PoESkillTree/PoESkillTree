using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace PoESkillTree.Utils
{
    // Application data folder API.
    public static class AppData
    {
        private static readonly FileVersionInfo VersionInfo =
            FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

        public static string ProductVersion => VersionInfo.ProductVersion;
        public static string ProductName => VersionInfo.ProductName;

        // The absolute path of application data folder.
        private static string _applicationData;

        // The flag indicating whether application data are portable.
        public static bool IsPortable { get; }
        // The name of INI file whose existance indicates that application is running in portable mode (must be same as defined in release.iss script).
        private const string PortableIniFileName = "Portable.ini";

        // Static constructor.
        static AppData()
        {
            IsPortable = File.Exists(Path.Combine(ProgramDirectory, PortableIniFileName));
        }

        // Returns abolute path to appliation data folder (i.e. system folder + AppDataFolderName).
        // If folder doesn't exist, it will be created.
        // If trailingSlash is true, directory separator will be appended to path returned.
        public static string GetFolder(bool trailingSlash = false)
        {
            // Resolve path just once, it doesn't change during process lifetime.
            if (_applicationData == null)
            {
                if (Debugger.IsAttached || IsPortable)
                {
                    // When debugging, use current directory (i.e. application root path).
                    _applicationData = ProgramDirectory;
                }
                else
                {
                    // Use roaming profile to store appliation data.
                    _applicationData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ProductName);
                    if (!Directory.Exists(_applicationData))
                        Directory.CreateDirectory(_applicationData);
                }
            }

            return trailingSlash ? _applicationData + Path.DirectorySeparatorChar : _applicationData;
        }

        // Returns absolute path of subfolder of appliation data folder.
        // If subfolder doesn't exist, it will be created.
        // If trailingSlash is true, directory separator will be appended to path returned.
        public static string GetFolder(string folderName, bool trailingSlash = false)
        {
            string path = Path.Combine(GetFolder(), folderName);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return trailingSlash ? path + Path.DirectorySeparatorChar : path;
        }


        // Returns value of PortableIniFileName INI file.
        // Returns null if not running in portable mode or key was not found.
        public static string GetIniValue(string section, string key)
        {
            if (!IsPortable) return null;

            string[] lines = File.ReadAllLines(Path.Combine(ProgramDirectory, PortableIniFileName));

            bool foundSection = false;
            foreach (string line in lines)
            {
                if (foundSection)
                {
                    if (line.StartsWith("[")) return null; // Next section means key was not found.

                    if (line.StartsWith(key + "="))
                        return line.Substring(key.Length + 1);
                }
                else
                {
                    if (line == "[" + section + "]")
                        foundSection = true;
                }
            }

            return null;
        }

        // Returns directory of running executable.
        public static string ProgramDirectory
        {
            get
            {
                return Path.GetDirectoryName(Uri.UnescapeDataString((new UriBuilder(Assembly.GetExecutingAssembly().CodeBase)).Path));
            }
        }

        /// <summary>
        /// Converts the given path to a path relative to the program executable.
        /// Does nothing if the given path is already relative.
        /// </summary>
        public static string ToRelativePath(string path)
        {
            if (path != Path.GetFullPath(path))
            {
                // already relative
                return path;
            }    
            var referenceUri = new Uri(ProgramDirectory + Path.DirectorySeparatorChar);
            return referenceUri.MakeRelativeUri(new Uri(path)).ToString();
        }

        // Sets ApplicationData path.
        // Use within UnitTest project only!
        public static void SetApplicationData(string path)
        {
            _applicationData = path;
        }
    }
}
