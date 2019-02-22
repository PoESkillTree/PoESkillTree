using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Parsing;

namespace PoESkillTree.Computation.Builders.Stats
{
    public class DamageSpecificationBuilder : IResolvable<DamageSpecificationBuilder>
    {
        [Flags]
        private enum Mode
        {
            Hits = 1,
            SkillDoT = 2,
            Skills = Hits | SkillDoT,
            Ailments = 4
        }

        private static readonly IReadOnlyList<DamageSource> AllDamageSources = Enums.GetValues<DamageSource>().ToList();
        private static readonly IReadOnlyList<AttackDamageHand> AllHands = Enums.GetValues<AttackDamageHand>().ToList();

        private readonly Mode _mode;
        private readonly DamageSource? _damageSource;
        private readonly AttackDamageHand? _hand;
        private readonly IAilmentBuilder _ailment;

        public DamageSpecificationBuilder()
            : this(Mode.Skills | Mode.Ailments, null, null, null)
        {
        }

        private DamageSpecificationBuilder(
            Mode mode, DamageSource? damageSource, IAilmentBuilder ailment, AttackDamageHand? hand)
        {
            _mode = mode;
            _damageSource = damageSource;
            _ailment = ailment;
            _hand = hand;
        }

        public DamageSpecificationBuilder Resolve(ResolveContext context)
        {
            return new DamageSpecificationBuilder(_mode, _damageSource,
                (IAilmentBuilder) _ailment?.Resolve(context), _hand);
        }

        public DamageSpecificationBuilder With(DamageSource damageSource)
        {
            if (_damageSource is DamageSource s && s != damageSource)
                throw new ParseException(
                    $"Damage source was already restricted to {_damageSource}");
            return new DamageSpecificationBuilder(_mode, damageSource, _ailment, _hand);
        }

        public DamageSpecificationBuilder With(AttackDamageHand hand)
        {
            if (_damageSource is DamageSource s && s != DamageSource.Attack)
                throw new ParseException(
                    $"Damage source was already restricted to {_damageSource}");
            if (_hand is AttackDamageHand h && h != hand)
                throw new ParseException(
                    $"Hand was already restricted to {_hand}");
            return new DamageSpecificationBuilder(_mode, DamageSource.Attack, _ailment, hand);
        }

        public DamageSpecificationBuilder With(IAilmentBuilder ailment)
        {
            if (_ailment != null)
                throw new ParseException($"Ailment was already restricted to {_ailment}");
            return new DamageSpecificationBuilder(_mode.RemoveFlags(Mode.Skills), _damageSource, ailment, _hand);
        }

        public DamageSpecificationBuilder WithHits() =>
            new DamageSpecificationBuilder(_mode.RemoveFlags(Mode.SkillDoT | Mode.Ailments), _damageSource, _ailment,
                _hand);

        public DamageSpecificationBuilder WithAilments() =>
            new DamageSpecificationBuilder(_mode.RemoveFlags(Mode.Skills), _damageSource, _ailment, _hand);

        public DamageSpecificationBuilder WithHitsAndAilments() =>
            new DamageSpecificationBuilder(_mode.RemoveFlags(Mode.SkillDoT), _damageSource, _ailment, _hand);

        public DamageSpecificationBuilder WithSkills() =>
            new DamageSpecificationBuilder(_mode.RemoveFlags(Mode.Ailments), _damageSource, _ailment, _hand);

        public IEnumerable<IDamageSpecification> Build(BuildParameters parameters)
        {
            var intermediateBuilder = new IntermediateBuilder(_mode, Hands(), Ailments(parameters));
            return BuildSkillDamage(intermediateBuilder).Concat(BuildAilmentDamage(intermediateBuilder));
        }

        private IEnumerable<IDamageSpecification> BuildSkillDamage(IntermediateBuilder intermediateBuilder)
        {
            var sources = SingleOrAll(_damageSource, AllDamageSources);
            return sources.SelectMany(intermediateBuilder.BuildSkillDamage);
        }

        private IEnumerable<IDamageSpecification> BuildAilmentDamage(IntermediateBuilder intermediateBuilder)
        {
            if (_damageSource is DamageSource source && source != DamageSource.OverTime)
                return intermediateBuilder.BuildAilmentDamage(source);
            return Enums.GetValues<DamageSource>().SelectMany(intermediateBuilder.BuildAilmentDamage);
        }

        private IReadOnlyList<AttackDamageHand> Hands() =>
            SingleOrAll(_hand, AllHands);

        private IReadOnlyList<Ailment> Ailments(BuildParameters parameters) =>
            SingleOrAll(_ailment?.Build(parameters), AilmentConstants.DamagingAilments);

        private static IReadOnlyList<T> SingleOrAll<T>(T? single, IReadOnlyList<T> all) where T : struct =>
            single.HasValue ? new[] { single.Value } : all;

        private class IntermediateBuilder
        {
            private readonly Mode _mode;
            private readonly IReadOnlyList<AttackDamageHand> _hands;
            private readonly IReadOnlyList<Ailment> _ailments;

            public IntermediateBuilder(Mode mode, IReadOnlyList<AttackDamageHand> hands,
                IReadOnlyList<Ailment> ailments)
            {
                _mode = mode;
                _hands = hands;
                _ailments = ailments;
            }

            public IEnumerable<IDamageSpecification> BuildSkillDamage(DamageSource damageSource)
            {
                if (damageSource == DamageSource.OverTime && !_mode.HasFlag(Mode.SkillDoT))
                    return Enumerable.Empty<IDamageSpecification>();
                if (damageSource != DamageSource.OverTime && !_mode.HasFlag(Mode.Hits))
                    return Enumerable.Empty<IDamageSpecification>();
                switch (damageSource)
                {
                    case DamageSource.Attack:
                        return BuildSkillAttackDamage();
                    case DamageSource.Spell:
                        return BuildSkillSpellDamage();
                    case DamageSource.Secondary:
                        return BuildSkillSecondaryDamage();
                    case DamageSource.OverTime:
                        return BuildSkillDamageOverTime();
                    default:
                        throw new ArgumentOutOfRangeException(nameof(damageSource), damageSource, null);
                }
            }

            private IEnumerable<IDamageSpecification> BuildSkillAttackDamage() =>
                _hands.Select(hand => new SkillAttackDamageSpecification(hand));

            private static IEnumerable<IDamageSpecification> BuildSkillSpellDamage() =>
                new[] { new SkillDamageSpecification(DamageSource.Spell) };

            private static IEnumerable<IDamageSpecification> BuildSkillSecondaryDamage() =>
                new[] { new SkillDamageSpecification(DamageSource.Secondary) };

            private static IEnumerable<IDamageSpecification> BuildSkillDamageOverTime() =>
                new[] { new SkillDamageSpecification(DamageSource.OverTime) };

            public IEnumerable<IDamageSpecification> BuildAilmentDamage(DamageSource damageSource)
            {
                if (!_mode.HasFlag(Mode.Ailments))
                    return Enumerable.Empty<IDamageSpecification>();
                switch (damageSource)
                {
                    case DamageSource.Attack:
                        return BuildAilmentAttackDamage();
                    case DamageSource.Spell:
                        return BuildAilmentSpellDamage();
                    case DamageSource.Secondary:
                        return BuildAilmentSecondaryDamage();
                    case DamageSource.OverTime:
                        return Enumerable.Empty<IDamageSpecification>();
                    default:
                        throw new ArgumentOutOfRangeException(nameof(damageSource), damageSource, null);
                }
            }

            private IEnumerable<IDamageSpecification> BuildAilmentAttackDamage() =>
                from hand in _hands
                from ailment in _ailments
                select new AilmentAttackDamageSpecification(hand, ailment);

            private IEnumerable<IDamageSpecification> BuildAilmentSpellDamage() =>
                _ailments.Select(a => new AilmentDamageSpecification(DamageSource.Spell, a));

            private IEnumerable<IDamageSpecification> BuildAilmentSecondaryDamage() =>
                _ailments.Select(a => new AilmentDamageSpecification(DamageSource.Secondary, a));
        }
    }
}