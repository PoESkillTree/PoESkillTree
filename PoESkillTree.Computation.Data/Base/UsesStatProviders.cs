using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Providers;
using PoESkillTree.Computation.Providers.Actions;
using PoESkillTree.Computation.Providers.Buffs;
using PoESkillTree.Computation.Providers.Charges;
using PoESkillTree.Computation.Providers.Damage;
using PoESkillTree.Computation.Providers.Effects;
using PoESkillTree.Computation.Providers.Entities;
using PoESkillTree.Computation.Providers.Equipment;
using PoESkillTree.Computation.Providers.Skills;
using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation.Data.Base
{
    public abstract class UsesStatProviders : UsesFormProviders
    {
        private readonly IDamageTypeProviderFactory _damageTypeProviderFactory;
        private readonly IEquipmentProviderFactory _equipmentProviderFactory;

        protected UsesStatProviders(IProviderFactories providerFactories)
            : base(providerFactories)
        {
            Stat = providerFactories.StatProviderFactory;
            Action = providerFactories.ActionProviderFactory;
            Charge = providerFactories.ChargeTypeProviderFactory;
            Buff = providerFactories.BuffProviderFactory;
            _damageTypeProviderFactory = providerFactories.DamageTypeProviderFactory;
            Keyword = providerFactories.KeywordProviderFactory;
            Entity = providerFactories.EntityProviderFactory;
            Skill = providerFactories.SkillProviderFactory;
            Effect = providerFactories.EffectProviderFactory;
            Source = providerFactories.DamageSourceProviderFactory;
            _equipmentProviderFactory = providerFactories.EquipmentProviderFactory;
        }

        // Stats directly from IStatProviderFactory

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

        // Damage types

        protected IDamageTypeProvider Physical => _damageTypeProviderFactory.Physical;
        protected IDamageTypeProvider Fire => _damageTypeProviderFactory.Fire;
        protected IDamageTypeProvider Lightning => _damageTypeProviderFactory.Lightning;
        protected IDamageTypeProvider Cold => _damageTypeProviderFactory.Cold;
        protected IDamageTypeProvider Chaos => _damageTypeProviderFactory.Chaos;
        protected IDamageTypeProvider RandomElement => _damageTypeProviderFactory.RandomElement;

        // Effects

        protected IEffectProviderFactory Effect { get; }

        protected IAilmentProviderFactory Ailment => Effect.Ailment;
        protected IGroundEffectProviderFactory Ground => Effect.Ground;

        // Entities

        protected IEntityProviderFactory Entity { get; }

        protected ISelfProvider Self => Entity.Self;
        protected IEnemyProvider Enemy => Entity.Enemy;
        protected IEntityProvider Ally => Entity.Ally;

        // Equipment

        protected IEquipmentProviderCollection Equipment => _equipmentProviderFactory.Equipment;

        protected IEquipmentProvider LocalHand => _equipmentProviderFactory.LocalHand;

        // Keywords

        protected IKeywordProviderFactory Keyword { get; }

        // Skills

        protected ISkillProviderFactory Skill { get; }

        protected ISkillProviderCollection Skills => Skill.Skills;

        protected ISkillProviderCollection Combine(params ISkillProvider[] skills) =>
            Skill.Combine(skills);

        // Sources

        protected IDamageSourceProviderFactory Source { get; }

        // Convenience methods

        protected IEquipmentProvider MainHand => Equipment[ItemSlot.MainHand];
        protected IEquipmentProvider OffHand => Equipment[ItemSlot.OffHand];

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