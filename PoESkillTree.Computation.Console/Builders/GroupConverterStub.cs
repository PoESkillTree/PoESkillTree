using PoESkillTree.Computation.Parsing.Builders.Actions;
using PoESkillTree.Computation.Parsing.Builders.Charges;
using PoESkillTree.Computation.Parsing.Builders.Damage;
using PoESkillTree.Computation.Parsing.Builders.Effects;
using PoESkillTree.Computation.Parsing.Builders.Equipment;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Skills;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Console.Builders
{
    public class GroupConverterStub : BuilderStub, IGroupConverter
    {
        public GroupConverterStub(string stringRepresentation)
            : base(stringRepresentation)
        {
        }

        public IDamageTypeBuilder AsDamageType =>
            new DamageTypeBuilderStub($"{this}.AsDamageType");

        public IChargeTypeBuilder AsChargeType =>
            new ChargeTypeBuilderStub($"{this}.AsChargeType");

        public IAilmentBuilder AsAilment =>
            new AilmentBuilderStub($"{this}.AsAilment");

        public IKeywordBuilder AsKeyword =>
            new KeywordBuilderStub($"{this}.AsKeyword");

        public IItemSlotBuilder AsItemSlot =>
            new ItemSlotBuilderStub($"{this}.AsItemSlot");

        public ISelfToAnyActionBuilder AsAction =>
            new SelfToAnyActionBuilderStub($"{this}.AsAction");

        public IStatBuilder AsStat =>
            new StatBuilderStub($"{this}.AsStat");

        public IFlagStatBuilder AsFlagStat =>
            new FlagStatBuilderStub($"{this}.AsFlagStat");

        public IPoolStatBuilder AsPoolStat =>
            new PoolStatBuilderStub($"{this}.AsPoolStat");

        public IDamageStatBuilder AsDamageStat =>
            new DamageStatBuilderStub($"{this}.AsDamageStat");

        public ISkillBuilder AsSkill =>
            new SkillBuilderStub($"{this}.AsSkill");
    }
}