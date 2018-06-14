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
        public enum Mode
        {
            ConvertTo,
            GainAs
        }

        private readonly ICoreStatBuilder _source;
        private readonly ICoreStatBuilder _target;
        private readonly Mode _mode;

        public ConversionStatBuilder(ICoreStatBuilder source, ICoreStatBuilder target, Mode mode = Mode.ConvertTo)
        {
            _source = source;
            _target = target;
            _mode = mode;
        }

        private ICoreStatBuilder Select(Func<ICoreStatBuilder, ICoreStatBuilder> selector) =>
            new ConversionStatBuilder(selector(_source), selector(_target), _mode);

        public ICoreStatBuilder Resolve(ResolveContext context) =>
            Select(b => b.Resolve(context));

        public ICoreStatBuilder WithEntity(IEntityBuilder entityBuilder) =>
            Select(b => b.WithEntity(entityBuilder));

        public ICoreStatBuilder WithStatConverter(Func<IStat, IStat> statConverter) =>
            Select(b => b.WithStatConverter(statConverter));

        public IValue BuildValue(Entity modifierSourceEntity) =>
            throw new ParseException("Can't access the value of conversion stats directly (yet)");

        public IEnumerable<StatBuilderResult> Build(ModifierSource originalModifierSource, Entity modifierSourceEntity)
        {
            var sourceResults = _source.Build(originalModifierSource, modifierSourceEntity).ToList();
            var targetResults = _target.Build(originalModifierSource, modifierSourceEntity).ToList();
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
            foreach (var sourceStat in sourceStats)
            {
                // TODO behaviors (pass something like IBehaviorFactory?)
                foreach (var targetStat in targetStatsByEntity[sourceStat.Entity])
                {
                    yield return Stat.CopyWithSuffix(sourceStat, $"{_mode}({targetStat})", dataType: typeof(int));
                }
                if (_mode == Mode.ConvertTo)
                {
                    yield return Stat.CopyWithSuffix(sourceStat, "Conversion", dataType: typeof(int));
                    yield return Stat.CopyWithSuffix(sourceStat, "SkillConversion", dataType: typeof(int));
                }
            }
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