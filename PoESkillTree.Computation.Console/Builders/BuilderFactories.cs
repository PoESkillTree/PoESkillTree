using PoESkillTree.Computation.Parsing.Builders;
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

namespace PoESkillTree.Computation.Console.Builders
{
    public class BuilderFactories : IBuilderFactories
    {
        public IActionBuilders ActionBuilders => new ActionBuildersStub();

        public IBuffBuilders BuffBuilders => new BuffBuildersStub();

        public IChargeTypeBuilders ChargeTypeBuilders => new ChargeTypeBuildersStub();

        public IConditionBuilders ConditionBuilders => new ConditionBuildersStub();

        public IDamageSourceBuilders DamageSourceBuilders => new DamageSourceBuildersStub();

        public IDamageTypeBuilders DamageTypeBuilders => new DamageTypeBuildersStub();

        public IEffectBuilders EffectBuilders => new EffectBuildersStub();

        public IEntityBuilders EntityBuilders => new EntityBuildersStub();

        public IEquipmentBuilders EquipmentBuilders => new EquipmentBuildersStub();

        public IFormBuilders FormBuilders => new FormBuildersStub();

        public IKeywordBuilders KeywordBuilders => new KeywordBuildersStub();

        public ISkillBuilders SkillBuilders => new SkillBuildersStub();

        public IStatBuilders StatBuilders => new StatBuildersStub();

        public IValueBuilders ValueBuilders => new ValueBuildersStub();

        public IItemSlotBuilders ItemSlotBuilders => new ItemSlotBuildersStub();
    }
}