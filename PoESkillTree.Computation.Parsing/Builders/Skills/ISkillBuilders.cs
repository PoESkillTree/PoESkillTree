namespace PoESkillTree.Computation.Parsing.Builders.Skills
{
    public interface ISkillBuilders
    {
        // All available skills
        ISkillBuilderCollection Skills { get; }

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