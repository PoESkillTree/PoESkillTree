using System.Collections.Generic;
using PoESkillTree.Computation.Builders;
using PoESkillTree.Computation.Builders.Actions;
using PoESkillTree.Computation.Builders.Buffs;
using PoESkillTree.Computation.Builders.Charges;
using PoESkillTree.Computation.Builders.Damage;
using PoESkillTree.Computation.Builders.Effects;
using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Builders.Resolving;
using PoESkillTree.Computation.Builders.Skills;
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

        public IChargeTypeBuilder AsChargeType
        {
            get
            {
                var core = new UnresolvedCoreBuilder<ChargeType>($"{this}.AsChargeType", 
                    context => CoreBuilder.Proxy(Resolve(context).AsChargeType, b => b.Build()));
                return new ChargeTypeBuilder(_statFactory, core);
            }
        }

        public IAilmentBuilder AsAilment
        {
            get
            {
                var core = new UnresolvedCoreBuilder<Ailment>($"{this}.AsAilment",
                    context => CoreBuilder.Proxy<IAilmentBuilder, IEffectBuilder, Ailment>(
                        Resolve(context).AsAilment, b => b.Build()));
                return new AilmentBuilder(_statFactory, core);
            }
        }

        public IKeywordBuilder AsKeyword =>
            new UnresolvedKeywordBuilder($"{this}.AsKeyword", context => Resolve(context).AsKeyword);

        public IItemSlotBuilder AsItemSlot =>
            new UnresolvedItemSlotBuilder($"{this}.AsItemSlot", context => Resolve(context).AsItemSlot);

        public IActionBuilder AsAction
        {
            get
            {
                var core = new UnresolvedCoreBuilder<string>($"{this}.AsAction", 
                    context => CoreBuilder.Proxy(Resolve(context).AsAction, b => b.Build()));
                return new ActionBuilder(_statFactory, core, new ModifierSourceEntityBuilder());
            }
        }

        public IStatBuilder AsStat
        {
            get
            {
                var core = new UnresolvedCoreStatBuilder($"{this}.AsStat",
                    context => new StatBuilderAdapter(Resolve(context).AsStat));
                return new StatBuilder(_statFactory, core);
            }
        }

        public IPoolStatBuilder AsPoolStat =>
            new PoolStatBuilderStub($"{this}.AsPoolStat", (_, context) => Resolve(context).AsPoolStat);

        public IDamageStatBuilder AsDamageStat =>
            new DamageStatBuilderStub($"{this}.AsDamageStat", (_, context) => Resolve(context).AsDamageStat);

        public IBuffBuilder AsBuff
        {
            get
            {
                var core = new UnresolvedCoreBuilder<string>($"{this}.AsBuff", 
                    context => CoreBuilder.Proxy((IEffectBuilder) Resolve(context).AsBuff, b => b.Build()));
                return new BuffBuilder(_statFactory, core);
            }
        }

        public ISkillBuilder AsSkill
        {
            get
            {
                var core = new UnresolvedCoreBuilder<SkillDefinition>($"{this}.AsSkill",
                    context => CoreBuilder.Proxy(Resolve(context).AsSkill, b => b.Build()));
                return new SkillBuilder(_statFactory, core);
            }
        }

        private IReferenceConverter Resolve(ResolveContext context) =>
            _resolver(this, context);
    }
}