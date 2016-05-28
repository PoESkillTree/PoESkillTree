using Gem = POESKillTree.SkillTreeFiles.ItemDB.Gem;

namespace UpdateDB.DataLoading.Gems
{
    /// <summary>
    /// Interface for classes that extract gems from web sources.
    /// </summary>
    public interface IGemReader
    {
        /// <summary>
        /// Extracts and returns the gem with the given name.
        /// </summary>
        Gem FetchGem(string name);
    }
}
