using System;
using System.IO;
using POESKillTree.Localization;

namespace POESKillTree.Utils
{
    public static class PathEx
    {
        public static string TrimTrailingDirectorySeparators(string path)
        {
            return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        public static bool IsPathValid(string path, out string errorMessage, bool canExist = true,
            bool mustBeFile = false, bool mustBeDirectory = false)
        {
            errorMessage = null;
            if (string.IsNullOrEmpty(path))
            {
                errorMessage = L10n.Message("Value is required.");
            }
            else if (string.IsNullOrWhiteSpace(path))
            {
                errorMessage = L10n.Message("Value only contains white space characters.");
            }
            else
            {
                try
                {
                    var fi = new FileInfo(path);
                    if (fi.Exists)
                    {
                        if (!canExist)
                        {
                            errorMessage = L10n.Message("Path already exists.");
                        }
                        else if (mustBeDirectory && File.Exists(path))
                        {
                            errorMessage = L10n.Message("Path exists and is not a directory.");
                        }
                        else if (mustBeFile && Directory.Exists(path))
                        {
                            errorMessage = L10n.Message("Path already exists as a directory.");
                        }
                    }
                }
                catch (ArgumentException)
                {
                    errorMessage = L10n.Message("Value contains invalid characters.");
                }
                catch (UnauthorizedAccessException)
                {
                    errorMessage = L10n.Message("Path could not be accessed.");
                }
                catch (PathTooLongException)
                {
                    errorMessage = L10n.Message("Path is too long.");
                }
                catch (NotSupportedException)
                {
                    errorMessage = L10n.Message("Value contains a colon at an invalid position-");
                }
            }
            return errorMessage == null;
        }
    }
}