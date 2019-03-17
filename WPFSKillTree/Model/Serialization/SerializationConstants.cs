using System;
using PoESkillTree.Utils;
using PoESkillTree.Localization;

namespace PoESkillTree.Model.Serialization
{
    /// <summary>
    /// Contains constants used for serialization of build files.
    /// </summary>
    public static class SerializationConstants
    {
        public const string BuildFolderFileName = ".buildfolder";

        public const string BuildFileExtension = ".pbuild";

        public static readonly string DefaultBuildName = L10n.Message("New build");

        public static readonly string EncodedDefaultBuildName = SerializationUtils.EncodeFileName(DefaultBuildName) +
                                                                BuildFileExtension;

        /// <summary>
        /// Current version of the file format used for .buildfolder and .pbuild files.
        /// Increase and handle if there are file format changes needing conversion of old to new files
        /// and/or breaking compatibility to older program versions.
        /// </summary>
        public static readonly Version BuildVersion = new Version("1.0");
    }
}