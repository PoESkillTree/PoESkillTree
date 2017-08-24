using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Skills;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Parsing.Builders.Buffs
{
    public interface IBuffBuilders
    {
        IBuffBuilder Fortify { get; }
        IBuffBuilder Maim { get; }
        IBuffBuilder Intimidate { get; }
        IBuffBuilder Taunt { get; }
        IBuffBuilder Blind { get; }

        IConfluxBuffBuilderFactory Conflux { get; }

        // TODO this probably needs changes when other skills from items are added
        // stats of the skill starting with "cursed enemies ..." are the (de)buff
        IBuffBuilder Curse(ISkillBuilder skill, ValueBuilder level);

        // source and target: Self
        // user needs to select the currently active step in the rotation
        IBuffRotation Rotation(ValueBuilder duration);

        IBuffBuilderCollection Buffs(IEntityBuilder source = null,
            IEntityBuilder target = null);
    }


    public interface IConfluxBuffBuilderFactory
    {
        IBuffBuilder Igniting { get; }
        IBuffBuilder Shocking { get; }
        IBuffBuilder Chilling { get; }
        IBuffBuilder Elemental { get; }
    }


    public interface IBuffRotation : IFlagStatBuilder
    {
        IBuffRotation Step(ValueBuilder duration, params IBuffBuilder[] buffs);
    }
}