using PoESkillTree.Computation.Providers.Buffs;
using PoESkillTree.Computation.Providers.Effects;
using PoESkillTree.Computation.Providers.Skills;
using PoESkillTree.Computation.Providers.Values;

namespace PoESkillTree.Computation.Providers.Stats
{
    public interface IStatProvider
    {
        // Minimum has no effect if stat has default value 0 and no base modifiers (BaseSet or 
        // BaseAdd). That is necessary to make sure Unarmed and Incinerate can't crit as long they 
        // don't get base crit chance.
        IStatProvider Minimum { get; } // default value: negative infinity
        IStatProvider Maximum { get; } // default value: positive infinity

        ValueProvider Value { get; } // default: 0

        // returned stat has the converted percentage as value
        IStatProvider ConvertTo(IStatProvider stat);
        IStatProvider AddAs(IStatProvider stat);
        // All modifiers that do not have Form.BaseSet are also applied to stat at percentOfTheirValue
        IFlagStatProvider ApplyModifiersTo(IStatProvider stat, ValueProvider percentOfTheirValue);

        // chance to double Value
        IStatProvider ChanceToDouble { get; }

        IBuffProvider ForXSeconds(ValueProvider seconds);
        // similar to ForXSeconds(), just with the duration set elsewhere
        IBuffProvider AsBuff { get; }
        IFlagStatProvider AsAura { get; }

        // add stat to skills instead of the stat applying as is, 
        // e.g. "Auras you Cast grant ... to you and Allies"
        IFlagStatProvider AddTo(ISkillProviderCollection skills);
        // add stat to an effect, e.g. "Consecrated Ground you create grants ... to you and Allies"
        IFlagStatProvider AddTo(IEffectProvider effect);
    }
}