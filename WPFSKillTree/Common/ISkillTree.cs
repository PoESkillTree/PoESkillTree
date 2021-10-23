using PoESkillTree.SkillTreeFiles;
using PoESkillTree.Utils.UrlProcessing;

namespace PoESkillTree.Common
{
    /// <summary>
    /// Interface for the main SkillTree class to reduce dependencies on the SkillTreeFiles namespace.
    /// </summary>
    public interface ISkillTree
    {
        /// <summary>
        /// Gets ascendancy classes helper.
        /// </summary>
        IAscendancyClasses AscendancyClasses { get; }
        
        /// <summary>
        /// Gets an encoded skill tree url of the current tree.
        /// </summary>
        /// <returns>encoded skill tree url</returns>
        public string EncodeUrl();

        /// <summary>
        /// Gets an encoded skill tree url of the current tree.
        /// </summary>
        /// <returns>encoded skill tree url</returns>
        public string EncodeUrl(SkillTreeUrlData data);

        /// <summary>
        /// Decodes skill tree data from a given url.
        /// </summary>
        /// <param name="url">normalized path of exile skill tree url</param>
        /// <returns>Skill tree data</returns>
        public SkillTreeUrlData DecodeUrl(string url);
    }
}