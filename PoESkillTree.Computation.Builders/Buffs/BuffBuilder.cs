using System;
using System.Linq;
using EnumsNET;
using PoESkillTree.Common.Utils;
using PoESkillTree.Computation.Builders.Actions;
using PoESkillTree.Computation.Builders.Effects;
using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Buffs;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Buffs
{
    public class BuffBuilder : EffectBuilder, IBuffBuilder
    {
        public BuffBuilder(IStatFactory statFactory, ICoreBuilder<string> identity) : base(statFactory, identity)
        {
        }

        public override IEffectBuilder Resolve(ResolveContext context) =>
            new BuffBuilder(StatFactory, Identity.Resolve(context));

        public IFlagStatBuilder NotAsBuffOn(IEntityBuilder target) =>
            InternalOn(target);

        public IStatBuilder Effect =>
            new StatBuilder(StatFactory, FromStatFactory(BuildEffectStat));

        public IActionBuilder Action =>
            new ActionBuilder(StatFactory, Identity, new ModifierSourceEntityBuilder());

        public override IFlagStatBuilder On(IEntityBuilder target) =>
            (IFlagStatBuilder) base.On(target)
                .CombineWith(new StatBuilder(StatFactory, FromStatFactory(BuildBuffActiveStat)))
                .CombineWith(new StatBuilder(StatFactory, FromStatFactory(BuildBuffSourceStat)))
                .For(target);

        private IStat BuildBuffSourceStat(BuildParameters parameters, Entity entity, string identity) =>
            BuildBuffSourceStat(parameters.ModifierSourceEntity, entity, identity);

        public IConditionBuilder IsOn(IEntityBuilder source, IEntityBuilder target) =>
            IsOn(target).And(IsFromSource(source, target));

        private IConditionBuilder IsFromSource(IEntityBuilder source, IEntityBuilder target)
        {
            var core = FromStatFactory((e, id) => StatFactory.FromIdentity(id, e, typeof(double)));
            core = new ParametrisedCoreStatBuilder<IEntityBuilder>(core, source,
                (eb, s) => eb.Build(s.Entity).Select(e => BuildBuffSourceStat(e, s.Entity, s.Identity)));
            return ((IFlagStatBuilder) new StatBuilder(StatFactory, core).For(target)).IsSet;
        }

        public override IStatBuilder AddStat(IStatBuilder stat)
        {
            var baseCoreBuilder = new StatBuilderAdapter(base.AddStat(stat));
            var coreBuilder = new StatBuilderWithValueConverter(baseCoreBuilder,
                BuildAddStatMultiplier,
                (l, r) => l.Multiply(r));
            return new StatBuilder(StatFactory, coreBuilder);
        }

        private IValue BuildAddStatMultiplier(Entity entity)
        {
            var identity = Build();
            var allEntites = Enums.GetValues<Entity>().ToList();
            var buffActiveValue = new StatValue(BuildBuffActiveStat(entity, identity));
            var buffSourceValues = allEntites.ToDictionary(Funcs.Identity,
                e => new StatValue(BuildBuffSourceStat(e, entity, identity)));
            var buffEffectValues = allEntites.ToDictionary(Funcs.Identity,
                e => new StatValue(BuildEffectStat(e, identity)));

            return new FunctionalValue(Calculate,
                $"AddStatMultiplier(buffActive:{buffActiveValue}, buffSources:{string.Join(",", buffSourceValues)}, " +
                $"buffEffects:{string.Join(",", buffEffectValues)})");

            NodeValue? Calculate(IValueCalculationContext context)
            {
                if (!buffActiveValue.Calculate(context).IsTrue())
                    return new NodeValue(1);

                // If multiple entities apply the same (de-)buff, it depends on the buff which one would actually apply.
                // Because that shouldn't happen in these calculations, simply the first one is taken.
                var sourcEntity = allEntites.First(e => buffSourceValues[e].Calculate(context).IsTrue());
                return buffEffectValues[sourcEntity].Calculate(context);
            }
        }

        private ICoreStatBuilder FromStatFactory(Func<Entity, string, IStat> statFactory) =>
            new CoreStatBuilderFromCoreBuilder<string>(Identity, statFactory);

        private ICoreStatBuilder FromStatFactory(Func<BuildParameters, Entity, string, IStat> statFactory) =>
            new CoreStatBuilderFromCoreBuilder<string>(Identity, statFactory);

        private IStat BuildEffectStat(Entity entity, string identity) =>
            StatFactory.BuffEffect(entity, identity);

        private IStat BuildBuffActiveStat(Entity entity, string identity) =>
            StatFactory.BuffIsActive(entity, identity);

        private IStat BuildBuffSourceStat(Entity source, Entity entity, string identity) =>
            StatFactory.BuffSourceIs(entity, identity, source);
    }
}