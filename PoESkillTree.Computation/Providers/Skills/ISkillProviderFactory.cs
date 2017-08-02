namespace PoESkillTree.Computation.Providers.Skills
{
    public interface ISkillProviderFactory
    {
        // All available skills
        ISkillProviderCollection Skills { get; }

        ISkillProviderCollection Combine(params ISkillProvider[] skills);

        // Single skills that need to be individually referenced

        ISkillProvider SummonSkeleton { get; }
        ISkillProvider VaalSummonSkeletons { get; }
        ISkillProvider RaiseSpectre { get; }
        ISkillProvider RaiseZombie { get; }

        ISkillProvider DetonateMines { get; }

        ISkillProvider BloodRage { get; }
        ISkillProvider MoltenShell { get; }

        ISkillProvider BoneOffering { get; }
        ISkillProvider FleshOffering { get; }
        ISkillProvider SpiritOffering { get; }
    }
}