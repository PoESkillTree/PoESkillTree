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
    public class ReferenceConverterStub : BuilderStub, IReferenceConverter
    {
        private readonly Resolver<IReferenceConverter> _resolver;

        public ReferenceConverterStub(string stringRepresentation, Resolver<IReferenceConverter> resolver)
            : base(stringRepresentation)
        {
            _resolver = resolver;
        }

        public IDamageTypeBuilder AsDamageType =>
            new DamageTypeBuilderStub($"{this}.AsDamageType", (_, context) => Resolve(context).AsDamageType);

        public IChargeTypeBuilder AsChargeType =>
            new ChargeTypeBuilderStub($"{this}.AsChargeType", (_, context) => Resolve(context).AsChargeType);

        public IAilmentBuilder AsAilment =>
            new AilmentBuilderStub($"{this}.AsAilment", (_, context) => Resolve(context).AsAilment);

        public IKeywordBuilder AsKeyword =>
            new KeywordBuilderStub($"{this}.AsKeyword", (_, context) => Resolve(context).AsKeyword);

        public IItemSlotBuilder AsItemSlot =>
            new ItemSlotBuilderStub($"{this}.AsItemSlot", (_, context) => Resolve(context).AsItemSlot);

        public ISelfToAnyActionBuilder AsAction =>
            new SelfToAnyActionBuilderStub($"{this}.AsAction", (_, context) => Resolve(context).AsAction);

        public IStatBuilder AsStat =>
            new StatBuilderStub($"{this}.AsStat", (_, context) => Resolve(context).AsStat);

        public IFlagStatBuilder AsFlagStat =>
            new FlagStatBuilderStub($"{this}.AsFlagStat", (_, context) => Resolve(context).AsFlagStat);

        public IPoolStatBuilder AsPoolStat =>
            new PoolStatBuilderStub($"{this}.AsPoolStat", (_, context) => Resolve(context).AsPoolStat);

        public IDamageStatBuilder AsDamageStat =>
            new DamageStatBuilderStub($"{this}.AsDamageStat", (_, context) => Resolve(context).AsDamageStat);

        public ISkillBuilder AsSkill =>
            new SkillBuilderStub($"{this}.AsSkill", (_, context) => Resolve(context).AsSkill);

        private IReferenceConverter Resolve(ResolveContext context) =>
            _resolver(this, context);
    }
}