using System;
using System.Collections.Generic;
using System.Linq;
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

        public IStatBuilder NotAsBuffOn(IEntityBuilder target) =>
            InternalOn(target);

        public IStatBuilder Effect => EffectOn(EntityBuilder.AllEntities);

        public IStatBuilder EffectOn(IEntityBuilder target)
        {
            return new StatBuilder(StatFactory, new CoreStatBuilderFromCoreBuilder<string>(Identity,
                (ps, s, i) => BuildStats(ps, s, i)));

            IEnumerable<IStat> BuildStats(BuildParameters parameters, Entity source, string identity)
                => target.Build(parameters.ModifierSourceEntity)
                    .Select(t => BuildEffectStat(source, t, identity));
        }

        public IActionBuilder Action =>
            new ActionBuilder(StatFactory, Identity, new ModifierSourceEntityBuilder());

        public override IStatBuilder On(IEntityBuilder target) =>
            base.On(target)
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
            return new StatBuilder(StatFactory, core).For(target).IsSet;
        }

        public override IStatBuilder AddStat(IStatBuilder stat) => AddStatForSource(stat, EntityBuilder.AllEntities);

        public IStatBuilder AddStatForSource(IStatBuilder stat, IEntityBuilder source)
        {
            var baseCoreBuilder = new StatBuilderAdapter(base.AddStat(stat));
            var coreBuilder = new StatBuilderWithValueConverter(baseCoreBuilder,
                (ps, target) => BuildAddStatMultiplier(source.Build(ps.ModifierSourceEntity), target),
                (l, r) => l.Multiply(r));
            return new StatBuilder(StatFactory, coreBuilder);
        }

        private IValue BuildAddStatMultiplier(IReadOnlyCollection<Entity> possibleSources, Entity target)
        {
            var identity = Build();
            var buffActiveValue = new StatValue(BuildBuffActiveStat(target, identity));
            var buffSourceValues = possibleSources.ToDictionary(Funcs.Identity,
                e => new StatValue(BuildBuffSourceStat(e, target, identity)));
            var buffEffectValues = possibleSources.ToDictionary(Funcs.Identity,
                e => new StatValue(BuildEffectStat(e, target, identity)));

            return new FunctionalValue(Calculate,
                $"AddStatMultiplier(buffActive:{buffActiveValue}, buffSources:{string.Join(",", buffSourceValues)}, " +
                $"buffEffects:{string.Join(",", buffEffectValues)})");

            NodeValue? Calculate(IValueCalculationContext context)
            {
                if (!buffActiveValue.Calculate(context).IsTrue())
                    return new NodeValue(1);

                // If multiple entities apply the same (de-)buff, it depends on the buff which one would actually apply.
                // Because that shouldn't happen in these calculations, simply the first one is taken.
                var sourcEntity = possibleSources.First(e => buffSourceValues[e].Calculate(context).IsTrue());
                return buffEffectValues[sourcEntity].Calculate(context);
            }
        }

        private ICoreStatBuilder FromStatFactory(Func<Entity, string, IStat> statFactory) =>
            new CoreStatBuilderFromCoreBuilder<string>(Identity, statFactory);

        private ICoreStatBuilder FromStatFactory(Func<BuildParameters, Entity, string, IStat> statFactory) =>
            new CoreStatBuilderFromCoreBuilder<string>(Identity, statFactory);

        private IStat BuildEffectStat(Entity source, Entity target, string identity) =>
            StatFactory.BuffEffect(source, target, identity);

        private IStat BuildBuffActiveStat(Entity target, string identity) =>
            StatFactory.BuffIsActive(target, identity);

        private IStat BuildBuffSourceStat(Entity source, Entity target, string identity) =>
            StatFactory.BuffSourceIs(source, target, identity);
    }
}