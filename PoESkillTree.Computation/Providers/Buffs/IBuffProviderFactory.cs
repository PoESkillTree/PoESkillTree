using PoESkillTree.Computation.Providers.Skills;
using PoESkillTree.Computation.Providers.Entities;
using PoESkillTree.Computation.Providers.Stats;
using PoESkillTree.Computation.Providers.Values;

namespace PoESkillTree.Computation.Providers.Buffs
{
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

        // source and target: Self
        // user needs to select the currently active step in the rotation
        IBuffRotation Rotation(ValueProvider duration);

        IBuffProviderCollection Buffs(IEntityProvider source = null,
            IEntityProvider target = null);
    }


    public interface IConfluxBuffProviderFactory
    {
        IBuffProvider Igniting { get; }
        IBuffProvider Shocking { get; }
        IBuffProvider Chilling { get; }
        IBuffProvider Elemental { get; }
    }


    public interface IBuffRotation : IFlagStatProvider
    {
        IBuffRotation Step(ValueProvider duration, params IBuffProvider[] buffs);
    }
}