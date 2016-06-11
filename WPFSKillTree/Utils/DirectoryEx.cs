using System.IO;

namespace POESKillTree.Utils
{
    public static class DirectoryEx
    {
        public static void DeleteIfExists(string path, bool recursive = false)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive);
        }

        public static void MoveOverwriting(string sourceDirName, string destDirName)
        {
            DeleteIfExists(destDirName, true);
            Directory.Move(sourceDirName, destDirName);
        }

        public static void MoveIfExists(string sourceDirName, string destDirName, bool overwrite = false)
        {
            if (!Directory.Exists(sourceDirName))
                return;
            if (overwrite)
                DeleteIfExists(destDirName, true);
            Directory.Move(sourceDirName, destDirName);
        }
    }
}