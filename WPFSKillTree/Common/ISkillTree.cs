using POESKillTree.SkillTreeFiles;
using POESKillTree.Utils.UrlProcessing;

namespace POESKillTree.Common
{
    /// <summary>
    /// Interface for the main SkillTree class to reduce dependencies on the SkillTreeFiles namespace.
    /// </summary>
    public interface ISkillTree
    {
        /// <summary>
        /// Gets the build converter used to instantiate build deserializers.
        /// </summary>
        IBuildConverter BuildConverter { get; }

        /// <summary>
        /// Gets ascendancy classes helper.
        /// </summary>
        IAscendancyClasses AscendancyClasses { get; }
    }
}