using PoESkillTree.Computation.Builders.Actions;
using PoESkillTree.Computation.Builders.Buffs;
using PoESkillTree.Computation.Builders.Charges;
using PoESkillTree.Computation.Builders.Conditions;
using PoESkillTree.Computation.Builders.Damage;
using PoESkillTree.Computation.Builders.Effects;
using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Builders.Equipment;
using PoESkillTree.Computation.Builders.Forms;
using PoESkillTree.Computation.Builders.Skills;
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
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Builders
{
    public class BuilderFactories : IBuilderFactories
    {
        public BuilderFactories(IStatFactory statFactory, SkillDefinitions skills)
        {
            ActionBuilders = new ActionBuilders(statFactory);
            BuffBuilders = new BuffBuilders(statFactory, skills);
            ChargeTypeBuilders = new ChargeTypeBuilders(statFactory);
            ConditionBuilders = new ConditionBuilders(statFactory);
            DamageTypeBuilders = new DamageTypeBuilders(statFactory);
            EffectBuilders = new EffectBuilders(statFactory);
            EntityBuilders = new EntityBuilders(statFactory);
            EquipmentBuilders = new EquipmentBuilders(statFactory);
            FormBuilders = new FormBuilders();
            KeywordBuilders = new KeywordBuilders();
            SkillBuilders = new SkillBuilders(statFactory, skills);
            StatBuilders = new StatBuilders(statFactory);
            ValueBuilders = new ValueBuilders();
            ItemSlotBuilders = new ItemSlotBuilders();
        }

        public IActionBuilders ActionBuilders { get; }
        public IBuffBuilders BuffBuilders { get; }
        public IChargeTypeBuilders ChargeTypeBuilders { get; }
        public IConditionBuilders ConditionBuilders { get; }
        public IDamageTypeBuilders DamageTypeBuilders { get; }
        public IEffectBuilders EffectBuilders { get; }
        public IEntityBuilders EntityBuilders { get; }
        public IEquipmentBuilders EquipmentBuilders { get; }
        public IFormBuilders FormBuilders { get; }
        public IKeywordBuilders KeywordBuilders { get; }
        public ISkillBuilders SkillBuilders { get; }
        public IStatBuilders StatBuilders { get; }
        public IValueBuilders ValueBuilders { get; }
        public IItemSlotBuilders ItemSlotBuilders { get; }
    }
}