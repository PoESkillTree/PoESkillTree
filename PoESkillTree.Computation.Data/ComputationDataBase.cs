using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Providers.Actions;
using PoESkillTree.Computation.Providers.Buffs;
using PoESkillTree.Computation.Providers.Charges;
using PoESkillTree.Computation.Providers.Conditions;
using PoESkillTree.Computation.Providers.Damage;
using PoESkillTree.Computation.Providers.Effects;
using PoESkillTree.Computation.Providers.Equipment;
using PoESkillTree.Computation.Providers.Forms;
using PoESkillTree.Computation.Providers.Matching;
using PoESkillTree.Computation.Providers.Skills;
using PoESkillTree.Computation.Providers.Stats;
using PoESkillTree.Computation.Providers.Entities;
using PoESkillTree.Computation.Providers.Values;

namespace PoESkillTree.Computation.Data
{
    // TODO better name
    // TODO split this and ComputationData into multiple classes
    //      (yes, I know the constructor is terrible)
    public class ComputationDataBase
    {
        private readonly IEquipmentProviderFactory _equipmentProviderFactory;
        private readonly IFormProviderFactory _formProviderFactory;
        private readonly IMatchContextFactory _matchContextFactory;
        private readonly IDamageTypeProviderFactory _damageTypeProviderFactory;

        public ComputationDataBase(
            IActionProviderFactory actionProviderFactory,
            IBuffProviderFactory buffProviderFactory,
            IChargeTypeProviderFactory chargeTypeProviderFactory,
            IConditionProviderFactory conditionProviderFactory,
            IDamageSourceProviderFactory damageSourceProviderFactory,
            IEquipmentProviderFactory equipmentProviderFactory,
            IFormProviderFactory formProviderFactory,
            IEffectProviderFactory effectProviderFactory,
            IMatchContextFactory matchContextFactory,
            IFluentValueBuilder valueBuilder,
            IKeywordProviderFactory keywordProviderFactory,
            ISkillProviderFactory skillProviderFactory,
            IDamageTypeProviderFactory damageTypeProviderFactory,
            IMatchConditionFactory matchConditionFactory,
            IEntityProviderFactory entityProviderFactory,
            IStatProviderFactory statProviderFactory)
        {
            _equipmentProviderFactory = equipmentProviderFactory;
            _formProviderFactory = formProviderFactory;
            _matchContextFactory = matchContextFactory;
            _damageTypeProviderFactory = damageTypeProviderFactory;
            Source = damageSourceProviderFactory;
            Effect = effectProviderFactory;
            ValueBuilder = valueBuilder;
            Keyword = keywordProviderFactory;
            Skill = skillProviderFactory;
            MatchCondition = matchConditionFactory;
            Entity = entityProviderFactory;
            Stat = statProviderFactory;
            Action = actionProviderFactory;
            Condition = conditionProviderFactory;
            Buff = buffProviderFactory;
            Charge = chargeTypeProviderFactory;
        }

        // Not all properties/methods are repeated here. Only those used often enough to warrant it.

        // Actions

        protected IActionProviderFactory Action { get; }

        protected ISelfToAnyActionProvider Kill => Action.Kill;
        protected IBlockActionProvider Block => Action.Block;
        protected IHitActionProvider Hit => Action.Hit;
        protected ICriticalStrikeActionProvider CriticalStrike => Action.CriticalStrike;

        // Buffs

        protected IBuffProviderFactory Buff { get; }

        protected IBuffProviderCollection Buffs(IEntityProvider source = null,
            IEntityProvider target = null) =>
            Buff.Buffs(source, target);

        // Charges

        protected IChargeTypeProviderFactory Charge { get; }

        // Conditions

        protected IConditionProviderFactory Condition { get; }

        protected IConditionProvider With(ISkillProviderCollection skills) =>
            Condition.With(skills);

        protected IConditionProvider With(ISkillProvider skill) => Condition.With(skill);

        protected IConditionProvider For(params IEntityProvider[] targets) =>
            Condition.For(targets);

        protected IConditionProvider And(params IConditionProvider[] conditions) =>
            Condition.And(conditions);

        protected IConditionProvider Or(params IConditionProvider[] conditions) =>
            Condition.Or(conditions);

        protected IConditionProvider Not(IConditionProvider condition) => Condition.Not(condition);

        // Sources

        protected IDamageSourceProviderFactory Source { get; }

        // Equipment

        protected IEquipmentProviderCollection Equipment => _equipmentProviderFactory.Equipment;

        protected IEquipmentProvider LocalHand => _equipmentProviderFactory.LocalHand;

        // Forms

        protected IFormProvider BaseSet => _formProviderFactory.BaseSet;
        protected IFormProvider PercentIncrease => _formProviderFactory.PercentIncrease;
        protected IFormProvider PercentMore => _formProviderFactory.PercentMore;
        protected IFormProvider BaseAdd => _formProviderFactory.BaseAdd;
        protected IFormProvider PercentReduce => _formProviderFactory.PercentReduce;
        protected IFormProvider PercentLess => _formProviderFactory.PercentLess;
        protected IFormProvider BaseSubtract => _formProviderFactory.BaseSubtract;
        protected IFormProvider TotalOverride => _formProviderFactory.TotalOverride;
        protected IFormProvider MinBaseAdd => _formProviderFactory.MinBaseAdd;
        protected IFormProvider MaxBaseAdd => _formProviderFactory.MaxBaseAdd;
        protected IFormProvider MaximumAdd => _formProviderFactory.MaximumAdd;
        protected IFormProvider SetFlag => _formProviderFactory.SetFlag;
        protected IFormProvider Zero => _formProviderFactory.Zero;
        protected IFormProvider Always => _formProviderFactory.Always;

        // Effects

        protected IEffectProviderFactory Effect { get; }

        protected IAilmentProviderFactory Ailment => Effect.Ailment;
        protected IGroundEffectProviderFactory Ground => Effect.Ground;

        // Matching -- Groups, Values and MatchConditions

        protected IMatchContext<IGroupConverter> Groups => _matchContextFactory.Groups;
        protected IGroupConverter Group => Groups.Single;

        protected IMatchContext<ValueProvider> Values => _matchContextFactory.Values;
        protected ValueProvider Value => Values.Single;

        protected IFluentValueBuilder ValueBuilder { get; }

        protected IMatchConditionFactory MatchCondition { get; }

        // Keywords

        protected IKeywordProviderFactory Keyword { get; }

        // Skills

        protected ISkillProviderFactory Skill { get; }

        protected ISkillProviderCollection Skills => Skill.Skills;

        protected ISkillProviderCollection Combine(params ISkillProvider[] skills) =>
            Skill.Combine(skills);

        // Damage types

        protected IDamageTypeProvider Physical => _damageTypeProviderFactory.Physical;
        protected IDamageTypeProvider Fire => _damageTypeProviderFactory.Fire;
        protected IDamageTypeProvider Lightning => _damageTypeProviderFactory.Lightning;
        protected IDamageTypeProvider Cold => _damageTypeProviderFactory.Cold;
        protected IDamageTypeProvider Chaos => _damageTypeProviderFactory.Chaos;
        protected IDamageTypeProvider RandomElement => _damageTypeProviderFactory.RandomElement;

        // Entities

        protected IEntityProviderFactory Entity { get; }

        protected ISelfProvider Self => Entity.Self;
        protected IEnemyProvider Enemy => Entity.Enemy;
        protected IEntityProvider Ally => Entity.Ally;

        // Stats

        protected IStatProviderFactory Stat { get; }

        protected IAttributeStatProviderFactory Attribute => Stat.Attribute;
        protected IFlaskStatProviderFactory Flask => Stat.Flask;
        protected IProjectileStatProviderFactory Projectile => Stat.Projectile;
        protected IFlagStatProviderFactory Flag => Stat.Flag;
        protected IGemStatProviderFactory Gem => Stat.Gem;

        protected IStatProvider ApplyOnce(params IStatProvider[] stats) => Stat.ApplyOnce(stats);

        protected IPoolStatProvider Life => Stat.Pool.Life;
        protected IPoolStatProvider Mana => Stat.Pool.Mana;
        protected IPoolStatProvider EnergyShield => Stat.Pool.EnergyShield;
        protected IStatProvider Armour => Stat.Armour;
        protected IEvasionStatProvider Evasion => Stat.Evasion;

        // Convenience methods

        protected IEquipmentProvider MainHand => Equipment[ItemSlot.MainHand];
        protected IEquipmentProvider OffHand => Equipment[ItemSlot.OffHand];

        protected IConditionProvider LocalIsMelee =>
            And(LocalHand.Has(Tags.Weapon), Not(LocalHand.Has(Tags.Ranged)));

        protected IConditionProvider Unarmed => Not(MainHand.HasItem);

        protected ISkillProviderCollection Traps => Skills[Keyword.Trap];
        protected ISkillProviderCollection Mines => Skills[Keyword.Mine];
        protected ISkillProviderCollection Totems => Skills[Keyword.Totem];
        protected ISkillProviderCollection Golems => Skills[Keyword.Golem];

        protected IDamageTypeProvider Elemental => Fire.And(Lightning).And(Cold);

        protected IEnumerable<IDamageTypeProvider> AllDamageTypes => new[]
        {
            Physical, Fire, Lightning, Cold, Chaos
        };
        protected IEnumerable<IDamageTypeProvider> ElementalDamageTypes => new[]
        {
            Fire, Lightning, Cold
        };

        protected IDamageStatProvider Damage =>
            AllDamageTypes.Aggregate((l, r) => l.And(r)).Damage;
    }
}