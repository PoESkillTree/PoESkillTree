using PoESkillTree.Computation.Parsing.Builders.Actions;
using PoESkillTree.Computation.Parsing.Builders.Buffs;
using PoESkillTree.Computation.Parsing.Builders.Charges;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Damage;
using PoESkillTree.Computation.Parsing.Builders.Effects;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Equipment;
using PoESkillTree.Computation.Parsing.Builders.Forms;
using PoESkillTree.Computation.Parsing.Builders.Skills;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Parsing.Builders
{
    // "A builder representing ..." is an implied part of most method documentation of Builders
    // to make it easier to read from the point of view of someone adding entries in the Computation.Data project.

    /// <summary>
    /// Contains all IXBuilders as properties that are not themself part of other IXBuilders. Therefore provides
    /// access to all interfaces in the Builders namespace.
    /// </summary>
    public interface IBuilderFactories
    {
        IActionBuilders ActionBuilders { get; }

        IBuffBuilders BuffBuilders { get; }

        IChargeTypeBuilders ChargeTypeBuilders { get; }

        IConditionBuilders ConditionBuilders { get; }

        IDamageSourceBuilders DamageSourceBuilders { get; }

        IDamageTypeBuilders DamageTypeBuilders { get; }

        IEffectBuilders EffectBuilders { get; }

        IEntityBuilders EntityBuilders { get; }

        IEquipmentBuilders EquipmentBuilders { get; }

        IFormBuilders FormBuilders { get; }

        IKeywordBuilders KeywordBuilders { get; }

        ISkillBuilders SkillBuilders { get; }

        IStatBuilders StatBuilders { get; }

        IValueBuilders ValueBuilders { get; }

        IItemSlotBuilders ItemSlotBuilders { get; }
    }
}