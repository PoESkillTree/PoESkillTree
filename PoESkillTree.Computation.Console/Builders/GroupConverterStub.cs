using PoESkillTree.Computation.Parsing.Builders.Actions;
using PoESkillTree.Computation.Parsing.Builders.Charges;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
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
        private readonly IConditionBuilders _conditionBuilders;

        public GroupConverterStub(string stringRepresentation, IConditionBuilders conditionBuilders)
            : base(stringRepresentation)
        {
            _conditionBuilders = conditionBuilders;
        }

        public IDamageTypeBuilder AsDamageType =>
            new DamageTypeBuilderStub($"{this}.AsDamageType", _conditionBuilders);

        public IChargeTypeBuilder AsChargeType =>
            new ChargeTypeBuilderStub($"{this}.AsChargeType", _conditionBuilders);

        public IAilmentBuilder AsAilment =>
            new AilmentBuilderStub($"{this}.AsAilment", _conditionBuilders);

        public IKeywordBuilder AsKeyword =>
            new KeywordBuilderStub($"{this}.AsKeyword");

        public IItemSlotBuilder AsItemSlot =>
            new ItemSlotBuilderStub($"{this}.AsItemSlot");

        public ISelfToAnyActionBuilder AsAction =>
            new SelfToAnyActionBuilderStub($"{this}.AsAction", _conditionBuilders);

        public IStatBuilder AsStat =>
            new StatBuilderStub($"{this}.AsStat", _conditionBuilders);

        public IFlagStatBuilder AsFlagStat =>
            new FlagStatBuilderStub($"{this}.AsFlagStat", _conditionBuilders);

        public IPoolStatBuilder AsPoolStat =>
            new PoolStatBuilderStub($"{this}.AsPoolStat", _conditionBuilders);

        public IDamageStatBuilder AsDamageStat =>
            new DamageStatBuilderStub($"{this}.AsDamageStat", _conditionBuilders);

        public ISkillBuilder AsSkill =>
            new SkillBuilderStub($"{this}.AsSkill", _conditionBuilders);
    }
}