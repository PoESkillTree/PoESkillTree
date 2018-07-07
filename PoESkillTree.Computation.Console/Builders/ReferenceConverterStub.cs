using System.Collections.Generic;
using PoESkillTree.Computation.Builders.Damage;
using PoESkillTree.Computation.Builders.Resolving;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Buffs;
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
        private readonly IStatFactory _statFactory = new StatFactory();
        private readonly Resolver<IReferenceConverter> _resolver;

        public ReferenceConverterStub(string stringRepresentation, Resolver<IReferenceConverter> resolver)
            : base(stringRepresentation)
        {
            _resolver = resolver;
        }

        /*
         * When the objects returned by these properties are resolved, the new objects are retrieved from the context.
         */

        public IDamageTypeBuilder AsDamageType
        {
            get
            {
                var core = new UnresolvedCoreBuilder<IEnumerable<DamageType>>($"{this}.AsDamageType", 
                    context => new ProxyDamageTypeBuilder(Resolve(context).AsDamageType));
                return new DamageTypeBuilder(_statFactory, core);
            }
        }

        public IChargeTypeBuilder AsChargeType =>
            new ChargeTypeBuilderStub($"{this}.AsChargeType", (_, context) => Resolve(context).AsChargeType);

        public IAilmentBuilder AsAilment =>
            new AilmentBuilderStub($"{this}.AsAilment", (_, context) => Resolve(context).AsAilment);

        public IKeywordBuilder AsKeyword =>
            new UnresolvedKeywordBuilder($"{this}.AsKeyword", context => Resolve(context).AsKeyword);

        public IItemSlotBuilder AsItemSlot =>
            new UnresolvedItemSlotBuilder($"{this}.AsItemSlot", context => Resolve(context).AsItemSlot);

        public IActionBuilder AsAction =>
            ActionBuilderStub.BySelf($"{this}.AsAction", (_, context) => Resolve(context).AsAction);

        public IStatBuilder AsStat =>
            new StatBuilderStub($"{this}.AsStat", (_, context) => Resolve(context).AsStat);

        public IPoolStatBuilder AsPoolStat =>
            new PoolStatBuilderStub($"{this}.AsPoolStat", (_, context) => Resolve(context).AsPoolStat);

        public IDamageStatBuilder AsDamageStat =>
            new DamageStatBuilderStub($"{this}.AsDamageStat", (_, context) => Resolve(context).AsDamageStat);

        public IBuffBuilder AsBuff =>
            new BuffBuilderStub($"{this}.AsBuff", (_, context) => Resolve(context).AsBuff);

        public ISkillBuilder AsSkill =>
            new SkillBuilderStub($"{this}.AsSkill", (_, context) => Resolve(context).AsSkill);

        private IReferenceConverter Resolve(ResolveContext context) =>
            _resolver(this, context);
    }
}