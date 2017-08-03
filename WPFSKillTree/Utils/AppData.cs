using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;

namespace POESKillTree.Utils
{
    // Application data folder API.
    public class AppData
    {
        // The folder name in system folders containing application data.
        private static readonly string AppDataFolderName = Properties.Version.ProductName;
        // The absolute path of application data folder.
        private static string ApplicationData;
        // The flag indicating whether application data are stored in folder with application binaries.
        private static bool HasPortableData;
        // The flag indicating whether application data are portable.
        public static bool IsPortable { get { return HasPortableData; } }
        // The name of INI file whose existance indicates that application is running in portable mode (must be same as defined in release.iss script).
        private const string PortableIniFileName = "Portable.ini";

        // Static constructor.
        static AppData()
        {
            HasPortableData = File.Exists(Path.Combine(ProgramDirectory, PortableIniFileName));
        }

        // Returns abolute path to appliation data folder (i.e. system folder + AppDataFolderName).
        // If folder doesn't exist, it will be created.
        // If trailingSlash is true, directory separator will be appended to path returned.
        public static string GetFolder(bool trailingSlash = false)
        {
            // Resolve path just once, it doesn't change during process lifetime.
            if (ApplicationData == null)
            {
                if (Debugger.IsAttached || HasPortableData)
                {
                    // When debugging, use current directory (i.e. application root path).
                    ApplicationData = ProgramDirectory;
                }
                else
                {
                    // Use roaming profile to store appliation data.
                    ApplicationData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppDataFolderName);
                    if (!Directory.Exists(ApplicationData))
                        Directory.CreateDirectory(ApplicationData);
                }
            }

            return trailingSlash ? ApplicationData + Path.DirectorySeparatorChar : ApplicationData;
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
            if (!HasPortableData) return null;

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
            ApplicationData = path;
        }
    }

    /// <summary>
    /// PatternConverter to use in Log4Net.config files that outputs the installation's application data folder.
    /// </summary>
    [UsedImplicitly]
    public class AppDataPatternConverter : log4net.Util.PatternConverter
    {
        protected override void Convert(TextWriter writer, object state)
        {
            writer.Write(AppData.GetFolder());
        }
    }
}
