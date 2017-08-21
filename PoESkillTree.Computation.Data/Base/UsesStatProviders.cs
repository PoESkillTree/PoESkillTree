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
        protected UsesStatProviders(IProviderFactories providerFactories)
            : base(providerFactories)
        {
        }

        // Stats directly from IStatProviderFactory

        protected IStatProviderFactory Stat => ProviderFactories.StatProviderFactory;

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

        protected IActionProviderFactory Action => ProviderFactories.ActionProviderFactory;

        protected ISelfToAnyActionProvider Kill => Action.Kill;
        protected IBlockActionProvider Block => Action.Block;
        protected ISelfToAnyActionProvider Hit => Action.Hit;
        protected ICriticalStrikeActionProvider CriticalStrike => Action.CriticalStrike;

        // Buffs

        protected IBuffProviderFactory Buff => ProviderFactories.BuffProviderFactory;

        protected IBuffProviderCollection Buffs(IEntityProvider source = null,
            IEntityProvider target = null) =>
            Buff.Buffs(source, target);

        // Charges

        protected IChargeTypeProviderFactory Charge => ProviderFactories.ChargeTypeProviderFactory;

        // Damage types

        private IDamageTypeProviderFactory DamageTypeProviderFactory => 
            ProviderFactories.DamageTypeProviderFactory;

        protected IDamageTypeProvider Physical => DamageTypeProviderFactory.Physical;
        protected IDamageTypeProvider Fire => DamageTypeProviderFactory.Fire;
        protected IDamageTypeProvider Lightning => DamageTypeProviderFactory.Lightning;
        protected IDamageTypeProvider Cold => DamageTypeProviderFactory.Cold;
        protected IDamageTypeProvider Chaos => DamageTypeProviderFactory.Chaos;
        protected IDamageTypeProvider RandomElement => DamageTypeProviderFactory.RandomElement;

        // Effects

        protected IEffectProviderFactory Effect => ProviderFactories.EffectProviderFactory;

        protected IAilmentProviderFactory Ailment => Effect.Ailment;
        protected IGroundEffectProviderFactory Ground => Effect.Ground;

        // Entities

        protected IEntityProviderFactory Entity => ProviderFactories.EntityProviderFactory;

        protected ISelfProvider Self => Entity.Self;
        protected IEnemyProvider Enemy => Entity.Enemy;
        protected IEntityProvider Ally => Entity.Ally;

        // Equipment

        private IEquipmentProviderFactory EquipmentProviderFactory =>
            ProviderFactories.EquipmentProviderFactory;

        protected IEquipmentProviderCollection Equipment => EquipmentProviderFactory.Equipment;

        protected IEquipmentProvider LocalHand => EquipmentProviderFactory.LocalHand;

        // Keywords

        protected IKeywordProviderFactory Keyword => ProviderFactories.KeywordProviderFactory;

        // Skills

        protected ISkillProviderFactory Skill => ProviderFactories.SkillProviderFactory;

        protected ISkillProviderCollection Skills => Skill.Skills;

        protected ISkillProviderCollection Combine(params ISkillProvider[] skills) =>
            Skill.Combine(skills);

        // Sources

        protected IDamageSourceProviderFactory Source => 
            ProviderFactories.DamageSourceProviderFactory;

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