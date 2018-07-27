using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Buffs;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;

namespace PoESkillTree.Computation.Data
{
    /// <inheritdoc />
    /// <summary>
    /// <see cref="IStatMatchers"/> implementation matching stat parts specifying stats 
    /// (excluding pool and damage stats).
    /// <para>These matchers are referenceable. They can reference <see cref="DamageStatMatchers"/> and
    /// <see cref="PoolStatMatchers"/> in addition to the <see cref="IReferencedMatchers"/> themselves.</para>
    /// </summary>
    public class GeneralStatMatchers : StatMatchersBase
    {
        private readonly IModifierBuilder _modifierBuilder;

        public GeneralStatMatchers(
            IBuilderFactories builderFactories, IMatchContexts matchContexts, IModifierBuilder modifierBuilder)
            : base(builderFactories, matchContexts)
        {
            _modifierBuilder = modifierBuilder;
        }

        public override IReadOnlyList<string> ReferenceNames { get; } = new[] { "StatMatchers" };

        protected override IEnumerable<MatcherData> CreateCollection() =>
            new StatMatcherCollection<IStatBuilder>(_modifierBuilder)
            {
                // attributes
                { "strength", Attribute.Strength },
                { "strength damage bonus", Attribute.StrengthDamageBonus },
                { "dexterity", Attribute.Dexterity },
                { "dexterity evasion bonus", Attribute.DexterityEvasionBonus },
                { "intelligence", Attribute.Intelligence },
                { "strength and dexterity", ApplyOnce(Attribute.Strength, Attribute.Dexterity) },
                { "strength and intelligence", ApplyOnce(Attribute.Strength, Attribute.Intelligence) },
                { "dexterity and intelligence", ApplyOnce(Attribute.Dexterity, Attribute.Intelligence) },
                { "attributes", ApplyOnce(Attribute.Strength, Attribute.Dexterity, Attribute.Intelligence) },
                // offense
                // - damage: see also DamageStatMatchers
                { "chance to deal double damage", Damage.ChanceToDouble },
                { "({DamageTypeMatchers}) damage taken", Reference.AsDamageType.Damage.Taken },
                { "take ({DamageTypeMatchers}) damage", Reference.AsDamageType.Damage.Taken },
                { "damage taken", Damage.Taken },
                { "damage taken from damage over time", Damage.Taken.With(DamageSource.OverTime) },
                // - penetration
                // - crit
                { "(global )?critical strike multiplier", CriticalStrike.Multiplier.WithSkills },
                { "(global )?critical strike chance", CriticalStrike.Chance },
                // - projectiles
                { "projectile speed", Projectile.Speed },
                { "arrow speed", Projectile.Speed, With(Keyword.Bow) },
                // - other
                { "accuracy rating", Stat.Accuracy },
                // defense
                // - life, mana, defences; see also PoolStatMatchers
                { "armour", Armour },
                { "evasion( rating)?", Evasion },
                { "evasion rating and armour", ApplyOnce(Armour, Evasion) },
                { "armour and evasion( rating)?", ApplyOnce(Armour, Evasion) },
                { "armour and energy shield", ApplyOnce(Armour, EnergyShield) },
                { "(global )?defences", ApplyOnce(Armour, Evasion, EnergyShield) },
                // - resistances
                { "({DamageTypeMatchers}) resistance", Reference.AsDamageType.Resistance },
                { "all elemental resistances", Elemental.Resistance },
                { "maximum ({DamageTypeMatchers}) resistance", Reference.AsDamageType.Resistance.Maximum },
                { "all maximum resistances", Elemental.And(Chaos).Resistance.Maximum },
                { "physical damage reduction", Physical.Resistance },
                // - leech
                {
                    @"(?<pool>({PoolStatMatchers})) per second to \k<pool> Leech rate",
                    Reference.AsPoolStat.Leech.RateLimit
                },
                {
                    "damage leeched as ({PoolStatMatchers})",
                    Reference.AsPoolStat.Leech.Of(Damage)
                },
                {
                    "attack damage leeched as ({PoolStatMatchers})",
                    Reference.AsPoolStat.Leech.Of(Damage.With(DamageSource.Attack))
                },
                {
                    "({DamageTypeMatchers}) damage leeched as ({PoolStatMatchers})",
                    References[1].AsPoolStat.Leech.Of(References[0].AsDamageType.Damage)
                },
                {
                    "({DamageTypeMatchers}) attack damage leeched as ({PoolStatMatchers})",
                    References[1].AsPoolStat.Leech.Of(References[0].AsDamageType.Damage.With(DamageSource.Attack))
                },
                {
                    "damage leeched as ({PoolStatMatchers}) and ({PoolStatMatchers})",
                    References[0].AsPoolStat.Leech.Of(Damage), References[1].AsPoolStat.Leech.Of(Damage)
                },
                {
                    "damage dealt by your totems is leeched to you as life",
                    Life.Leech.Of(Damage.For(Entity.Totem))
                },
                { "({PoolStatMatchers}) leeched per second", Reference.AsPoolStat.Leech.Rate },
                // - block
                { "chance to block", Block.AttackChance },
                { "block chance", Block.AttackChance },
                { "block chance with staves", Block.AttackChance, MainHand.Has(Tags.Staff) },
                { "maximum block chance", Block.AttackChance.Maximum },
                { "chance to block spells", Block.SpellChance },
                { "chance to block spells and attacks", ApplyOnce(Block.SpellChance, Block.AttackChance) },
                // - other
                { "chance to dodge attacks", Stat.Dodge.AttackChance },
                { "chance to dodge spell damage", Stat.Dodge.SpellChance },
                { "chance to evade( attacks)?", Evasion.Chance },
                { "chance to evade projectile attacks", Evasion.ChanceAgainstProjectileAttacks },
                { "chance to evade melee attacks", Evasion.ChanceAgainstMeleeAttacks },
                {
                    "damage is taken from ({PoolStatMatchers}) before ({PoolStatMatchers})",
                    AnyDamageType.DamageTakenFrom(References[0].AsPoolStat).Before(References[1].AsPoolStat)
                },
                {
                    "({DamageTypeMatchers}) damage is taken from ({PoolStatMatchers}) before ({PoolStatMatchers})",
                    References[0].AsDamageType.DamageTakenFrom(References[1].AsPoolStat)
                        .Before(References[2].AsPoolStat)
                },
                // speed
                { "attack speed", Stat.CastSpeed.With(DamageSource.Attack) },
                { "cast speed", Stat.CastSpeed.With(DamageSource.Spell), Stat.CastSpeed.With(DamageSource.Secondary) },
                { "movement speed", Stat.MovementSpeed },
                {
                    // not the most elegant solution but by far the easiest
                    @"movement speed \(hidden\)",
                    Stat.MovementSpeed, Not(Flag.IgnoreMovementSpeedPenalties.IsSet)
                },
                { "attack and cast speed", Stat.CastSpeed },
                { "attack, cast( speed)? and movement speed", Stat.CastSpeed, Stat.MovementSpeed },
                { "animation speed", Stat.AnimationSpeed },
                // regen and recharge
                { "({PoolStatMatchers}) regeneration rate", Reference.AsPoolStat.Regen },
                { "energy shield recharge rate", EnergyShield.Recharge },
                {
                    "recovery rate of life, mana and energy shield",
                    Life.RecoveryRate, Mana.RecoveryRate, EnergyShield.RecoveryRate
                },
                // gain
                // charges
                { "(?<!maximum )({ChargeTypeMatchers})", Reference.AsChargeType.Amount },
                { "(?<!while at )maximum ({ChargeTypeMatchers})", Reference.AsChargeType.Amount.Maximum },
                {
                    "maximum ({ChargeTypeMatchers}) and maximum ({ChargeTypeMatchers})",
                    ApplyOnce(References[0].AsChargeType.Amount.Maximum, References[1].AsChargeType.Amount.Maximum)
                },
                { "chance to (gain|grant) an? ({ChargeTypeMatchers})", Reference.AsChargeType.ChanceToGain },
                {
                    "chance to (gain|grant) an? ({ChargeTypeMatchers}) and an? ({ChargeTypeMatchers})",
                    References[0].AsChargeType.ChanceToGain, References[1].AsChargeType.ChanceToGain
                },
                {
                    // Isn't really useful to parse, but if not, no damage related stat would be parsed, leading to
                    // a ParseException because this is "on Hit with Claws".
                    "chance to steal power, frenzy, and endurance charges",
                    Charge.ChanceToSteal
                },
                { "({ChargeTypeMatchers}) duration", Reference.AsChargeType.Duration },
                {
                    "endurance, frenzy and power charge duration",
                    Charge.Endurance.Duration, Charge.Frenzy.Duration, Charge.Power.Duration
                },
                // skills
                { "cooldown recovery speed", Stat.CooldownRecoverySpeed },
                { "mana cost( of skills)?", Mana.Cost },
                { "mana reserved", Mana.Reservation },
                { "skill effect duration", Stat.Duration },
                { "warcry duration", Stat.Duration, With(Keyword.Warcry) },
                { "curse duration", Stat.Duration, With(Keyword.Curse) },
                // traps, mines, totems
                { "trap duration", Stat.Trap.Duration },
                { "mine duration", Stat.Mine.Duration },
                { "totem duration", Stat.Totem.Duration },
                { "traps? placed at a time", Traps.CombinedInstances.Maximum },
                { "remote mines? placed at a time", Mines.CombinedInstances.Maximum },
                { "totems? summoned at a time", Totems.CombinedInstances.Maximum },
                { "trap trigger area of effect", Stat.Trap.TriggerAoE },
                { "mine detonation area of effect", Stat.Mine.DetonationAoE },
                { "trap throwing speed", Stat.Trap.Speed },
                { "mine laying speed", Stat.Mine.Speed },
                { "totem placement speed", Stat.Totem.Speed },
                { "totem life", Life.For(Entity.Totem) },
                // minions
                {
                    "maximum number of skeletons",
                    Skills.SummonSkeleton.Instances.Maximum, Skills.VaalSummonSkeletons.Instances.Maximum
                },
                { "maximum number of spectres", Skills.RaiseSpectre.Instances.Maximum },
                { "maximum number of zombies", Skills.RaiseZombie.Instances.Maximum },
                {
                    "skeleton duration",
                    Stat.Duration, Or(With(Skills.SummonSkeleton), With(Skills.VaalSummonSkeletons))
                },
                { "golem at a time", Golems.CombinedInstances.Maximum },
                // buffs
                { "chance to gain ({BuffMatchers})", Reference.AsBuff.Chance },
                { "({BuffMatchers}) duration", Reference.AsBuff.Duration },
                { "({BuffMatchers}) effect", Reference.AsBuff.Effect },
                { "effect of buffs granted by your golems", Buffs(Entity.Minion).With(Keyword.Golem).Effect },
                {
                    "effect of buffs granted by your elemental golems",
                    Buffs(Entity.Minion).With(Keyword.Golem, Fire).Effect,
                    Buffs(Entity.Minion).With(Keyword.Golem, Cold).Effect,
                    Buffs(Entity.Minion).With(Keyword.Golem, Lightning).Effect
                },
                { "effect of your curses", Buffs(Self).With(Keyword.Curse).Effect },
                { "effect of curses on you", Buffs(targets: Self).With(Keyword.Curse).Effect },
                { "effect of non-curse auras you cast", Buffs(Self).With(Keyword.Aura).Without(Keyword.Curse).Effect },
                { "chance to fortify", Buff.Fortify.Chance },
                { "effect of fortify on you", Buff.Fortify.Effect },
                { "chance for attacks to maim on hit", Buff.Maim.Chance, With(Keyword.Attack) },
                { "chance to taunt", Buff.Taunt.Chance },
                { "chance to blind enemies", Buff.Blind.Chance },
                // ailments
                { "chance to ({AilmentMatchers})( the enemy)?", Reference.AsAilment.Chance },
                {
                    "chance to freeze, shock and ignite",
                    Ailment.Freeze.Chance, Ailment.Shock.Chance, Ailment.Ignite.Chance
                },
                { "chance to avoid being ({AilmentMatchers})", Reference.AsAilment.Avoidance },
                { "chance to avoid elemental ailments", Ailment.Elemental.Select(a => a.Avoidance) },
                { "({AilmentMatchers}) duration( on enemies)?", Reference.AsAilment.Duration },
                { "duration of elemental ailments on enemies", Ailment.Elemental.Select(a => a.Duration) },
                // stun
                { "chance to avoid being stunned", Effect.Stun.Avoidance },
                { "stun and block recovery", Effect.Stun.Recovery, Block.Recovery },
                { "block recovery", Block.Recovery },
                { "stun threshold", Effect.Stun.Threshold },
                { "enemy stun threshold", Effect.Stun.Threshold.For(Enemy) },
                { "stun duration( on enemies)?", Effect.Stun.Duration },
                { "stun duration (?<inner>with .*) on enemies", Effect.Stun.Duration, "${inner}" },
                {
                    "chance to avoid interruption from stuns while casting",
                    Effect.Stun.ChanceToAvoidInterruptionWhileCasting
                },
                { "chance to double stun duration", Effect.Stun.Duration.ChanceToDouble },
                // flasks
                { "effect of flasks( on you)?", Flask.Effect },
                { "flask effect duration", Flask.Duration },
                { "life recovery from flasks", Flask.LifeRecovery },
                { "mana recovery from flasks", Flask.ManaRecovery },
                { "flask charges used", Flask.ChargesUsed },
                { "flask charges gained", Flask.ChargesGained },
                { "flask recovery (speed|rate)", Flask.RecoverySpeed },
                // item quantity/quality
                { "quantity of items found", Stat.ItemQuantity },
                { "rarity of items found", Stat.ItemRarity },
                // range and area of effect
                { "area of effect", Stat.AreaOfEffect },
                { "melee weapon and unarmed range", Stat.Range, With(Keyword.Melee) },
                { "melee weapon range", Stat.Range, And(With(Keyword.Melee), MainHand.HasItem) },
                // other
                { "rampage stacks", Stat.RampageStacks },
                { "chance to knock enemies back", Effect.Knockback.Chance },
                { "knockback distance", Effect.Knockback.Distance },
                { "character size", Stat.CharacterSize },
                { "reflected elemental damage taken", Elemental.ReflectedDamageTaken },
                { "reflected physical damage taken", Physical.ReflectedDamageTaken },
                { "damage taken gained as mana over 4 seconds when hit", Stat.DamageTakenGainedAsMana },
                { "light radius", Stat.LightRadius },
            };
    }
}