namespace PoESkillTree.Computation.Parsing.Builders.Effects
{
    public interface IEffectBuilders
    {
        IStunEffectBuilder Stun { get; }

        IKnockbackEffectBuilder Knockback { get; }

        IAilmentBuilders Ailment { get; }

        IGroundEffectBuilders Ground { get; }
    }
}