using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Buffs;
using PoESkillTree.Computation.Common.Builders.Charges;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Equipment;
using PoESkillTree.Computation.Common.Builders.Forms;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Common.Builders
{
    // "A builder representing ..." is an implied part of most method documentation of Builders
    // to make it easier to read from the point of view of someone adding entries in the Computation.Data project.

    /// <summary>
    /// Contains all IXBuilders as properties that are not themselves part of other IXBuilders. Therefore provides
    /// access to all interfaces in the Builders namespace.
    /// </summary>
    public interface IBuilderFactories
    {
        IActionBuilders ActionBuilders { get; }

        IBuffBuilders BuffBuilders { get; }

        IChargeTypeBuilders ChargeTypeBuilders { get; }

        IConditionBuilders ConditionBuilders { get; }

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