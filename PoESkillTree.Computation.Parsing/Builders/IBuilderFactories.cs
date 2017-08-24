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
    }
}