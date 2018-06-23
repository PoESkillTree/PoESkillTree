using PoESkillTree.Computation.Builders.Conditions;
using PoESkillTree.Computation.Builders.Damage;
using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Builders.Equipment;
using PoESkillTree.Computation.Builders.Forms;
using PoESkillTree.Computation.Builders.Stats;
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
    public abstract class BuilderFactories : IBuilderFactories
    {
        public BuilderFactories()
        {
            var statFactory = new StatFactory();
            ConditionBuilders = new ConditionBuilders(statFactory);
            DamageSourceBuilders = new DamageSourceBuilders();
            EntityBuilders = new EntityBuilders(statFactory);
            EquipmentBuilders = new EquipmentBuilders(statFactory);
            FormBuilders = new FormBuilders();
            ValueBuilders = new ValueBuilders();
            ItemSlotBuilders = new ItemSlotBuilders();
        }

        public abstract IActionBuilders ActionBuilders { get; }
        public abstract IBuffBuilders BuffBuilders { get; }
        public abstract IChargeTypeBuilders ChargeTypeBuilders { get; }
        public IConditionBuilders ConditionBuilders { get; }
        public IDamageSourceBuilders DamageSourceBuilders { get; }
        public abstract IDamageTypeBuilders DamageTypeBuilders { get; }
        public abstract IEffectBuilders EffectBuilders { get; }
        public IEntityBuilders EntityBuilders { get; }
        public IEquipmentBuilders EquipmentBuilders { get; }
        public IFormBuilders FormBuilders { get; }
        public abstract IKeywordBuilders KeywordBuilders { get; }
        public abstract ISkillBuilders SkillBuilders { get; }
        public abstract IStatBuilders StatBuilders { get; }
        public IValueBuilders ValueBuilders { get; }
        public IItemSlotBuilders ItemSlotBuilders { get; }
    }
}