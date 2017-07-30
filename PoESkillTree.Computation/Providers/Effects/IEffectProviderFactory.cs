namespace PoESkillTree.Computation.Providers.Effects
{
    public interface IEffectProviderFactory
    {
        IStunEffectProvider Stun { get; }

        IKnockbackEffectProvider Knockback { get; }

        IAilmentProviderFactory Ailment { get; }

        IGroundEffectProviderFactory Ground { get; }
    }
}