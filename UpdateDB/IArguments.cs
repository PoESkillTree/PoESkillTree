using System;
using System.Collections.Generic;

namespace UpdateDB
{
    /// <summary>
    /// Specifies the category of a DataLoader and is used to select the ones that are run.
    /// </summary>
    [Flags]
    public enum LoaderCategories
    {
        /// <summary>
        /// Is not used as a category for a loader instance but is used to run all available Loader.
        /// </summary>
        Any = 0,
        /// <summary>
        /// Specifies that a loader produces files that are version controlled.
        /// </summary>
        VersionControlled = 1,
        /// <summary>
        /// Specifies that a loader produces files that are not version controlled.
        /// </summary>
        NotVersionControlled = 2,
        /// <summary>
        /// Is not used as a category for a loader instance but is used to not select any loader by its category.
        /// </summary>
        None = 4
    }

    /// <summary>
    /// Specifies the root directory in which all downloaded files are saved.
    /// </summary>
    public enum OutputDirectory
    {
        /// <summary>
        /// Use <see cref="PoESkillTree.Utils.AppData.GetFolder(bool)"/> as root output directory.
        /// </summary>
        AppData,
        /// <summary>
        /// Use /PoESkillTree/WPFSKillTree as root output directory.
        /// </summary>
        SourceCode,
        /// <summary>
        /// Use the directory from which the executable is called as root output directory.
        /// </summary>
        Current,
        /// <summary>
        /// Use the directory that is explicitly specified.
        /// </summary>
        Specified
    }

    /// <summary>
    /// Used to define the behaviour of a <see cref="DataLoaderExecutor"/> instance.
    /// </summary>
    public interface IArguments
    {
        /// <summary>
        /// Specifies which DataLoaders are activated.
        /// </summary>
        LoaderCategories ActivatedLoaders { get; }

        /// <summary>
        /// Specifies the root output directory.
        /// </summary>
        OutputDirectory OutputDirectory { get; }

        /// <summary>
        /// The output directory used if <see cref="OutputDirectory"/> is
        /// <see cref="UpdateDB.OutputDirectory.Specified"/>.
        /// </summary>
        string SpecifiedOutputDirectory { get; }

        /// <summary>
        /// Specifies which DataLoaders are explicitly activated (independent of <see cref="ActivatedLoaders"/>).
        /// Each string identifies one DataLoader.
        /// </summary>
        IEnumerable<string> LoaderFlags { get; }
    }
}