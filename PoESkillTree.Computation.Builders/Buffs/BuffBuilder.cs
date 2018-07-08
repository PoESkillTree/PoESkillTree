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
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Buffs
{
    public class BuffBuilder : EffectBuilder, IBuffBuilder
    {
        public BuffBuilder(IStatFactory statFactory, string identity) : base(statFactory, identity)
        {
        }

        public IFlagStatBuilder NotAsBuffOn(IEntityBuilder target) =>
            InternalOn(target);

        public IStatBuilder Effect =>
            new StatBuilder(StatFactory, new LeafCoreStatBuilder(BuildEffectStat));

        public IActionBuilder Action =>
            new ActionBuilder(StatFactory, new ConstantCoreBuilder<string>(Identity),
                new ModifierSourceEntityBuilder());

        public override IFlagStatBuilder On(IEntityBuilder target) =>
            (IFlagStatBuilder) base.On(target)
                .CombineWith(new StatBuilder(StatFactory, new LeafCoreStatBuilder(BuildBuffActiveStat)))
                .CombineWith(new StatBuilder(StatFactory, new LeafCoreStatBuilder(BuildBuffSourceStat)))
                .For(target);

        private IStat BuildBuffSourceStat(BuildParameters parameters, Entity entity) =>
            BuildBuffSourceStat(parameters.ModifierSourceEntity, entity);

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
            var allEntites = Enums.GetValues<Entity>().ToList();
            var buffActiveValue = new StatValue(BuildBuffActiveStat(entity));
            var buffSourceValues = allEntites.ToDictionary(Funcs.Identity,
                e => new StatValue(BuildBuffSourceStat(e, entity)));
            var buffEffectValues = allEntites.ToDictionary(Funcs.Identity,
                e => new StatValue(BuildEffectStat(e)));

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

        private IStat BuildEffectStat(Entity entity) =>
            StatFactory.FromIdentity($"{Identity}.Effect", entity, typeof(int));

        private IStat BuildBuffActiveStat(Entity entity) =>
            StatFactory.FromIdentity($"{Identity}.BuffActive", entity, typeof(bool));

        private IStat BuildBuffSourceStat(Entity modifierSourceEntity, Entity entity) =>
            StatFactory.FromIdentity($"{Identity}.BuffSourceIs({modifierSourceEntity})", entity,
                typeof(bool));
    }
}