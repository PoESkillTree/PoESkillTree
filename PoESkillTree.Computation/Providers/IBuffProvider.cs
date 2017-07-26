using System;

namespace PoESkillTree.Computation.Providers
{
    public interface IBuffProvider : IEffectProvider
    {
        IStatProvider EffectIncrease { get; }

        // action to gain/apply the buff
        IActionProvider<ISelfProvider, ITargetProvider> Action { get; }
    }


    public interface IBuffProviderCollection : IProviderCollection<IBuffProvider>
    {
        IStatProvider CombinedLimit { get; }
        IStatProvider EffectIncrease { get; }

        IBuffProviderCollection ExceptFrom(params ISkillProvider[] skills);
    }


    public interface IConfluxBuffProvider
    {
        IBuffProvider Igniting { get; }
        IBuffProvider Shocking { get; }
        IBuffProvider Chilling { get; }
        IBuffProvider Elemental { get; }
    }


    public static class BuffProviders
    {
        public static readonly IBuffProvider Fortify;
        public static readonly IBuffProvider Maim;
        public static readonly IBuffProvider Intimidate;
        public static readonly IBuffProvider Taunt;
        public static readonly IBuffProvider Blind;

        public static readonly IConfluxBuffProvider Conflux;

        // TODO this probably needs changes when other skills from items are added
        // stats of the skill starting with "cursed enemies ..." are the (de)buff
        public static IBuffProvider Curse(ISkillProvider skill, ValueProvider level)
        {
            throw new NotImplementedException();
        }

        public static IBuffProviderCollection Buffs(ITargetProvider source = null, 
            IKeywordProvider withKeyword = null, IKeywordProvider withoutKeyword = null, 
            ITargetProvider target = null)
        {
            throw new NotImplementedException();
        }

        // source and target: Self
        // user needs to select the currently active step in the rotation
        public static IBuffRotation Rotation(ValueProvider duration)
        {
            throw new NotImplementedException();
        }

        public interface IBuffRotation : IFlagStatProvider
        {
            IBuffRotation Step(ValueProvider duration, params IBuffProvider[] buffs);
        }
    }
}