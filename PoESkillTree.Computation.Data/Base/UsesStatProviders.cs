using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Builders.Actions;
using PoESkillTree.Computation.Parsing.Builders.Buffs;
using PoESkillTree.Computation.Parsing.Builders.Charges;
using PoESkillTree.Computation.Parsing.Builders.Damage;
using PoESkillTree.Computation.Parsing.Builders.Effects;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Equipment;
using PoESkillTree.Computation.Parsing.Builders.Skills;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Data.Base
{
    public abstract class UsesStatProviders : UsesFormProviders
    {
        protected UsesStatProviders(IBuilderFactories builderFactories)
            : base(builderFactories)
        {
        }

        // Stats directly from IStatBuilders

        protected IStatBuilders Stat => BuilderFactories.StatBuilders;

        protected IAttributeStatBuilders Attribute => Stat.Attribute;
        protected IFlaskStatBuilders Flask => Stat.Flask;
        protected IProjectileStatBuilders Projectile => Stat.Projectile;
        protected IFlagStatBuilders Flag => Stat.Flag;
        protected IGemStatBuilders Gem => Stat.Gem;

        protected IStatBuilder ApplyOnce(params IStatBuilder[] stats) => Stat.ApplyOnce(stats);

        protected IPoolStatBuilder Life => Stat.Pool.Life;
        protected IPoolStatBuilder Mana => Stat.Pool.Mana;
        protected IPoolStatBuilder EnergyShield => Stat.Pool.EnergyShield;
        protected IStatBuilder Armour => Stat.Armour;
        protected IEvasionStatBuilder Evasion => Stat.Evasion;

        // Actions

        protected IActionBuilders Action => BuilderFactories.ActionBuilders;

        protected IActionBuilder Kill => Action.Kill;
        protected IBlockActionBuilder Block => Action.Block;
        protected IActionBuilder Hit => Action.Hit;
        protected ICriticalStrikeActionBuilder CriticalStrike => Action.CriticalStrike;

        // Buffs

        protected IBuffBuilders Buff => BuilderFactories.BuffBuilders;

        protected IBuffBuilderCollection Buffs(IEntityBuilder source = null,
            IEntityBuilder target = null) =>
            Buff.Buffs(source, target);

        // Charges

        protected IChargeTypeBuilders Charge => BuilderFactories.ChargeTypeBuilders;

        // Damage types

        private IDamageTypeBuilders DamageTypeBuilders => 
            BuilderFactories.DamageTypeBuilders;

        protected IDamageTypeBuilder Physical => DamageTypeBuilders.Physical;
        protected IDamageTypeBuilder Fire => DamageTypeBuilders.Fire;
        protected IDamageTypeBuilder Lightning => DamageTypeBuilders.Lightning;
        protected IDamageTypeBuilder Cold => DamageTypeBuilders.Cold;
        protected IDamageTypeBuilder Chaos => DamageTypeBuilders.Chaos;
        protected IDamageTypeBuilder RandomElement => DamageTypeBuilders.RandomElement;

        // Effects

        protected IEffectBuilders Effect => BuilderFactories.EffectBuilders;

        protected IAilmentBuilders Ailment => Effect.Ailment;
        protected IGroundEffectBuilders Ground => Effect.Ground;

        // Entities

        protected IEntityBuilders Entity => BuilderFactories.EntityBuilders;

        protected IEntityBuilder Self => Entity.Self;
        protected IEnemyBuilder Enemy => Entity.Enemy;
        protected IEntityBuilder Ally => Entity.Ally;

        // Equipment

        private IEquipmentBuilders EquipmentBuilders =>
            BuilderFactories.EquipmentBuilders;

        protected IEquipmentBuilderCollection Equipment => EquipmentBuilders.Equipment;

        // Keywords

        protected IKeywordBuilders Keyword => BuilderFactories.KeywordBuilders;

        // Skills

        protected ISkillBuilders Skill => BuilderFactories.SkillBuilders;

        protected ISkillBuilderCollection Skills => Skill.Skills;

        protected ISkillBuilderCollection Combine(params ISkillBuilder[] skills) =>
            Skill.Combine(skills);

        // Sources

        protected IDamageSourceBuilders Source => 
            BuilderFactories.DamageSourceBuilders;

        // Convenience methods

        protected IEquipmentBuilder MainHand => Equipment[ItemSlot.MainHand];
        protected IEquipmentBuilder OffHand => Equipment[ItemSlot.OffHand];

        protected ISkillBuilderCollection Traps => Skills[Keyword.Trap];
        protected ISkillBuilderCollection Mines => Skills[Keyword.Mine];
        protected ISkillBuilderCollection Totems => Skills[Keyword.Totem];
        protected ISkillBuilderCollection Golems => Skills[Keyword.Golem];
        protected ISkillBuilderCollection Minions => Skills[Keyword.Minion];

        protected IDamageTypeBuilder Elemental => Fire.And(Lightning).And(Cold);

        protected IEnumerable<IDamageTypeBuilder> AllDamageTypes => new[]
        {
            Physical, Fire, Lightning, Cold, Chaos
        };
        protected IEnumerable<IDamageTypeBuilder> ElementalDamageTypes => new[]
        {
            Fire, Lightning, Cold
        };

        protected IDamageStatBuilder Damage =>
            AllDamageTypes.Aggregate((l, r) => l.And(r)).Damage;
    }
}