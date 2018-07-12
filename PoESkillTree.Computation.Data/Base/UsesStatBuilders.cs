using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Buffs;
using PoESkillTree.Computation.Common.Builders.Charges;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Equipment;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Data.Base
{
    /// <inheritdoc />
    /// <summary>
    /// Base class for matcher implementations providing direct access to <see cref="IStatBuilders"/>,
    /// <see cref="IActionBuilders"/>, <see cref="IBuffBuilders"/>, <see cref="IChargeTypeBuilders"/>,
    /// <see cref="IDamageTypeBuilders"/>, <see cref="IEffectBuilders"/>, <see cref="IEntityBuilders"/>,
    /// <see cref="IEquipmentBuilders"/>, <see cref="IKeywordBuilders"/>, <see cref="ISkillBuilders"/>,
    /// and some of their properties and methods,
    /// in addition to the properties provided by <see cref="UsesFormBuilders"/>.
    /// <para>Also contains a few convenience properties/methods located at the end of the class.</para>
    /// </summary>
    public abstract class UsesStatBuilders : UsesFormBuilders
    {
        protected UsesStatBuilders(IBuilderFactories builderFactories)
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

        protected IPoolStatBuilder Life => Stat.Pool.From(Pool.Life);
        protected IPoolStatBuilder Mana => Stat.Pool.From(Pool.Mana);
        protected IPoolStatBuilder EnergyShield => Stat.Pool.From(Pool.EnergyShield);
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

        protected IBuffBuilderCollection Buffs(IEntityBuilder source = null, params IEntityBuilder[] targets) =>
            Buff.Buffs(source, targets);

        // Charges

        protected IChargeTypeBuilders Charge => BuilderFactories.ChargeTypeBuilders;

        // Damage types

        private IDamageTypeBuilders DamageTypeBuilders => BuilderFactories.DamageTypeBuilders;

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

        private IEquipmentBuilders EquipmentBuilders => BuilderFactories.EquipmentBuilders;

        protected IEquipmentBuilderCollection Equipment => EquipmentBuilders.Equipment;

        // Keywords

        protected IKeywordBuilders Keyword => BuilderFactories.KeywordBuilders;

        // Skills

        protected ISkillBuilders Skill => BuilderFactories.SkillBuilders;

        protected ISkillBuilderCollection Skills => Skill.Skills;

        protected ISkillBuilderCollection Combine(params ISkillBuilder[] skills) =>
            Skill.Combine(skills);


        // Convenience methods
        
        /// <summary>
        /// Returns a stat whose modifiers apply to all given stats, but only once.
        /// (no multiple application if one of the stats is converted to another)
        /// </summary>
        protected static IStatBuilder ApplyOnce(IStatBuilder first, params IStatBuilder[] stats) => 
            stats.Aggregate(first, (s1, s2) => s1.CombineWith(s2));


        /// <summary>
        /// Shortcut for <c>Equipment[ItemSlot.MainHand]</c>.
        /// </summary>
        protected IEquipmentBuilder MainHand => Equipment[ItemSlot.MainHand];

        /// <summary>
        /// Shortcut for <c>Equipment[ItemSlot.OffHand]</c>.
        /// </summary>
        protected IEquipmentBuilder OffHand => Equipment[ItemSlot.OffHand];


        /// <summary>
        /// Shortcut for <c>Skills[Keyword.Trap]</c>.
        /// </summary>
        protected ISkillBuilderCollection Traps => Skills[Keyword.Trap];

        /// <summary>
        /// Shortcut for <c>Skills[Keyword.Mine]</c>.
        /// </summary>
        protected ISkillBuilderCollection Mines => Skills[Keyword.Mine];

        /// <summary>
        /// Shortcut for <c>Skills[Keyword.Totem]</c>.
        /// </summary>
        protected ISkillBuilderCollection Totems => Skills[Keyword.Totem];

        /// <summary>
        /// Shortcut for <c>Skills[Keyword.Golem]</c>.
        /// </summary>
        protected ISkillBuilderCollection Golems => Skills[Keyword.Golem];

        /// <summary>
        /// Shortcut for <c>Skills[Keyword.Minion]</c>.
        /// </summary>
        protected ISkillBuilderCollection Minions => Skills[Keyword.Minion];

        /// <summary>
        /// Shortcut for <c>Fire.And(Lightning).And(Cold)</c>.
        /// </summary>
        protected IDamageTypeBuilder Elemental => ElementalDamageTypes.Aggregate((l, r) => l.And(r));

        protected IDamageTypeBuilder AnyDamageType => AllDamageTypes.Aggregate((l, r) => l.And(r));


        /// <summary>
        /// Gets an enumerable of all damage types.
        /// </summary>
        protected IEnumerable<IDamageTypeBuilder> AllDamageTypes => new[]
        {
            Physical, Fire, Lightning, Cold, Chaos
        };

        /// <summary>
        /// Gets an enumerable of the elemental damage types.
        /// </summary>
        protected IEnumerable<IDamageTypeBuilder> ElementalDamageTypes => new[]
        {
            Fire, Lightning, Cold
        };

        /// <summary>
        /// Gets a stat for damage with all damage types.
        /// </summary>
        protected IDamageStatBuilder Damage => AnyDamageType.Damage;
    }
}