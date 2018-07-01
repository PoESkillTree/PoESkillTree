using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Parsing;

namespace PoESkillTree.Computation.Builders.Stats
{
    public class ConversionStatBuilder : ICoreStatBuilder
    {
        public delegate IEnumerable<IStat> ConversionStatFactory(IStat sourceStat, IEnumerable<IStat> targetStats);
        
        private readonly ConversionStatFactory _statFactory;
        private readonly ICoreStatBuilder _source;
        private readonly ICoreStatBuilder _target;

        public ConversionStatBuilder(
            ConversionStatFactory statFactory, ICoreStatBuilder source, ICoreStatBuilder target)
        {
            _statFactory = statFactory;
            _source = source;
            _target = target;
        }

        private ICoreStatBuilder Select(Func<ICoreStatBuilder, ICoreStatBuilder> selector) =>
            new ConversionStatBuilder(_statFactory, selector(_source), selector(_target));

        public ICoreStatBuilder Resolve(ResolveContext context) =>
            Select(b => b.Resolve(context));

        public ICoreStatBuilder WithEntity(IEntityBuilder entityBuilder) =>
            Select(b => b.WithEntity(entityBuilder));

        public ICoreStatBuilder WithStatConverter(Func<IStat, IStat> statConverter) =>
            new ConversionStatBuilder((s, ts) => _statFactory(s, ts).Select(statConverter), _source, _target);

        public IEnumerable<StatBuilderResult> Build(BuildParameters parameters, ModifierSource originalModifierSource)
        {
            var sourceResults = _source.Build(parameters, originalModifierSource).ToList();
            var targetResults = _target.Build(parameters, originalModifierSource).ToList();
            if (sourceResults.Count != targetResults.Count)
                throw new ParseException("Source and target stats of conversion must build to same amount of mods");
            return sourceResults.Zip(targetResults, (s, t) => Build(originalModifierSource, s, t));
        }

        private StatBuilderResult Build(
            ModifierSource originalModifierSource, StatBuilderResult source, StatBuilderResult target)
        {
            VerifyStats(source.Stats, target.Stats);
            VerifyModifierSource(originalModifierSource, source.ModifierSource, target.ModifierSource);

            var stats = BuildStats(source.Stats, target.Stats);
            var valueConverter = BuildValueConverter(source.ValueConverter, target.ValueConverter);
            return new StatBuilderResult(stats.ToList(), originalModifierSource, valueConverter);
        }

        private static void VerifyStats(IReadOnlyList<IStat> sourceStats, IReadOnlyList<IStat> targetStats)
        {
            var sourceEntities = sourceStats.Select(s => s.Entity).ToHashSet();
            var targetEntities = targetStats.Select(s => s.Entity).ToHashSet();
            if (!sourceEntities.SetEquals(targetEntities))
                throw new ParseException("Source and target stats of conversions must belong to the same entity");
        }

        private IEnumerable<IStat> BuildStats(IReadOnlyList<IStat> sourceStats, IReadOnlyList<IStat> targetStats)
        {
            var targetStatsByEntity = targetStats.ToLookup(s => s.Entity);
            return sourceStats.SelectMany(s => _statFactory(s, targetStatsByEntity[s.Entity]));
        }

        private static void VerifyModifierSource(
            ModifierSource originalSource, ModifierSource sourceSource, ModifierSource targetSource)
        {
            if (originalSource != sourceSource)
                throw new ParseException("The source stat of conversions must not be ModifierSource specific");
            if (originalSource != targetSource)
                throw new ParseException("The target stat of conversions must not be ModifierSource specific");
        }

        private static ValueConverter BuildValueConverter(
            ValueConverter sourceConverter, ValueConverter targetConverter) =>
            v => targetConverter(sourceConverter(v));
    }
}