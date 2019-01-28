namespace PoESkillTree.Computation.Common.Builders.Stats
{
    public interface ISkillEntityStatBuilders
    {
        /// <summary>
        /// Gets a stat representing the speed with which this entity is placed/thrown/laid.
        /// </summary>
        IStatBuilder Speed { get; }

        /// <summary>
        /// The duration this entity lasts in seconds.
        /// </summary>
        IStatBuilder Duration { get; }
    }

    public interface ITrapStatBuilders : ISkillEntityStatBuilders
    {
        IStatBuilder TriggerAoE { get; }
    }

    public interface IMineStatBuilders : ISkillEntityStatBuilders
    {
        IStatBuilder DetonationAoE { get; }
    }
}