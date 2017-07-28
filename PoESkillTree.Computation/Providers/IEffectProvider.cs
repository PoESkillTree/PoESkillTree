using System.Collections.Generic;

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


    public interface IAilmentProviderCollection : IProviderCollection<IAilmentProvider>
    {
        
    }


    public interface IAilmentProviderFactory
    {
        IDamagingAilmentProvider Ignite { get; }
        IAilmentProvider Shock { get; }
        IAilmentProvider Chill { get; }
        IAilmentProvider Freeze { get; }

        IDamagingAilmentProvider Bleed { get; }
        IDamagingAilmentProvider Poison { get; }

        IAilmentProviderCollection All { get; }
        IAilmentProviderCollection Elemental { get; }
        IProviderCollection<IDamagingAilmentProvider> Damaging { get; }
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


    public interface IEffectProviderFactory
    {
        IStunProvider Stun { get; }

        IKnockbackProvider Knockback { get; }
    }


    public static class EffectProviders
    {
        public static readonly IAilmentProviderFactory Ailment;

        public static readonly IEffectProviderFactory Effect;
    }
}