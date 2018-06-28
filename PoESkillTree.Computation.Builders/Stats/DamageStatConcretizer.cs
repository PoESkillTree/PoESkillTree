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
        private readonly bool _applyToSkillDamage;
        private readonly bool _applyToAilmentDamage;

        public DamageStatConcretizer(IStatFactory statFactory, DamageSpecificationBuilder specificationBuilder)
            : this(statFactory, specificationBuilder, false, false)
        {
        }

        private DamageStatConcretizer(
            IStatFactory statFactory, DamageSpecificationBuilder specificationBuilder,
            bool applyToSkillDamage, bool applyToAilmentDamage)
        {
            _statFactory = statFactory;
            _specificationBuilder = specificationBuilder;
            _applyToSkillDamage = applyToSkillDamage;
            _applyToAilmentDamage = applyToAilmentDamage;
        }

        private DamageStatConcretizer With(DamageSpecificationBuilder specificationBuilder) =>
            new DamageStatConcretizer(_statFactory, specificationBuilder, _applyToSkillDamage, _applyToAilmentDamage);

        public DamageStatConcretizer With(DamageSource source)
        {
            var concretizer = new DamageStatConcretizer(_statFactory, _specificationBuilder.With(source), true,
                _applyToAilmentDamage);
            return source == DamageSource.OverTime ? concretizer : concretizer.WithSkills();
        }

        public DamageStatConcretizer WithHits() => With(_specificationBuilder.WithHits());

        public DamageStatConcretizer WithHitsAndAilments() => With(_specificationBuilder.WithHitsAndAilments());

        public DamageStatConcretizer WithAilments() => With(_specificationBuilder.WithAilments());

        public DamageStatConcretizer With(IAilmentBuilder ailment) => With(_specificationBuilder.With(ailment));

        public DamageStatConcretizer WithSkills() =>
            new DamageStatConcretizer(_statFactory, _specificationBuilder.WithSkills(), _applyToSkillDamage, true);

        public DamageStatConcretizer With(AttackDamageHand hand) => With(_specificationBuilder.With(hand));

        public DamageStatConcretizer NotDamageRelated() =>
            new DamageStatConcretizer(_statFactory, _specificationBuilder);

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
            if (_applyToSkillDamage)
            {
                results.AddRange(ApplyToSkillDamage(modifierForm, result, sourceStats, sourceDamageSources));
            }
            if (_applyToAilmentDamage)
            {
                results.AddRange(ApplyToAilmentDamage(modifierForm, result, sourceStats));
            }
            return results;
        }

        private IEnumerable<StatBuilderResult> ApplyToSkillDamage(Form modifierForm, StatBuilderResult result,
            IReadOnlyList<IStat> sourceStats, IEnumerable<DamageSource> sourceDamageSources)
        {
            var specBuilder = new DamageSpecificationBuilder().WithSkills()
                .With(Enums.GetValues<DamageSource>().Except(sourceDamageSources).ToArray());
            foreach (var spec in specBuilder.Build())
            {
                var stats = ConcretizeStats(spec, result.Stats);
                var applyStats = sourceStats.Select(
                    s => _statFactory.ApplyModifiersToSkillDamage(s, spec.DamageSource, modifierForm));
                var valueConverter = ApplyToDamageValueConverter(applyStats);
                yield return new StatBuilderResult(stats, result.ModifierSource,
                    v => valueConverter(result.ValueConverter(v)));
            }
        }

        private IEnumerable<StatBuilderResult> ApplyToAilmentDamage(Form modifierForm, StatBuilderResult result,
            IEnumerable<IStat> sourceStats)
        {
            var specBuilder = new DamageSpecificationBuilder().WithAilments();
            var applyStats = sourceStats.Select(
                s => _statFactory.ApplyModifiersToAilmentDamage(s, modifierForm));
            var valueConverter = ApplyToDamageValueConverter(applyStats);
            foreach (var spec in specBuilder.Build())
            {
                var stats = ConcretizeStats(spec, result.Stats);
                yield return new StatBuilderResult(stats, result.ModifierSource,
                    v => valueConverter(result.ValueConverter(v)));
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