using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using PoESkillTree.Computation.Builders.Conditions;
using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Parsing;
using PoESkillTree.Utils;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Builders.Stats
{
    public class DamageStatConcretizer : IResolvable<DamageStatConcretizer>
    {
        private readonly IStatFactory _statFactory;
        private readonly DamageSpecificationBuilder _specificationBuilder;

        // Null for never apply (don't ever set to true)
        private readonly bool? _applyToSkillDamage;
        private readonly bool? _applyToAilmentDamage;

        private readonly Func<IDamageSpecification, IConditionBuilder> _condition;

        public DamageStatConcretizer(
            IStatFactory statFactory, DamageSpecificationBuilder specificationBuilder,
            bool canApplyToSkillDamage = false, bool canApplyToAilmentDamage = false)
            : this(statFactory, specificationBuilder,
                CanApplyToDefault(canApplyToSkillDamage), CanApplyToDefault(canApplyToAilmentDamage),
                _ => new NullConditionBuilder())
        {
        }

        private static bool? CanApplyToDefault(bool canApply) => canApply ? false : (bool?) null;

        private DamageStatConcretizer(
            IStatFactory statFactory, DamageSpecificationBuilder specificationBuilder,
            bool? applyToSkillDamage, bool? applyToAilmentDamage,
            Func<IDamageSpecification, IConditionBuilder> condition)
        {
            _statFactory = statFactory;
            _specificationBuilder = specificationBuilder;
            _applyToSkillDamage = applyToSkillDamage;
            _applyToAilmentDamage = applyToAilmentDamage;
            _condition = condition;
        }

        private DamageStatConcretizer With(DamageSpecificationBuilder specificationBuilder) =>
            new DamageStatConcretizer(_statFactory, specificationBuilder, null, null, _condition);

        private DamageStatConcretizer WithCanApply(DamageSpecificationBuilder specificationBuilder,
            bool applyToSkillDamage = false, bool applyToAilmentDamage = false,
            Func<IDamageSpecification, IConditionBuilder> condition = null)
        {
            return new DamageStatConcretizer(_statFactory, specificationBuilder,
                CanApplyToSkillDamage && applyToSkillDamage ? true : _applyToSkillDamage,
                CanApplyToAilmentDamage && applyToAilmentDamage ? true : _applyToAilmentDamage,
                condition ?? _condition);
        }

        public bool CanApplyToSkillDamage => _applyToSkillDamage.HasValue;
        public bool CanApplyToAilmentDamage => _applyToAilmentDamage.HasValue;

        public DamageStatConcretizer With(DamageSource source)
            => WithCanApply(_specificationBuilder.With(source), applyToSkillDamage: true);

        public DamageStatConcretizer WithHits() => With(_specificationBuilder.WithHits());

        public DamageStatConcretizer WithHitsAndAilments() => With(_specificationBuilder.WithHitsAndAilments());

        public DamageStatConcretizer WithAilments() => With(_specificationBuilder.WithAilments());

        public DamageStatConcretizer With(IAilmentBuilder ailment) => With(_specificationBuilder.With(ailment));

        public DamageStatConcretizer WithSkills() =>
            WithCanApply(_specificationBuilder.WithSkills(), applyToAilmentDamage: true);

        public DamageStatConcretizer With(AttackDamageHand hand) => With(_specificationBuilder.With(hand));

        public DamageStatConcretizer NotDamageRelated() => With(_specificationBuilder);

        public DamageStatConcretizer With(Func<IDamageSpecification, IConditionBuilder> condition) =>
            WithCanApply(_specificationBuilder, condition: condition);

        public DamageStatConcretizer Resolve(ResolveContext context) =>
            WithCanApply(_specificationBuilder.Resolve(context),
                condition: _condition.AndThen(c => c.Resolve(context)));

        public IEnumerable<StatBuilderResult> Concretize(BuildParameters parameters, StatBuilderResult result)
        {
            var applyToSkillDamage = _applyToSkillDamage.GetValueOrDefault(false);
            var applyToAilmentDamage = _applyToAilmentDamage.GetValueOrDefault(false);
            var applyToAny = applyToSkillDamage || applyToAilmentDamage;

            var results = new List<StatBuilderResult>();
            var sourceStats = new List<IStat>();
            // This method is the hot path of building modifiers from builders. Using HashSet instead of this array
            // would be much slower (HashSet.Add() would be 60% of this method's execution time, array is negligible).
            var sourceSkillDamageSources = new bool[Enums.GetMemberCount<DamageSource>()];
            foreach (var spec in _specificationBuilder.Build())
            {
                var stats = ConcretizeStats(spec, result.Stats);
                var valueConverter = ValueConverterForResult(parameters, result, spec);
                results.Add(new StatBuilderResult(stats, result.ModifierSource, valueConverter));

                if (applyToSkillDamage && spec.IsSkillDamage())
                {
                    sourceSkillDamageSources[(int) spec.DamageSource] = true;
                }
                if (applyToAny)
                {
                    sourceStats.AddRange(stats);
                }
            }

            if (applyToSkillDamage)
            {
                results.AddRange(ApplyToSkillDamage(parameters, result, sourceStats, sourceSkillDamageSources));
            }
            if (applyToAilmentDamage)
            {
                results.AddRange(ApplyToAilmentDamage(parameters, result, sourceStats));
            }
            return results;
        }

        private IEnumerable<StatBuilderResult> ApplyToSkillDamage(
            BuildParameters parameters, StatBuilderResult result,
            IReadOnlyList<IStat> sourceStats, IReadOnlyList<bool> sourceSkillDamageSources)
        {
            var specs = Enums.GetValues<DamageSource>()
                .Where(s => !sourceSkillDamageSources[(int) s])
                .Select(source => new DamageSpecificationBuilder().WithSkills().With(source))
                .SelectMany(specBuilder => specBuilder.Build());
            foreach (var spec in specs)
            {
                var stats = ConcretizeStats(spec, result.Stats);
                var applyStats = sourceStats.Select(
                    s => _statFactory.ApplyModifiersToSkillDamage(s, spec.DamageSource, parameters.ModifierForm));
                var valueConverter = ValueConverterForResult(parameters, result, spec)
                    .AndThen(ApplyToDamageValueConverter(applyStats));
                yield return new StatBuilderResult(stats, result.ModifierSource, valueConverter);
            }
        }

        private IEnumerable<StatBuilderResult> ApplyToAilmentDamage(
            BuildParameters parameters, StatBuilderResult result, IEnumerable<IStat> sourceStats)
        {
            var specBuilder = new DamageSpecificationBuilder().WithAilments();
            var applyStats = sourceStats.Select(
                s => _statFactory.ApplyModifiersToAilmentDamage(s, parameters.ModifierForm)).ToList();
            foreach (var spec in specBuilder.Build())
            {
                var stats = ConcretizeStats(spec, result.Stats);
                var valueConverter = ValueConverterForResult(parameters, result, spec)
                    .AndThen(ApplyToDamageValueConverter(applyStats));
                yield return new StatBuilderResult(stats, result.ModifierSource, valueConverter);
            }
        }

        private ValueConverter ValueConverterForResult(
            BuildParameters parameters, StatBuilderResult result, IDamageSpecification spec)
        {
            var condition = _condition(spec).Build(parameters);
            if (condition.HasStatConverter)
                throw new InvalidOperationException("Conditions passed to With must not have stat converters");
            if (!condition.HasValue)
                return result.ValueConverter;
            return result.ValueConverter.AndThen(v => v.If(condition.Value));
        }

        private static ValueConverter ApplyToDamageValueConverter(IEnumerable<IStat> applyStats)
        {
            var values = applyStats
                .Select(s => new FunctionalValue(c => c.GetValue(s) / 100, $"{s}.Value / 100"))
                .ToList();
            var multiplier = new FunctionalValue(
                c => values.Select(v => v.Calculate(c)).AggregateOnValues(Combine),
                $"RequireEqualWhereNotNull({string.Join(",", values)})");
            return v => v.Multiply(new ValueBuilderImpl(multiplier));
            
            // There isn't any obvious way to combine different values but it currently can't happen:
            // - More than one source damage source for ApplyModifiersToSkillDamage can't happen because
            //   With(DamageSource) only takes one argument.
            // - Different ApplyModifiersToAilmentDamage values for different source damage sources don't make sense.
            //   If e.g. crit multi for spells and attacks would be applied to ailments at different values, it would
            //   be ambiguous how to apply generic crit multi, which is split into the damage sources, to ailments.
            // - The current solution would not work if different stats of the original StatBuilderResult (built from
            //   the core builder) have different ApplyModifiersTo values. That is possible with IDamageStatBuilder and
            //   its damage types, but damage-type specific ApplyModifiersTo modifiers do not exist.
            NodeValue Combine(NodeValue left, NodeValue right)
            {
                if (left == right)
                    return left;
                throw new ParseException(
                    $"ApplyModifiersToDamage values must be equal for all concretized stats. {left} and {right} given");
            }
        }

        private IReadOnlyList<IStat> ConcretizeStats(IDamageSpecification spec, IEnumerable<IStat> resultStats) =>
            resultStats.Select(s => _statFactory.ConcretizeDamage(s, spec)).ToList();

        private class NullConditionBuilder : IConditionBuilder
        {
            public IConditionBuilder Resolve(ResolveContext context) => this;
            public IConditionBuilder And(IConditionBuilder condition) => condition;
            public IConditionBuilder Or(IConditionBuilder condition) => condition;
            public IConditionBuilder Not => this;
            public ConditionBuilderResult Build(BuildParameters parameters) => new ConditionBuilderResult();
        }
    }
}