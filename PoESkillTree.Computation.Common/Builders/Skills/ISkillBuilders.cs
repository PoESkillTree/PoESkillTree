namespace PoESkillTree.Computation.Common.Builders.Skills
{
    /// <summary>
    /// Factory interface for skills.
    /// </summary>
    public interface ISkillBuilders
    {
        /// <summary>
        /// Gets a collection of all skills that have the given keyword.
        /// </summary>
        ISkillBuilderCollection this[params IKeywordBuilder[] keywords] { get; }

        // Single skills that need to be individually referenced

        ISkillBuilder SummonSkeleton { get; }
        ISkillBuilder VaalSummonSkeletons { get; }
        ISkillBuilder RaiseSpectre { get; }
        ISkillBuilder RaiseZombie { get; }

        ISkillBuilder DetonateMines { get; }

        ISkillBuilder FromId(string skillId);
    }
}