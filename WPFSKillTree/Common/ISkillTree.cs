namespace POESKillTree.Common
{
    /// <summary>
    /// Interface for the main SkillTree class to reduce dependencies on the SkillTreeFiles namespace.
    /// </summary>
    public interface ISkillTree
    {
        /// <summary>
        /// Returns the number of non-ascendancy points the given tree url uses.
        /// </summary>
        uint PointsUsed(string treeUrl);

        /// <summary>
        /// Returns the character class of the given tree url.
        /// </summary>
        string CharacterClass(string treeUrl);

        /// <summary>
        /// Returns the ascendancy class of the given tree url.
        /// </summary>
        string AscendancyClass(string treeUrl);
    }
}