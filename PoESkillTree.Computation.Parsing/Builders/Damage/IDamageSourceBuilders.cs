namespace PoESkillTree.Computation.Parsing.Builders.Damage
{
    /// <summary>
    /// Factory interface for damage sources.
    /// </summary>
    public interface IDamageSourceBuilders
    {
        IDamageSourceBuilder Attack { get; }
        IDamageSourceBuilder Spell { get; }
        IDamageSourceBuilder Secondary { get; }
        IDamageSourceBuilder DamageOverTime { get; }
    }
}