using System.Net.Http;
using System.Threading.Tasks;
using Gem = POESKillTree.SkillTreeFiles.ItemDB.Gem;

namespace UpdateDB.DataLoading.Gems
{
    /// <summary>
    /// Interface for classes that extract gems from web sources.
    /// </summary>
    public interface IGemReader
    {
        HttpClient HttpClient { set; }

        /// <summary>
        /// Extracts and returns the gem with the given name.
        /// </summary>
        Task<Gem> FetchGemAsync(string name);
    }
}
