using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MoreLinq;
using PoESkillTree.Computation.Builders.Damage;
using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Builders.Stats
{
    internal class StatBuilders : StatBuildersBase, IStatBuilders
    {
        public StatBuilders(IStatFactory statFactory) : base(statFactory)
        {
        }

        public IStatBuilder Level => FromIdentity(typeof(int));
        public IStatBuilder CharacterClass => FromIdentity(typeof(CharacterClass));

        public IStatBuilder Armour => FromIdentity(typeof(int));

        public IEvasionStatBuilder Evasion => new EvasionStatBuilder(StatFactory);

        public IDamageRelatedStatBuilder Accuracy
            => DamageRelatedFromIdentity(typeof(int)).WithSkills(DamageSource.Attack);

        public IDamageRelatedStatBuilder ChanceToHit
            => DamageRelatedFromIdentity(typeof(int)).WithSkills(DamageSource.Attack);

        public IStatBuilder MovementSpeed => FromIdentity(typeof(double));
        public IStatBuilder ActionSpeed => FromIdentity(typeof(double));

        public IDamageRelatedStatBuilder CastRate => new CastRateStatBuilder(StatFactory);
        public IDamageRelatedStatBuilder BaseCastTime => DamageRelatedFromIdentity(typeof(double)).WithHits;
        public IStatBuilder HitRate => FromIdentity(typeof(double));

        public IStatBuilder DamageHasKeyword(DamageSource damageSource, IKeywordBuilder keyword)
        {
            var coreBuilder = new CoreStatBuilderFromCoreBuilder<Keyword>(
                CoreBuilder.Proxy(keyword, (ps, b) => b.Build(ps)),
                (e, k) => StatFactory.MainSkillPartDamageHasKeyword(e, k, damageSource));
            return new StatBuilder(StatFactory, coreBuilder);
        }

        public IStatBuilder AreaOfEffect => FromIdentity(typeof(int));
        public IStatBuilder Radius => FromIdentity(typeof(int));

        public IDamageRelatedStatBuilder Range
            => DamageRelatedFromIdentity(typeof(int)).WithSkills(DamageSource.Attack);

        public IStatBuilder Cooldown => FromIdentity(typeof(double));
        public IStatBuilder CooldownRecoverySpeed => FromIdentity(typeof(double));
        public IStatBuilder Duration => FromIdentity(typeof(double));
        public IStatBuilder SecondaryDuration => FromIdentity(typeof(double));
        public IStatBuilder SkillStage => FromIdentity(typeof(int),
            ExplicitRegistrationTypes.UserSpecifiedValue(double.MaxValue));
        public IStatBuilder MainSkillPart => FromIdentity(typeof(int));

        public ITrapStatBuilders Trap => new TrapStatBuilders(StatFactory);
        public IMineStatBuilders Mine => new MineStatBuilders(StatFactory);
        public ISkillEntityStatBuilders Totem => new TotemStatBuilders(StatFactory);

        public IStatBuilder ItemQuantity => FromIdentity(typeof(int));
        public IStatBuilder ItemRarity => FromIdentity(typeof(int));

        public IStatBuilder PrimordialJewelsSocketed => FromIdentity(typeof(int));
        public IStatBuilder GrandSpectrumJewelsSocketed => FromIdentity(typeof(int));

        public IStatBuilder RampageStacks => FromIdentity(typeof(int));
        public IStatBuilder CharacterSize => FromIdentity(typeof(double));
        public IStatBuilder LightRadius => FromIdentity(typeof(double));
        public IStatBuilder AttachedBrands => FromIdentity(typeof(int));

        public IStatBuilder DamageTakenGainedAsMana =>
            FromIdentity("% of damage taken gained as mana over 4 seconds", typeof(int));

        public IStatBuilder Unique(string name, Type type)
            => FromIdentity(name, type, ExplicitRegistrationTypes.UserSpecifiedValue());

        public IAttributeStatBuilders Attribute => new AttributeStatBuilders(StatFactory);
        public IRequirementStatBuilders Requirements => new RequirementStatBuilders(StatFactory);
        public IPoolStatBuilders Pool => new PoolStatBuilders(StatFactory);
        public IDodgeStatBuilders Dodge => new DodgeStatBuilders(StatFactory);
        public IFlaskStatBuilders Flask => new FlaskStatBuilders(StatFactory);
        public IProjectileStatBuilders Projectile => new ProjectileStatBuilders(StatFactory);
        public IFlagStatBuilders Flag => new FlagStatBuilders(StatFactory);
        public IGemStatBuilders Gem => new GemStatBuilders(StatFactory);
    }

    internal class TrapStatBuilders : StatBuildersBase, ITrapStatBuilders
    {
        public TrapStatBuilders(IStatFactory statFactory) : base(statFactory)
        {
        }

        public IStatBuilder Speed => FromIdentity("Trap throwing speed", typeof(double));
        public IStatBuilder Duration => FromIdentity("Trap duration", typeof(double));
        public IStatBuilder TriggerAoE => FromIdentity("Trap trigger AoE", typeof(int));
    }

    internal class MineStatBuilders : StatBuildersBase, IMineStatBuilders
    {
        public MineStatBuilders(IStatFactory statFactory) : base(statFactory)
        {
        }

        public IStatBuilder Speed => FromIdentity("Mine laying speed", typeof(double));
        public IStatBuilder Duration => FromIdentity("Mine duration", typeof(double));
        public IStatBuilder DetonationAoE => FromIdentity("Mine detonation AoE", typeof(int));
    }

    internal class TotemStatBuilders : StatBuildersBase, ISkillEntityStatBuilders
    {
        public TotemStatBuilders(IStatFactory statFactory) : base(statFactory)
        {
        }

        public IStatBuilder Speed => FromIdentity("Totem placement speed", typeof(double));
        public IStatBuilder Duration => FromIdentity("Totem duration", typeof(double));
    }

    internal class AttributeStatBuilders : StatBuildersBase, IAttributeStatBuilders
    {
        public AttributeStatBuilders(IStatFactory statFactory) : base(statFactory)
        {
        }

        public IStatBuilder Strength => FromIdentity(typeof(int));
        public IStatBuilder Dexterity => FromIdentity(typeof(int));
        public IStatBuilder Intelligence => FromIdentity(typeof(int));
        public IStatBuilder StrengthDamageBonus => FromIdentity(typeof(int));
        public IStatBuilder DexterityEvasionBonus => FromIdentity(typeof(int));
    }

    internal class RequirementStatBuilders : StatBuildersBase, IRequirementStatBuilders
    {
        public RequirementStatBuilders(IStatFactory statFactory) : base(statFactory)
        {
        }

        public IStatBuilder Level => Requirement(typeof(int));
        public IStatBuilder Strength => Requirement(typeof(int));
        public IStatBuilder Dexterity => Requirement(typeof(int));
        public IStatBuilder Intelligence => Requirement(typeof(int));

        private IStatBuilder Requirement(Type dataType, [CallerMemberName] string requiredStat = null)
            => FromStatFactory(e => StatFactory.Requirement(StatFactory.FromIdentity(requiredStat, e, dataType)));
    }

    internal class DodgeStatBuilders : StatBuildersBase, IDodgeStatBuilders
    {
        public DodgeStatBuilders(IStatFactory statFactory) : base(statFactory)
        {
        }

        public IStatBuilder AttackChance => FromIdentity("Chance to dodge attacks", typeof(int));
        public IStatBuilder SpellChance => FromIdentity("Chance to dodge spells", typeof(int));
    }

    internal class FlaskStatBuilders : StatBuildersBase, IFlaskStatBuilders
    {
        public FlaskStatBuilders(IStatFactory statFactory) : base(statFactory)
        {
        }

        public IStatBuilder Effect => FromIdentity("Flask effect", typeof(int));
        public IStatBuilder Duration => FromIdentity("Flask effect duration", typeof(double));
        public IStatBuilder LifeRecovery => FromIdentity("Flask life recovery", typeof(int));
        public IStatBuilder ManaRecovery => FromIdentity("Flask mana recovery", typeof(int));
        public IStatBuilder RecoverySpeed => FromIdentity("Flask recovery speed", typeof(double));
        public IStatBuilder ChargesUsed => FromIdentity("Flask charges used", typeof(int));
        public IStatBuilder ChargesGained => FromIdentity("Flask charges gained", typeof(double));

        public IConditionBuilder IsAnyActive =>
            FromIdentity("Is any flask active?", typeof(bool), ExplicitRegistrationTypes.UserSpecifiedValue()).IsSet;
    }

    internal class ProjectileStatBuilders : StatBuildersBase, IProjectileStatBuilders
    {
        public ProjectileStatBuilders(IStatFactory statFactory) : base(statFactory)
        {
        }

        public IStatBuilder Speed => FromIdentity("Projectile speed", typeof(int));
        public IStatBuilder Count => FromIdentity("Projectile count", typeof(int));

        public IStatBuilder PierceCount => FromIdentity("Projectile pierce count", typeof(int));
        public IStatBuilder ChainCount => FromIdentity("Projectile chain count", typeof(int));
        public IStatBuilder Fork => FromIdentity("Projectile.Fork", typeof(bool));

        public IStatBuilder TravelDistance =>
            FromIdentity("Projectile travel distance", typeof(int), ExplicitRegistrationTypes.UserSpecifiedValue(35));
    }

    internal class FlagStatBuilders : StatBuildersBase, IFlagStatBuilders
    {
        public FlagStatBuilders(IStatFactory statFactory) : base(statFactory)
        {
        }

        public IStatBuilder IgnoreMovementSpeedPenalties =>
            FromIdentity("Ignore movement speed penalties from equipped armor", typeof(bool));

        public IStatBuilder ShieldModifiersApplyToMinionsInstead =>
            FromIdentity("Modifiers on an equipped shield apply to your minions instead", typeof(bool));

        public IStatBuilder IgnoreHexproof => FromIdentity(typeof(bool));

        public IStatBuilder AlwaysMoving
            => FromIdentity("Are you always moving?", typeof(bool), ExplicitRegistrationTypes.UserSpecifiedValue());

        public IStatBuilder AlwaysStationary
            => FromIdentity("Are you always stationary?", typeof(bool), ExplicitRegistrationTypes.UserSpecifiedValue());

        public IStatBuilder BrandAttachedToEnemy
            => FromIdentity("Is your Brand attached to an enemy?", typeof(bool),
                ExplicitRegistrationTypes.UserSpecifiedValue());

        public IStatBuilder BannerPlanted
            => FromIdentity("Is your Banner planted?", typeof(bool), ExplicitRegistrationTypes.UserSpecifiedValue());

        public IStatBuilder IncreasesToSourceApplyToTarget(IStatBuilder source, IStatBuilder target)
            => new StatBuilder(StatFactory,
                new ModifiersApplyToOtherStatCoreStatBuilder(source, target, Form.Increase, StatFactory));

        private class ModifiersApplyToOtherStatCoreStatBuilder : ICoreStatBuilder
        {
            private readonly IStatBuilder _target;
            private readonly IStatBuilder _source;
            private readonly Form _form;
            private readonly IStatFactory _statFactory;

            public ModifiersApplyToOtherStatCoreStatBuilder(
                IStatBuilder source, IStatBuilder target, Form form, IStatFactory statFactory)
                => (_target, _source, _form, _statFactory) = (target, source, form, statFactory);

            public ICoreStatBuilder Resolve(ResolveContext context)
                => new ModifiersApplyToOtherStatCoreStatBuilder(
                    _source.Resolve(context), _target.Resolve(context), _form, _statFactory);

            public ICoreStatBuilder WithEntity(IEntityBuilder entityBuilder)
                => new ModifiersApplyToOtherStatCoreStatBuilder(
                    _source.For(entityBuilder), _target.For(entityBuilder), _form, _statFactory);

            public IEnumerable<StatBuilderResult> Build(BuildParameters parameters)
            {
                return _source.Build(parameters).EquiZip(_target.Build(parameters), MergeResults);

                StatBuilderResult MergeResults(StatBuilderResult source, StatBuilderResult target)
                {
                    var mergedStats = source.Stats.EquiZip(target.Stats,
                        (s, t) => _statFactory.StatIsAffectedByModifiersToOtherStat(t, s, _form));
                    return new StatBuilderResult(mergedStats.ToList(), source.ModifierSource, source.ValueConverter);
                }
            }
        }
    }

    internal class GemStatBuilders : StatBuildersBase, IGemStatBuilders
    {
        public GemStatBuilders(IStatFactory statFactory) : base(statFactory)
        {
        }

        public IStatBuilder IncreaseSupportLevel => FromIdentity("Level of socketed support gems", typeof(int));
    }
}