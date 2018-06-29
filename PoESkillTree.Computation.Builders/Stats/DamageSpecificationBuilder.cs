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

        public IEnumerable<IDamageSpecification> Build() => BuildSkillDamage().Concat(BuildAilmentDamage());

        private IEnumerable<IDamageSpecification> BuildSkillDamage()
        {
            var sources = SingleOrAll(_damageSource, Enums.GetValues<DamageSource>);
            return sources.SelectMany(BuildSkillDamage);
        }

        private IEnumerable<IDamageSpecification> BuildSkillDamage(DamageSource damageSource)
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
                    return BuildSkillSpelllDamage();
                case DamageSource.Secondary:
                    return BuildSkillSecondaryDamage();
                case DamageSource.OverTime:
                    return BuildSkillDamageOverTime();
                default:
                    throw new ArgumentOutOfRangeException(nameof(damageSource), damageSource, null);
            }
        }

        private IEnumerable<IDamageSpecification> BuildSkillAttackDamage() =>
            Hands().Select(hand => new SkillAttackDamageSpecification(hand));

        private IEnumerable<IDamageSpecification> BuildSkillSpelllDamage() =>
            new[] { new SkillDamageSpecification(DamageSource.Spell) };

        private IEnumerable<IDamageSpecification> BuildSkillSecondaryDamage() =>
            new[] { new SkillDamageSpecification(DamageSource.Secondary) };

        private IEnumerable<IDamageSpecification> BuildSkillDamageOverTime() =>
            new[] { new SkillDamageSpecification(DamageSource.OverTime) };

        private IEnumerable<IDamageSpecification> BuildAilmentDamage()
        {
            if (_damageSource is DamageSource source && source != DamageSource.OverTime)
                return BuildAilmentDamage(source);
            return Enums.GetValues<DamageSource>().SelectMany(BuildAilmentDamage);
        }

        private IEnumerable<IDamageSpecification> BuildAilmentDamage(DamageSource damageSource)
        {
            if (!_mode.HasFlag(Mode.Ailments))
                return Enumerable.Empty<IDamageSpecification>();
            switch (damageSource)
            {
                case DamageSource.Attack:
                    return BuildAilmentAttackDamage();
                case DamageSource.Spell:
                    return BuildAilmentSpelllDamage();
                case DamageSource.Secondary:
                    return BuildAilmentSecondaryDamage();
                case DamageSource.OverTime:
                    return Enumerable.Empty<IDamageSpecification>();
                default:
                    throw new ArgumentOutOfRangeException(nameof(damageSource), damageSource, null);
            }
        }

        private IEnumerable<IDamageSpecification> BuildAilmentAttackDamage() =>
            from hand in Hands()
            from ailment in Ailments()
            select new AilmentAttackDamageSpecification(hand, ailment);

        private IEnumerable<IDamageSpecification> BuildAilmentSpelllDamage() =>
            Ailments().Select(a => new AilmentDamageSpecification(DamageSource.Spell, a));

        private IEnumerable<IDamageSpecification> BuildAilmentSecondaryDamage() =>
            Ailments().Select(a => new AilmentDamageSpecification(DamageSource.Secondary, a));

        private IEnumerable<AttackDamageHand> Hands() =>
            SingleOrAll(_hand, Enums.GetValues<AttackDamageHand>);

        private IEnumerable<Ailment> Ailments() =>
            SingleOrAll(_ailment?.Build(), () => AilmentConstants.DamagingAilments);

        private static IEnumerable<T> SingleOrAll<T>(T? single, Func<IEnumerable<T>> all) where T : struct =>
            single.HasValue ? new[] { single.Value } : all();
    }
}