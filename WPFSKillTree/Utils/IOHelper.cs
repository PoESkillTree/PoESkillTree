using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace POESKillTree.Utils
{
    public class IOHelper
    {
        public static void CopyDirectory(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyDirectoryData(diSource, diTarget);
        }

        private static void CopyDirectoryData(DirectoryInfo source, DirectoryInfo target)
        {
            // Check if the target directory exists; if not, create it.
            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyDirectoryData(diSourceSubDir, nextTargetSubDir);
            }
        }

        // backup folder to "<sourcePath>_backup_<currentdate>_<prefix>"
        public static bool BackupDirectory(string sourcePath)
        {
            try
            {
                int iter = 1;
                string folderPrefix = @"";
                string backupFolderName = sourcePath + @"_backup_" + DateTime.Today.ToString("dd.MM.yyyy");

                // check if backup folder already exists, if so, add prefix "_v<number>", then check again
                while (Directory.Exists(backupFolderName + folderPrefix))
                {
                    folderPrefix = @"_v" + iter++;
                }

                // do actual copying
                CopyDirectory(sourcePath, backupFolderName + folderPrefix);
                return true;
            }
            catch
            {
                //something went wrong
                return false;
            }
        }

        //look for a folder <sourcePath_backup> with latest date. empty string if nothing found
        public static string GetLatestBackupFolder(string sourcePath)
        {
            string[] backupList = Directory.GetDirectories(@".", @sourcePath + @"_backup*");

            if (backupList.Length > 0 )
            {
                //sort names descending
                var dirs = from dir in Directory.GetDirectories(@".", @sourcePath + @"_backup*")
                    orderby dir descending
                    select dir;

                //return most recent one
                return Regex.Replace(dirs.First(), @".\\", "");
            } 
            else
            { //no backup found. to bad
                return "";
            }
        }
    }
}
