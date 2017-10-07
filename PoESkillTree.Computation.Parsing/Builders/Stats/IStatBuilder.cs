using PoESkillTree.Computation.Parsing.Builders.Buffs;
using PoESkillTree.Computation.Parsing.Builders.Effects;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Skills;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Parsing.Builders.Stats
{
    public interface IStatBuilder : IResolvable<IStatBuilder>
    {
        // Minimum has no effect if stat has default value 0 and no base modifiers (BaseSet or 
        // BaseAdd). That is necessary to make sure Unarmed and Incinerate can't crit as long they 
        // don't get base crit chance.
        IStatBuilder Minimum { get; } // default value: negative infinity
        IStatBuilder Maximum { get; } // default value: positive infinity

        ValueBuilder Value { get; } // default: 0

        // returned stat has the converted percentage as value
        IStatBuilder ConvertTo(IStatBuilder stat);
        IStatBuilder AddAs(IStatBuilder stat);
        // All modifiers that do not have Form.BaseSet are also applied to stat at percentOfTheirValue
        IFlagStatBuilder ApplyModifiersTo(IStatBuilder stat, IValueBuilder percentOfTheirValue);

        // chance to double Value
        IStatBuilder ChanceToDouble { get; }

        // For Buffs and Auras: some conditions apply to this stat (e.g. the "Attack" from 
        // "Attack Speed"), others specify whether the buff/aura is granted (e.g. "if you've Blocked 
        // Recently"). It has to be decided at some point which conditions to and which don't.
        // That point must be before the conditions are combined into one.
        // Probably as a property inherent in conditions, i.e. decided on condition construction.

        IBuffBuilder ForXSeconds(IValueBuilder seconds);
        // similar to ForXSeconds(), just with the duration set elsewhere
        IBuffBuilder AsBuff { get; }

        IFlagStatBuilder AsAura(params IEntityBuilder[] affectedEntities);

        // add stat to skills instead of the stat applying as is, 
        // e.g. "Auras you Cast grant ... to you and Allies"
        IFlagStatBuilder AddTo(ISkillBuilderCollection skills);
        // add stat to an effect, e.g. "Consecrated Ground you create grants ... to you and Allies"
        IFlagStatBuilder AddTo(IEffectBuilder effect);
    }
}