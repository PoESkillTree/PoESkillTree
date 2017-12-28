using PoESkillTree.Computation.Parsing.Builders.Skills;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Builders.Buffs
{
    /// <summary>
    /// Represents a collection of buffs.
    /// </summary>
    public interface IBuffBuilderCollection : IBuilderCollection<IBuffBuilder>
    {
        /// <summary>
        /// Gets a stat representing the combined limit of active instances of the buffs in this collection.
        /// </summary>
        IStatBuilder CombinedLimit { get; }

        /// <summary>
        /// Gets a stat representing the effect modifier that is applied to all buffs in this collection.
        /// </summary>
        IStatBuilder Effect { get; }

        /// <summary>
        /// Returns a new collection that includes all buffs in this collection except those originating from any
        /// skill in <paramref name="skills"/>.
        /// </summary>
        IBuffBuilderCollection ExceptFrom(params ISkillBuilder[] skills);

        /// <summary>
        /// Returns a new collection that includes all buffs in this collection that originate from any skill
        /// with the keyword <paramref name="keyword"/>.
        /// </summary>
        IBuffBuilderCollection With(IKeywordBuilder keyword);

        /// <summary>
        /// Returns a new collection that includes all buffs in this collection except those that originate from any 
        /// skill with the keyword <paramref name="keyword"/>.
        /// </summary>
        IBuffBuilderCollection Without(IKeywordBuilder keyword);
    }
}