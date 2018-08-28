namespace PoESkillTree.Computation.Common.Builders.Stats
{
    /// <summary>
    /// Factory interface for pool stats.
    /// </summary>
    public interface IPoolStatBuilders
    {
        IPoolStatBuilder From(Pool pool);
    }
}