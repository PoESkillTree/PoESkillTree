using System;
using System.IO;

namespace PoESkillTree.Utils
{
    public static class DirectoryEx
    {
        /// <summary>
        /// Recursive directory deletion that handles directories being opened in Windows Explorer.
        /// Source: http://stackoverflow.com/a/1703799
        /// </summary>
        private static void DeleteRecursive(string path)
        {
            foreach (var directory in Directory.GetDirectories(path))
            {
                DeleteRecursive(directory);
            }

            try
            {
                Directory.Delete(path, true);
            }
            catch (IOException)
            {
                Directory.Delete(path, true);
            }
            catch (UnauthorizedAccessException)
            {
                Directory.Delete(path, true);
            }
        }

        public static void DeleteIfExists(string path, bool recursive = false)
        {
            if (Directory.Exists(path))
            {
                if (recursive)
                    DeleteRecursive(path);
                else
                    Directory.Delete(path, false);
            }
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