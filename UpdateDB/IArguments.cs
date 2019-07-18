using System.Collections.Generic;

namespace UpdateDB
{
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
        /// Specifies the root output directory.
        /// </summary>
        OutputDirectory OutputDirectory { get; }

        /// <summary>
        /// The output directory used if <see cref="OutputDirectory"/> is
        /// <see cref="UpdateDB.OutputDirectory.Specified"/>.
        /// </summary>
        string SpecifiedOutputDirectory { get; }

        /// <summary>
        /// Specifies which DataLoaders are explicitly activated.
        /// Each string identifies one DataLoader.
        /// </summary>
        IEnumerable<string> LoaderFlags { get; }
    }
}