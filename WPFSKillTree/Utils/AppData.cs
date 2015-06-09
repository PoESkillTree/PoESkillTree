using System;
using System.Diagnostics;
using System.IO;

namespace POESKillTree.Utils
{
    // Application data folder API.
    public class AppData
    {
        // The folder name in system folders containing application data.
        // TODO: This could be same as AppId.
        const string AppDataFolderName = "PoESkillTree";
        // The absolute path of application data folder.
        static string ApplicationData;

        // Returns abolute path to appliation data folder (i.e. system folder + AppDataFolderName).
        // If folder doesn't exist, it will be created.
        // If trailingSlash is true, directory separator will be appended to path returned.
        public static string GetFolder(bool trailingSlash = false)
        {
            // Resolve path just once, it doesn't change during process lifetime.
            if (ApplicationData == null)
            {
                if (Debugger.IsAttached)
                {
                    // When debugging, use current directory (i.e. application root path).
                    ApplicationData = Environment.CurrentDirectory;
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

        // Sets ApplicationData path.
        // Use within UnitTest project only!
        public static void SetApplicationData(string path)
        {
            ApplicationData = path;
        }
    }
}
