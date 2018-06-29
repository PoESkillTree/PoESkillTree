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

        public IEnumerable<IDamageSpecification> Build()
        {
            if (_damageSource is DamageSource s)
                return Build(s);
            return Enums.GetValues<DamageSource>().SelectMany(Build);
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
            if (!_mode.HasFlag(Mode.Hits))
                yield break;

            if (_hand is AttackDamageHand hand)
            {
                yield return new AttackDamageSpecification(hand);
            }
            else
            {
                foreach (var attackDamageHand in Enums.GetValues<AttackDamageHand>())
                    yield return new AttackDamageSpecification(attackDamageHand);
            }
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

            if (!_mode.HasFlag(Mode.Ailments))
                yield break;

            if (_ailment is null)
            {
                foreach (var ailment in Enums.GetValues<Ailment>())
                    yield return new AilmentDamageSpecification(ailment);
            }
            else
            {
                yield return new AilmentDamageSpecification(_ailment.Build());
            }
        }
    }
}