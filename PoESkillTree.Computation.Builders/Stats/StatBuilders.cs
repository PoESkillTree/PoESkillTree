using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MoreLinq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Skills;
using static PoESkillTree.Computation.Common.ExplicitRegistrationTypes;

namespace PoESkillTree.Computation.Builders.Stats
{
    internal class StatBuilders : StatBuildersBase, IStatBuilders
    {
        public StatBuilders(IStatFactory statFactory) : base(statFactory)
        {
        }

        public IStatBuilder Level => FromIdentity(typeof(uint));
        public IStatBuilder CharacterClass => FromIdentity(typeof(CharacterClass));
        public IStatBuilder PassivePoints => FromIdentity(typeof(uint));
        public IStatBuilder AscendancyPassivePoints => FromIdentity(typeof(uint));

        public IStatBuilder Armour => FromIdentity(typeof(uint));

        public IEvasionStatBuilder Evasion => new EvasionStatBuilder(StatFactory);

        public IDamageRelatedStatBuilder Accuracy
            => DamageRelatedFromIdentity(typeof(uint)).WithSkills(DamageSource.Attack);

        public IDamageRelatedStatBuilder ChanceToHit
            => DamageRelatedFromIdentity(typeof(uint)).WithSkills(DamageSource.Attack);

        public IStatBuilder MovementSpeed => FromIdentity(typeof(double));
        public IStatBuilder ActionSpeed => FromIdentity(typeof(double));

        public IDamageRelatedStatBuilder CastRate => new CastRateStatBuilder(StatFactory);
        public IDamageRelatedStatBuilder BaseCastTime => DamageRelatedFromIdentity(typeof(double)).WithHits;
        public IStatBuilder HitRate => FromIdentity(typeof(double));
        public IStatBuilder AdditionalCastRate => FromIdentity(typeof(double));

        public IStatBuilder DamageHasKeyword(DamageSource damageSource, IKeywordBuilder keyword)
        {
            var coreBuilder = new CoreStatBuilderFromCoreBuilder<Keyword>(
                CoreBuilder.Proxy(keyword, (ps, b) => b.Build(ps)),
                (e, k) => StatFactory.MainSkillPartDamageHasKeyword(e, k, damageSource));
            return new StatBuilder(StatFactory, coreBuilder);
        }

        public IStatBuilder AreaOfEffect => FromIdentity(typeof(int));
        public IStatBuilder Radius => FromIdentity(typeof(uint));

        public IDamageRelatedStatBuilder Range
            => DamageRelatedFromIdentity(typeof(uint)).WithSkills(DamageSource.Attack);

        public IStatBuilder Cooldown => FromIdentity(typeof(double));
        public IStatBuilder CooldownRecoverySpeed => FromIdentity(typeof(double));
        public IStatBuilder Duration => FromIdentity(typeof(double));
        public IStatBuilder SecondaryDuration => FromIdentity(typeof(double));
        public IStatBuilder SkillStage => FromIdentity(typeof(uint), UserSpecifiedValue(double.MaxValue));
        public IStatBuilder MainSkillPart => FromIdentity(typeof(uint));

        public ITrapStatBuilders Trap => new TrapStatBuilders(StatFactory);
        public IMineStatBuilders Mine => new MineStatBuilders(StatFactory);
        public ISkillEntityStatBuilders Totem => new TotemStatBuilders(StatFactory);

        public IStatBuilder ItemQuantity => FromIdentity(typeof(int));
        public IStatBuilder ItemRarity => FromIdentity(typeof(int));

        public IStatBuilder PrimordialJewelsSocketed => FromIdentity(typeof(uint));
        public IStatBuilder GrandSpectrumJewelsSocketed => FromIdentity(typeof(uint));

        public IStatBuilder RampageStacks => FromIdentity(typeof(uint));
        public IStatBuilder AttachedBrands => FromIdentity(typeof(uint));

        public IStatBuilder PassiveNodeSkilled(ushort nodeId) => FromIdentity($"{nodeId}.Skilled", typeof(bool));

        public IStatBuilder DamageTakenGainedAsMana => FromIdentity(typeof(uint));

        public ValueBuilder UniqueAmount(string name)
            => FromIdentity(name, typeof(uint), UserSpecifiedValue(0)).Value;

        public IStatBuilder IndependentMultiplier(string identity)
            => FromIdentity(identity, typeof(uint), IndependentResult(NodeType.Increase));

        public IStatBuilder IndependentTotal(string identity)
            => FromIdentity(identity, typeof(uint), IndependentResult(NodeType.Total));

        public IAttributeStatBuilders Attribute => new AttributeStatBuilders(StatFactory);
        public IRequirementStatBuilders Requirements => new RequirementStatBuilders(StatFactory);
        public IPoolStatBuilders Pool => new PoolStatBuilders(StatFactory);
        public IDodgeStatBuilders Dodge => new DodgeStatBuilders(StatFactory);
        public IFlaskStatBuilders Flask => new FlaskStatBuilders(StatFactory);
        public IProjectileStatBuilders Projectile => new ProjectileStatBuilders(StatFactory);
        public IFlagStatBuilders Flag => new FlagStatBuilders(StatFactory);
        public IGemStatBuilders Gem => new GemStatBuilders(StatFactory);
    }

    internal class TrapStatBuilders : PrefixedStatBuildersBase, ITrapStatBuilders
    {
        public TrapStatBuilders(IStatFactory statFactory) : base(statFactory, "Trap")
        {
        }

        public IStatBuilder Speed => FromIdentity("ThrowingSpeed", typeof(double));
        public IStatBuilder Duration => FromIdentity(typeof(double));
        public IStatBuilder TriggerAoE => FromIdentity(typeof(int));
    }

    internal class MineStatBuilders : PrefixedStatBuildersBase, IMineStatBuilders
    {
        public MineStatBuilders(IStatFactory statFactory) : base(statFactory, "Mine")
        {
        }

        public IStatBuilder Speed => FromIdentity("LayingSpeed", typeof(double));
        public IStatBuilder Duration => FromIdentity(typeof(double));
        public IStatBuilder DetonationAoE => FromIdentity(typeof(int));
    }

    internal class TotemStatBuilders : PrefixedStatBuildersBase, ISkillEntityStatBuilders
    {
        public TotemStatBuilders(IStatFactory statFactory) : base(statFactory, "Totem")
        {
        }

        public IStatBuilder Speed => FromIdentity("PlacementSpeed", typeof(double));
        public IStatBuilder Duration => FromIdentity(typeof(double));
    }

    internal class AttributeStatBuilders : StatBuildersBase, IAttributeStatBuilders
    {
        public AttributeStatBuilders(IStatFactory statFactory) : base(statFactory)
        {
        }

        public IStatBuilder Strength => FromIdentity(typeof(uint));
        public IStatBuilder Dexterity => FromIdentity(typeof(uint));
        public IStatBuilder Intelligence => FromIdentity(typeof(uint));
        public IStatBuilder StrengthDamageBonus => FromIdentity(typeof(uint));
        public IStatBuilder DexterityEvasionBonus => FromIdentity(typeof(uint));
    }

    internal class RequirementStatBuilders : StatBuildersBase, IRequirementStatBuilders
    {
        public RequirementStatBuilders(IStatFactory statFactory) : base(statFactory)
        {
        }

        public IStatBuilder Level => Requirement();
        public IStatBuilder Strength => Requirement();
        public IStatBuilder Dexterity => Requirement();
        public IStatBuilder Intelligence => Requirement();

        private IStatBuilder Requirement([CallerMemberName] string requiredStat = null)
            => FromStatFactory(e => StatFactory.Requirement(StatFactory.FromIdentity(requiredStat, e, typeof(uint))));
    }

    internal class DodgeStatBuilders : PrefixedStatBuildersBase, IDodgeStatBuilders
    {
        public DodgeStatBuilders(IStatFactory statFactory) : base(statFactory, "Dodge")
        {
        }

        public IStatBuilder AttackChance => FromIdentity(typeof(uint));
        public IStatBuilder SpellChance => FromIdentity(typeof(uint));
    }

    internal class FlaskStatBuilders : PrefixedStatBuildersBase, IFlaskStatBuilders
    {
        public FlaskStatBuilders(IStatFactory statFactory) : base(statFactory, "Flask")
        {
        }

        public IStatBuilder Effect => FromIdentity(typeof(int));
        public IStatBuilder Duration => FromIdentity(typeof(double));
        public IStatBuilder LifeRecovery => FromIdentity(typeof(int));
        public IStatBuilder ManaRecovery => FromIdentity(typeof(int));
        public IStatBuilder LifeRecoverySpeed => FromIdentity(typeof(double));
        public IStatBuilder ManaRecoverySpeed => FromIdentity(typeof(double));
        public IStatBuilder InstantRecovery => FromIdentity(typeof(uint));
        public IStatBuilder ChargesUsed => FromIdentity(typeof(int));
        public IStatBuilder ChargesGained => FromIdentity(typeof(double));
        public IStatBuilder MaximumCharges => FromIdentity(typeof(uint));
        public IStatBuilder ChanceToGainCharge => FromIdentity(typeof(double));
    }

    internal class ProjectileStatBuilders : PrefixedStatBuildersBase, IProjectileStatBuilders
    {
        public ProjectileStatBuilders(IStatFactory statFactory) : base(statFactory, "Projectile")
        {
        }

        public IStatBuilder Speed => FromIdentity(typeof(int));
        public IStatBuilder Count => FromIdentity(typeof(uint));

        public IStatBuilder PierceCount => FromIdentity(typeof(uint));
        public IStatBuilder ChainCount => FromIdentity(typeof(uint));
        public IStatBuilder Fork => FromIdentity(typeof(bool));

        public ValueBuilder TravelDistance => FromIdentity(typeof(uint), UserSpecifiedValue(35)).Value;
    }

    internal class FlagStatBuilders : StatBuildersBase, IFlagStatBuilders
    {
        public FlagStatBuilders(IStatFactory statFactory) : base(statFactory)
        {
        }

        public IStatBuilder ShieldModifiersApplyToMinionsInstead => FromIdentity(typeof(bool));

        public IStatBuilder IgnoreHexproof => FromIdentity(typeof(bool));
        public IStatBuilder CriticalStrikeChanceIsLucky => FromIdentity(typeof(bool));
        public IStatBuilder FarShot => FromIdentity(typeof(bool));

        public IConditionBuilder AlwaysMoving
            => FromIdentity("Are you always moving?", typeof(bool), UserSpecifiedValue(false)).IsSet;

        public IConditionBuilder AlwaysStationary
            => FromIdentity("Are you always stationary?", typeof(bool), UserSpecifiedValue(false)).IsSet;

        public IConditionBuilder IsBrandAttachedToEnemy
            => FromIdentity("Is your Brand attached to an enemy?", typeof(bool), UserSpecifiedValue(false)).IsSet;

        public IConditionBuilder IsBannerPlanted
            => FromIdentity("Is your Banner planted?", typeof(bool), UserSpecifiedValue(false)).IsSet;

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