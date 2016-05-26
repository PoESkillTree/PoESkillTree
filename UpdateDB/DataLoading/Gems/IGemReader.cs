using Gem = POESKillTree.SkillTreeFiles.ItemDB.Gem;

namespace UpdateDB.DataLoading.Gems
{
    // The base class of gem data reader.
    public interface IGemReader
    {
        // Returns gem data.
        Gem FetchGem(string name);
    }
}
