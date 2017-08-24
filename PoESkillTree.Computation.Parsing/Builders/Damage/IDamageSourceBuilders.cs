namespace PoESkillTree.Computation.Parsing.Builders.Damage
{
    public interface IDamageSourceBuilders
    {
        IDamageSourceBuilder Attack { get; }
        IDamageSourceBuilder Spell { get; }
        IDamageSourceBuilder Secondary { get; }
        IDamageSourceBuilder DamageOverTime { get; }
    }
}