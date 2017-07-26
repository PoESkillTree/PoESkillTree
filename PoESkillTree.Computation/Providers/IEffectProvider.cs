namespace PoESkillTree.Computation.Providers
{
    public interface IEffectProvider
    {
        IFlagStatProvider On(ITargetProvider target);

        // needs to be entered by user if this sets On(target) to 1?
        // (default action is Hit if non is specified)
        IStatProvider ChanceOn(ITargetProvider target);

        // shortcut for On(target).IsSet
        IConditionProvider IsOn(ITargetProvider target);

        // duration when source is Self
        IStatProvider Duration { get; }
    }


    public interface IAvoidableEffectProvider : IEffectProvider
    {
        IStatProvider Avoidance { get; }
    }


    public interface IAilmentProvider : IAvoidableEffectProvider
    {
        // shortcut for ChanceOn(Enemy)
        IStatProvider Chance { get; }

        // default value is 1 for everything except bleed
        // default value is positive infinity for bleed
        IStatProvider InstancesOn(ITargetProvider target);

        IFlagStatProvider AddSource(IDamageTypeProvider type);
    }


    public interface IDamagingAilmentProvider : IAilmentProvider
    {

    }


    public interface IStunProvider : IAvoidableEffectProvider, 
        IActionProvider<ISelfProvider, IEnemyProvider>
    {
        IStatProvider Threshold { get; }

        IStatProvider Recovery { get; }

        IStatProvider ChanceToAvoidInterruptionWhileCasting { get; }
    }


    public interface IKnockbackProvider : IEffectProvider
    {
        IStatProvider Distance { get; }
    }


    public static class EffectProviders
    {
        public static readonly IDamagingAilmentProvider Ignite;
        public static readonly IAilmentProvider Shock;
        public static readonly IAilmentProvider Chill;
        public static readonly IAilmentProvider Freeze;

        public static readonly IDamagingAilmentProvider Bleed;
        public static readonly IDamagingAilmentProvider Poison;

        public static readonly IAilmentProvider ElementalAilment;
        public static readonly IAilmentProvider AnyAilment;

        public static readonly IStunProvider Stun;
        public static readonly IKnockbackProvider Knockback;
    }
}