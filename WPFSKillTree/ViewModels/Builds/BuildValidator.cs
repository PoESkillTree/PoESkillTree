using System.IO;
using System.Linq;
using POESKillTree.Localization;
using POESKillTree.Model;
using POESKillTree.Model.Serialization;
using POESKillTree.Utils;

namespace POESKillTree.ViewModels.Builds
{
    /// <summary>
    /// Used to validate build names and to decide whether builds can be moved into a folder or whether
    /// a folder can have subfolders.
    /// </summary>
    public class BuildValidator
    {
        /// <summary>
        /// The maximum depth of folder nesting. Includes the root folder.
        /// </summary>
        private const int MaxBuildDepth = 4;

        private readonly Options _options;

        public BuildValidator(Options options)
        {
            _options = options;
        }

        /// <summary>
        /// Returns true iff the <paramref name="folder"/> can have subfolders. There can only be a constant number
        /// of folders nested into each other.
        /// </summary>
        public bool CanHaveSubfolder(IBuildFolderViewModel folder)
        {
            return GetDepthOfFolder(folder) < MaxBuildDepth;
        }

        /// <summary>
        /// Returns true iff <paramref name="build"/> can be moved to <paramref name="parent"/>.
        /// </summary>
        public bool CanMoveTo(IBuildViewModel build, IBuildFolderViewModel parent)
        {
            if (!IsNameUnique(build.Build.Name, build, parent))
                return false;

            var parentPath = BasePathFor(parent);
            var folder = build as IBuildFolderViewModel;
            if (folder == null)
            {
                return CanBeChildOf(build, parentPath);
            }
            else
            {
                var parentDepth = GetDepthOfFolder(parent);
                return CanBeChildOf(folder, parentPath, parentDepth);
            }
        }

        private static bool CanBeChildOf(IBuildFolderViewModel build, string parentPath, int parentDepth)
        {
            // Check depth and name
            if (parentDepth >= MaxBuildDepth)
                return false;
            var name = build.Build.Name;
            if (!IsNameValid(name, Path.Combine(parentPath, FileNameForFolder(name))))
                return false;
            // Check all subbuilds
            var nextPath = Path.Combine(parentPath, SerializationUtils.EncodeFileName(name));
            var nextDepth = parentDepth + 1;
            foreach (var child in build.Children)
            {
                var folder = child as IBuildFolderViewModel;
                if (folder == null)
                {
                    if (!CanBeChildOf(child, nextPath))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!CanBeChildOf(folder, nextPath, nextDepth))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool CanBeChildOf(IBuildViewModel build, string parentPath)
        {
            var name = build.Build.Name;
            return IsNameValid(name, Path.Combine(parentPath, FileNameForBuild(name)));
        }

        private static int GetDepthOfFolder(IBuildFolderViewModel folder)
        {
            return folder.Parent == null ? 1 : 1 + GetDepthOfFolder(folder.Parent);
        }

        /// <summary>
        /// Returns an string that contains an error message if it is not null or empty that describes
        /// why <paramref name="name"/> is not allowed as a name for <paramref name="folder"/>.
        /// </summary>
        public string ValidateExistingFolderName(string name, IBuildViewModel folder)
        {
            return ValidateName(name, PathForFolder(name, folder.Parent), folder, folder.Parent);
        }

        /// <summary>
        /// Returns an string that contains an error message if it is not null or empty that describes
        /// why <paramref name="name"/> is not allowed as a name for a new subfolder of <paramref name="parent"/>.
        /// </summary>
        public string ValidateNewFolderName(string name, IBuildFolderViewModel parent)
        {
            return ValidateName(name, PathForFolder(name, parent), null, parent);
        }

        /// <summary>
        /// Returns an string that contains an error message if it is not null or empty that describes
        /// why <paramref name="name"/> is not allowed as a name for <paramref name="build"/>.
        /// </summary>
        public string ValidateExistingFileName(string name, IBuildViewModel build)
        {
            return ValidateName(name, PathForBuild(name, build.Parent), build, build.Parent);
        }

        /// <summary>
        /// Returns an string that contains an error message if it is not null or empty that describes
        /// why <paramref name="name"/> is not allowed as a name for a new build located in <paramref name="parent"/>.
        /// </summary>
        public string ValidateNewBuildName(string name, IBuildFolderViewModel parent)
        {
            return ValidateName(name, PathForBuild(name, parent), null, parent);
        }

        private static string ValidateName(string name, string fullPath, IBuildViewModel build,
            IBuildFolderViewModel parent)
        {
            if (!IsNameUnique(name, build, parent))
            {
                return L10n.Message("A build or folder with this name already exists.");
            }
            string message;
            IsNameValid(name, fullPath, out message);
            return message;
        }

        private static bool IsNameUnique(string name, IBuildViewModel build, IBuildFolderViewModel parent)
        {
            return parent.Children.All(b => b == build || b.Build.Name != name);
        }

        private static bool IsNameValid(string name, string fullPath)
        {
            string dummy;
            return IsNameValid(name, fullPath, out dummy);
        }

        private static bool IsNameValid(string name, string fullPath, out string errorMessage)
        {
            if (string.IsNullOrEmpty(name))
            {
                errorMessage = L10n.Message("Value is required.");
                return false;
            }
            return PathEx.IsPathValid(fullPath, out errorMessage, mustBeFile: true);
        }

        private static string FileNameForBuild(string name)
        {
            return SerializationUtils.EncodeFileName(name) + SerializationConstants.BuildFileExtension;
        }

        private string PathForBuild(string name, IBuildViewModel parent)
        {
            return Path.Combine(BasePathFor(parent), FileNameForBuild(name));
        }

        private static string FileNameForFolder(string name)
        {
            return Path.Combine(SerializationUtils.EncodeFileName(name), SerializationConstants.BuildFolderFileName);
        }

        private string PathForFolder(string name, IBuildViewModel parent)
        {
            return Path.Combine(BasePathFor(parent), FileNameForFolder(name));
        }

        private string BasePathFor(IBuildViewModel build)
        {
            var fileName = SerializationUtils.EncodeFileName(build.Build.Name);
            return build.Parent == null
                ? _options.BuildsSavePath // Root folder is not directly part of the path
                : Path.Combine(BasePathFor(build.Parent), fileName);
        }
    }
}