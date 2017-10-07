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
            new DamageTypeBuilderStub($"{this}.AsDamageType", (c, _) => c);

        public IChargeTypeBuilder AsChargeType =>
            new ChargeTypeBuilderStub($"{this}.AsChargeType", (c, _) => c);

        public IAilmentBuilder AsAilment =>
            new AilmentBuilderStub($"{this}.AsAilment", (c, _) => c);

        public IKeywordBuilder AsKeyword =>
            new KeywordBuilderStub($"{this}.AsKeyword", (c, _) => c);

        public IItemSlotBuilder AsItemSlot =>
            new ItemSlotBuilderStub($"{this}.AsItemSlot", (c, _) => c);

        public ISelfToAnyActionBuilder AsAction =>
            new SelfToAnyActionBuilderStub($"{this}.AsAction", (c, _) => c);

        public IStatBuilder AsStat =>
            new StatBuilderStub($"{this}.AsStat", (c, _) => c);

        public IFlagStatBuilder AsFlagStat =>
            new FlagStatBuilderStub($"{this}.AsFlagStat", (c, _) => c);

        public IPoolStatBuilder AsPoolStat =>
            new PoolStatBuilderStub($"{this}.AsPoolStat", (c, _) => c);

        public IDamageStatBuilder AsDamageStat =>
            new DamageStatBuilderStub($"{this}.AsDamageStat", (c, _) => c);

        public ISkillBuilder AsSkill =>
            new SkillBuilderStub($"{this}.AsSkill", (c, _) => c);
    }
}