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

        IBuffProviderCollection With(IKeywordProvider keyword);
        IBuffProviderCollection Without(IKeywordProvider keyword);
    }


    public interface IConfluxBuffProviderFactory
    {
        IBuffProvider Igniting { get; }
        IBuffProvider Shocking { get; }
        IBuffProvider Chilling { get; }
        IBuffProvider Elemental { get; }
    }


    public interface IBuffProviderFactory
    {
        IBuffProvider Fortify { get; }
        IBuffProvider Maim { get; }
        IBuffProvider Intimidate { get; }
        IBuffProvider Taunt { get; }
        IBuffProvider Blind { get; }

        IConfluxBuffProviderFactory Conflux { get; }

        // TODO this probably needs changes when other skills from items are added
        // stats of the skill starting with "cursed enemies ..." are the (de)buff
        IBuffProvider Curse(ISkillProvider skill, ValueProvider level);
    }


    public static class BuffProviders
    {
        public static readonly IBuffProviderFactory Buff;

        public static IBuffProviderCollection Buffs(ITargetProvider source = null, 
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