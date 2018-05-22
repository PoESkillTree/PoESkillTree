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

namespace PoESkillTree.Computation.Console.Builders
{
    /*
     * This namespace contains stub implementations of the interfaces in PoESkillTree.Computation.Parsing.Builders.
     *
     * The implementations are really simple, they just build a string representing the called method/property chain.
     *
     * The fact that implementations must support resolving of references and values (see IResolvable<T>) and that
     * the things to resolve are generally at the start of method chains, requires all implementations to pass
     * the resolve calls up and build another string after resolving. This complicates things a little, but the
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

        public IFormBuilders FormBuilders => new FormBuilders();

        public IKeywordBuilders KeywordBuilders => new KeywordBuildersStub();

        public ISkillBuilders SkillBuilders => new SkillBuildersStub();

        public IStatBuilders StatBuilders => new StatBuildersStub();

        public IValueBuilders ValueBuilders => new ValueBuilders();

        public IItemSlotBuilders ItemSlotBuilders => new ItemSlotBuildersStub();
    }
}