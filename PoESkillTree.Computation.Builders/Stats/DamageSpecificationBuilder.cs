using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Parsing;

namespace PoESkillTree.Computation.Builders.Stats
{
    internal class DamageSpecificationBuilder : IResolvable<DamageSpecificationBuilder>
    {
        [Flags]
        private enum Mode
        {
            Hits = 1,
            SkillDoT = 2,
            Skills = Hits | SkillDoT,
            Ailments = 4
        }

        private readonly Mode _mode;
        private readonly IReadOnlyList<DamageSource> _damageSources;
        private readonly IReadOnlyList<AttackDamageHand> _hands;
        private readonly IReadOnlyList<IAilmentBuilder> _ailments;

        public DamageSpecificationBuilder()
            : this(Mode.Skills | Mode.Ailments, null, null, null)
        {
        }

        private DamageSpecificationBuilder(
            Mode mode, IReadOnlyList<DamageSource> damageSources, IReadOnlyList<IAilmentBuilder> ailments,
            IReadOnlyList<AttackDamageHand> hands)
        {
            _mode = mode;
            _damageSources = damageSources;
            _ailments = ailments;
            _hands = hands;
        }

        public DamageSpecificationBuilder Resolve(ResolveContext context)
        {
            return new DamageSpecificationBuilder(_mode, _damageSources,
                _ailments?.Select(b => b.Resolve(context)).Cast<IAilmentBuilder>().ToList(), _hands);
        }

        public DamageSpecificationBuilder With(params DamageSource[] damageSources)
        {
            if (_damageSources != null && _damageSources.Intersect(damageSources).Count() != damageSources.Length)
                throw new ParseException(
                    $"Damage sources were already restricted to {string.Join(", ", _damageSources)}");
            return new DamageSpecificationBuilder(_mode, damageSources, _ailments, _hands);
        }

        public DamageSpecificationBuilder With(AttackDamageHand hand)
        {
            if (_damageSources != null && !_damageSources.Contains(DamageSource.Attack))
                throw new ParseException(
                    $"Damage sources were already restricted to {string.Join(", ", _damageSources)}");
            if (_hands != null && !_hands.Contains(hand))
                throw new ParseException(
                    $"Hands were already restricted to {string.Join(", ", _hands)}");
            return new DamageSpecificationBuilder(_mode, new[] { DamageSource.Attack }, _ailments, new[] { hand });
        }

        public DamageSpecificationBuilder With(params IAilmentBuilder[] ailments)
        {
            if (_ailments != null)
                throw new ParseException($"Ailments were already restricted to {string.Join(", ", _ailments)}");
            return new DamageSpecificationBuilder(_mode.RemoveFlags(Mode.Skills), _damageSources, ailments, _hands);
        }

        public DamageSpecificationBuilder WithHits() =>
            new DamageSpecificationBuilder(_mode.RemoveFlags(Mode.SkillDoT | Mode.Ailments), _damageSources, _ailments,
                _hands);

        public DamageSpecificationBuilder WithAilments() =>
            new DamageSpecificationBuilder(_mode.RemoveFlags(Mode.Skills), _damageSources, _ailments, _hands);

        public DamageSpecificationBuilder WithHitsAndAilments() =>
            new DamageSpecificationBuilder(_mode.RemoveFlags(Mode.SkillDoT), _damageSources, _ailments, _hands);

        public DamageSpecificationBuilder WithSkills() =>
            new DamageSpecificationBuilder(_mode.RemoveFlags(Mode.Ailments), _damageSources, _ailments, _hands);

        public IEnumerable<IDamageSpecification> Build()
        {
            var sources = _damageSources ?? Enums.GetValues<DamageSource>();
            return sources.SelectMany(Build);
        }

        private IEnumerable<IDamageSpecification> Build(DamageSource damageSource)
        {
            switch (damageSource)
            {
                case DamageSource.Attack:
                    return BuildAttackDamage();
                case DamageSource.Spell:
                    return BuildSpelllDamage();
                case DamageSource.Secondary:
                    return BuildSecondaryDamage();
                case DamageSource.OverTime:
                    return BuildDamageOverTime();
                default:
                    throw new ArgumentOutOfRangeException(nameof(damageSource), damageSource, null);
            }
        }

        private IEnumerable<IDamageSpecification> BuildAttackDamage()
        {
            if (_mode.HasFlag(Mode.Hits))
                foreach (var attackDamageHand in _hands ?? Enums.GetValues<AttackDamageHand>())
                    yield return new AttackDamageSpecification(attackDamageHand);
        }

        private IEnumerable<IDamageSpecification> BuildSpelllDamage()
        {
            if (_mode.HasFlag(Mode.Hits))
                yield return new SkillDamageSpecification(DamageSource.Spell);
        }

        private IEnumerable<IDamageSpecification> BuildSecondaryDamage()
        {
            if (_mode.HasFlag(Mode.Hits))
                yield return new SkillDamageSpecification(DamageSource.Secondary);
        }

        private IEnumerable<IDamageSpecification> BuildDamageOverTime()
        {
            if (_mode.HasFlag(Mode.SkillDoT))
                yield return new SkillDamageSpecification(DamageSource.OverTime);

            if (_mode.HasFlag(Mode.Ailments))
                foreach (var ailment in _ailments?.Select(b => b.Build()) ?? Enums.GetValues<Ailment>())
                    yield return new AilmentDamageSpecification(ailment);
        }
    }
}