namespace PoESkillTree.Computation.Parsing.Builders.Skills
{
    /// <summary>
    /// Factory interface for skills.
    /// </summary>
    public interface ISkillBuilders
    {
        /// <summary>
        /// Gets a collection of all existing skills.
        /// </summary>
        ISkillBuilderCollection Skills { get; }

        /// <summary>
        /// Returns a collection consisting of the given skills.
        /// </summary>
        ISkillBuilderCollection Combine(params ISkillBuilder[] skills);

        // Single skills that need to be individually referenced

        ISkillBuilder SummonSkeleton { get; }
        ISkillBuilder VaalSummonSkeletons { get; }
        ISkillBuilder RaiseSpectre { get; }
        ISkillBuilder RaiseZombie { get; }

        ISkillBuilder DetonateMines { get; }

        ISkillBuilder BloodRage { get; }
        ISkillBuilder MoltenShell { get; }

        ISkillBuilder BoneOffering { get; }
        ISkillBuilder FleshOffering { get; }
        ISkillBuilder SpiritOffering { get; }
    }
}