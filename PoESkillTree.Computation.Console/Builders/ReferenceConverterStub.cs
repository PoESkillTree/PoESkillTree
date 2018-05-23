using PoESkillTree.Computation.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Charges;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Equipment;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;

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

        /*
         * When the objects returned by these properties are resolved, the new objects are retrieved from the context.
         */

        public IDamageTypeBuilder AsDamageType =>
            new DamageTypeBuilderStub($"{this}.AsDamageType", (_, context) => Resolve(context).AsDamageType);

        public IChargeTypeBuilder AsChargeType =>
            new ChargeTypeBuilderStub($"{this}.AsChargeType", (_, context) => Resolve(context).AsChargeType);

        public IAilmentBuilder AsAilment =>
            new AilmentBuilderStub($"{this}.AsAilment", (_, context) => Resolve(context).AsAilment);

        public IKeywordBuilder AsKeyword =>
            new KeywordBuilderStub($"{this}.AsKeyword", (_, context) => Resolve(context).AsKeyword);

        public IItemSlotBuilder AsItemSlot =>
            new UnresolvedItemSlotBuilder($"{this}.AsItemSlot", context => Resolve(context).AsItemSlot);

        public IActionBuilder AsAction =>
            ActionBuilderStub.SelfToAny($"{this}.AsAction", (_, context) => Resolve(context).AsAction);

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