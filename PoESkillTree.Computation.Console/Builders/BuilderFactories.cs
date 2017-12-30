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
    /*
     * This namespace contains stub implementations of the interfaces in PoESkillTree.Computation.Parsing.Builders.
     *
     * The implementations are really simple, they just build a string representing the called method/property chain.
     *
     * The fact that implementations must support resolving of references and values (see IResolvable<T>) and that
     * the things to resolve are generally at the start of method chains, requires all implementations to pass
     * the resolve calls up and build another string after resolving. This complicates things a litte, but the
     * details are hidden in the utility functions provided by BuilderFactory. Except for some IActionBuilder creations,
     * as it has additional parameters with its Source and Target.
     *
     * MatchContextStub and ReferenceConverterStub are the classes where resolving is not just passing the call on.
     */

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