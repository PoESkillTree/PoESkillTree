using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Parsing;

namespace PoESkillTree.Computation.Builders.Stats
{
    internal class DamageStatConcretizer : IResolvable<DamageStatConcretizer>
    {
        private readonly IStatFactory _statFactory;
        private readonly DamageSpecificationBuilder _specificationBuilder;

        // Null for never apply (don't ever set to true)
        private readonly bool? _applyToSkillDamage;
        private readonly bool? _applyToAilmentDamage;

        public DamageStatConcretizer(IStatFactory statFactory, DamageSpecificationBuilder specificationBuilder,
            bool canApplyToSkillDamage = false, bool canApplyToAilmentDamage = false)
            : this(statFactory, specificationBuilder,
                CanApplyToDefault(canApplyToSkillDamage), CanApplyToDefault(canApplyToAilmentDamage))
        {
        }

        private static bool? CanApplyToDefault(bool canApply) => canApply ? false : (bool?) null;

        private DamageStatConcretizer(
            IStatFactory statFactory, DamageSpecificationBuilder specificationBuilder,
            bool? applyToSkillDamage, bool? applyToAilmentDamage)
        {
            _statFactory = statFactory;
            _specificationBuilder = specificationBuilder;
            _applyToSkillDamage = applyToSkillDamage;
            _applyToAilmentDamage = applyToAilmentDamage;
        }

        private DamageStatConcretizer With(DamageSpecificationBuilder specificationBuilder) =>
            new DamageStatConcretizer(_statFactory, specificationBuilder, null, null);

        private DamageStatConcretizer WithCanApply(DamageSpecificationBuilder specificationBuilder,
            bool applyToSkillDamage = false, bool applyToAilmentDamage = false)
        {
            return new DamageStatConcretizer(_statFactory, specificationBuilder,
                CanApplyToSkillDamage && applyToSkillDamage ? true : _applyToSkillDamage,
                CanApplyToAilmentDamage && applyToAilmentDamage ? true : _applyToAilmentDamage);
        }

        public bool CanApplyToSkillDamage => _applyToSkillDamage.HasValue;
        public bool CanApplyToAilmentDamage => _applyToAilmentDamage.HasValue;

        public DamageStatConcretizer With(DamageSource source)
        {
            var concretizer = WithCanApply(_specificationBuilder.With(source), applyToSkillDamage: true);
            return source == DamageSource.OverTime ? concretizer : concretizer.WithSkills();
        }

        public DamageStatConcretizer WithHits() => With(_specificationBuilder.WithHits());

        public DamageStatConcretizer WithHitsAndAilments() => With(_specificationBuilder.WithHitsAndAilments());

        public DamageStatConcretizer WithAilments() => With(_specificationBuilder.WithAilments());

        public DamageStatConcretizer With(IAilmentBuilder ailment) => With(_specificationBuilder.With(ailment));

        public DamageStatConcretizer WithSkills() =>
            WithCanApply(_specificationBuilder.WithSkills(), applyToAilmentDamage: true);

        public DamageStatConcretizer With(AttackDamageHand hand) => WithCanApply(_specificationBuilder.With(hand));

        public DamageStatConcretizer NotDamageRelated() => With(_specificationBuilder);

        public DamageStatConcretizer Resolve(ResolveContext context) => With(_specificationBuilder.Resolve(context));

        public IEnumerable<StatBuilderResult> Concretize(Form modifierForm, StatBuilderResult result)
        {
            var results = new List<StatBuilderResult>();
            var sourceStats = new List<IStat>();
            var sourceDamageSources = new HashSet<DamageSource>();
            foreach (var spec in _specificationBuilder.Build())
            {
                sourceDamageSources.Add(spec.DamageSource);
                var stats = ConcretizeStats(spec, result.Stats);
                sourceStats.AddRange(stats);
                results.Add(new StatBuilderResult(stats, result.ModifierSource, result.ValueConverter));
            }
            if (_applyToSkillDamage.GetValueOrDefault(false))
            {
                results.AddRange(ApplyToSkillDamage(modifierForm, result, sourceStats, sourceDamageSources));
            }
            if (_applyToAilmentDamage.GetValueOrDefault(false))
            {
                results.AddRange(ApplyToAilmentDamage(modifierForm, result, sourceStats));
            }
            return results;
        }

        private IEnumerable<StatBuilderResult> ApplyToSkillDamage(Form modifierForm, StatBuilderResult result,
            IReadOnlyList<IStat> sourceStats, IEnumerable<DamageSource> sourceDamageSources)
        {
            var specs = Enums.GetValues<DamageSource>()
                .Except(sourceDamageSources)
                .Select(source => new DamageSpecificationBuilder().WithSkills().With(source))
                .SelectMany(specBuilder => specBuilder.Build());
            foreach (var spec in specs)
            {
                var stats = ConcretizeStats(spec, result.Stats);
                var applyStats = sourceStats.Select(
                    s => _statFactory.ApplyModifiersToSkillDamage(s, spec.DamageSource, modifierForm));
                var valueConverter = result.ValueConverter.AndThen(ApplyToDamageValueConverter(applyStats));
                yield return new StatBuilderResult(stats, result.ModifierSource, valueConverter);
            }
        }

        private IEnumerable<StatBuilderResult> ApplyToAilmentDamage(Form modifierForm, StatBuilderResult result,
            IEnumerable<IStat> sourceStats)
        {
            var specBuilder = new DamageSpecificationBuilder().WithAilments();
            var applyStats = sourceStats.Select(
                s => _statFactory.ApplyModifiersToAilmentDamage(s, modifierForm));
            var valueConverter = result.ValueConverter.AndThen(ApplyToDamageValueConverter(applyStats));
            foreach (var spec in specBuilder.Build())
            {
                var stats = ConcretizeStats(spec, result.Stats);
                yield return new StatBuilderResult(stats, result.ModifierSource, valueConverter);
            }
        }

        private static ValueConverter ApplyToDamageValueConverter(IEnumerable<IStat> applyStats)
        {
            var values = applyStats
                .Select(s => new FunctionalValue(c => c.GetValue(s) / 100, $"{s}.Value / 100"))
                .ToList();
            var multiplier = new FunctionalValue(
                c => values.Select(v => v.Calculate(c)).AggregateOnValues(Combine),
                $"RequireEqualWhereNotNull({string.Join(",", values)})");
            return v => v.Multiply(new ValueBuilderImpl(multiplier)).If(multiplier);
            
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
    }
}