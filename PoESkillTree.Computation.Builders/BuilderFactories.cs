using PoESkillTree.Computation.Builders.Equipment;
using PoESkillTree.Computation.Builders.Forms;
using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common.Builders;
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

namespace PoESkillTree.Computation.Builders
{
    public class BuilderFactories : IBuilderFactories
    {
        public IActionBuilders ActionBuilders { get; }
        public IBuffBuilders BuffBuilders { get; }
        public IChargeTypeBuilders ChargeTypeBuilders { get; }
        public IConditionBuilders ConditionBuilders { get; }
        public IDamageSourceBuilders DamageSourceBuilders { get; }
        public IDamageTypeBuilders DamageTypeBuilders { get; }
        public IEffectBuilders EffectBuilders { get; }
        public IEntityBuilders EntityBuilders { get; }
        public IEquipmentBuilders EquipmentBuilders { get; }
        public IFormBuilders FormBuilders { get; } = new FormBuilders();
        public IKeywordBuilders KeywordBuilders { get; }
        public ISkillBuilders SkillBuilders { get; }
        public IStatBuilders StatBuilders { get; }
        public IValueBuilders ValueBuilders { get; } = new ValueBuilders();
        public IItemSlotBuilders ItemSlotBuilders { get; } = new ItemSlotBuilders();
    }
}