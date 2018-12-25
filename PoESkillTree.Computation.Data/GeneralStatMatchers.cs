using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Buffs;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.GameModel.Items;
using PoESkillTree.Utils.Extensions;

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
                { "(all )?attributes", ApplyOnce(Attribute.Strength, Attribute.Dexterity, Attribute.Intelligence) },
                // offense
                // - damage: see also DamageStatMatchers
                { "chance to deal double damage", Damage.ChanceToDouble },
                // - damage multiplier
                {
                    "({DamageTypeMatchers}) damage over time multiplier",
                    Reference.AsDamageType.DamageMultiplier.With(DamageSource.OverTime)
                },
                {
                    "non-ailment ({DamageTypeMatchers}) damage over time multiplier",
                    Reference.AsDamageType.DamageMultiplier.WithSkills(DamageSource.OverTime)
                },
                // - damage taken
                { "damage taken", Damage.Taken },
                { "({DamageTypeMatchers}) damage taken", Reference.AsDamageType.Damage.Taken },
                { "take ({DamageTypeMatchers}) damage", Reference.AsDamageType.Damage.Taken },
                { "damage taken from hits", Damage.Taken.WithHits },
                { "({DamageTypeMatchers}) damage taken from hits", Reference.AsDamageType.Damage.Taken.WithHits },
                { "damage taken from damage over time", Damage.Taken.With(DamageSource.OverTime) },
                {
                    "({DamageTypeMatchers}) damage taken over time",
                    Reference.AsDamageType.Damage.Taken.With(DamageSource.OverTime)
                },
                {
                    "take ({DamageTypeMatchers}) damage over time",
                    Reference.AsDamageType.Damage.Taken.With(DamageSource.OverTime)
                },
                { "damage taken from projectiles", Damage.Taken.With(Keyword.Projectile) },
                {
                    "damage taken from trap or mine hits",
                    Damage.Taken.With(Keyword.Trap).WithHits, Damage.Taken.With(Keyword.Mine).WithHits
                },
                // - damage taken as
                {
                    "({DamageTypeMatchers}) damage from hits taken as lightning damage",
                    Reference.AsDamageType.HitDamageTakenAs(DamageType.Lightning)
                },
                {
                    "({DamageTypeMatchers}) damage from hits taken as cold damage",
                    Reference.AsDamageType.HitDamageTakenAs(DamageType.Cold)
                },
                {
                    "({DamageTypeMatchers}) damage from hits taken as fire damage",
                    Reference.AsDamageType.HitDamageTakenAs(DamageType.Fire)
                },
                {
                    "({DamageTypeMatchers}) damage from hits taken as chaos damage",
                    Reference.AsDamageType.HitDamageTakenAs(DamageType.Chaos)
                },
                // - penetration
                // - crit
                { "(global )?critical strike multiplier", CriticalStrike.Multiplier.WithSkills },
                { "(global )?critical strike chance", CriticalStrike.Chance },
                {
                    "({KeywordMatchers}) critical strike multiplier",
                    CriticalStrike.Multiplier.WithSkills.With(Reference.AsKeyword)
                },
                { "({KeywordMatchers}) critical strike chance", CriticalStrike.Chance.With(Reference.AsKeyword) },
                { "projectiles have critical strike chance", CriticalStrike.Chance.With(Keyword.Projectile) },
                // - projectiles
                { "projectile speed", Projectile.Speed },
                { "arrow speed", Projectile.Speed, And(With(Keyword.Attack), MainHand.Has(Tags.Bow)) },
                // - other
                { "(global )?accuracy rating", Stat.Accuracy },
                { "minion accuracy rating", Stat.Accuracy.For(Entity.Minion) },
                // defense
                // - life, mana, defences; see also PoolStatMatchers
                { "armour", Armour },
                { "evasion( rating)?", Evasion },
                { "evasion rating and armour", ApplyOnce(Armour, Evasion) },
                { "armour and evasion( rating)?", ApplyOnce(Armour, Evasion) },
                { "armour and energy shield", ApplyOnce(Armour, EnergyShield) },
                { "(global )?defences", ApplyOnce(Armour, Evasion, EnergyShield) },
                { "minion maximum life", Life.For(Entity.Minion) },
                // - resistances
                { "({DamageTypeMatchers}) resistance", Reference.AsDamageType.Resistance },
                {
                    "({DamageTypeMatchers}) and ({DamageTypeMatchers}) resistances",
                    References[0].AsDamageType.Resistance, References[1].AsDamageType.Resistance
                },
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
                    "spell damage leeched as ({PoolStatMatchers})",
                    Reference.AsPoolStat.Leech.Of(Damage.With(DamageSource.Spell))
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
                    "attack damage leeched as ({PoolStatMatchers}) and ({PoolStatMatchers})",
                    References[0].AsPoolStat.Leech.Of(Damage.With(DamageSource.Attack)),
                    References[1].AsPoolStat.Leech.Of(Damage.With(DamageSource.Attack))
                },
                {
                    "damage dealt by your totems is leeched to you as life",
                    Life.Leech.Of(Damage.For(Entity.Totem))
                },
                { "({PoolStatMatchers}) leeched per second", Reference.AsPoolStat.Leech.Rate },
                // - block
                { "chance to block attack damage", Block.AttackChance },
                { "chance to block spell damage", Block.SpellChance },
                { "chance to block spell and attack damage", Block.SpellChance, Block.AttackChance },
                { "enemy block chance", ApplyOnce(Block.SpellChance, Block.AttackChance).For(Enemy) },
                { "maximum chance to block attack damage", Block.AttackChance.Maximum },
                // - other
                { "chance to dodge attacks", Stat.Dodge.AttackChance },
                { "chance to dodge attack hits", Stat.Dodge.AttackChance },
                { "chance to dodge spell hits", Stat.Dodge.SpellChance },
                { "chance to dodge attack and spell hits", Stat.Dodge.AttackChance, Stat.Dodge.SpellChance },
                {
                    "enemies have chance to dodge hits",
                    ApplyOnce(Stat.Dodge.AttackChance, Stat.Dodge.SpellChance).For(Enemy)
                },
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
                { "attack speed", Stat.CastRate.With(DamageSource.Attack) },
                {
                    "({KeywordMatchers}) attack speed",
                    Stat.CastRate.With(DamageSource.Attack).With(Reference.AsKeyword)
                },
                { "cast speed", Stat.CastRate.With(DamageSource.Spell), Stat.CastRate.With(DamageSource.Secondary) },
                { "cast speed for curses", Stat.CastRate.With(DamageSource.Attack).With(Keyword.Curse) },
                { "movement speed", Stat.MovementSpeed },
                {
                    // not the most elegant solution but by far the easiest
                    @"movement speed \(hidden\)",
                    Stat.MovementSpeed, Not(Flag.IgnoreMovementSpeedPenalties.IsSet)
                },
                { "attack and cast speed", Stat.CastRate },
                { "attack, cast( speed)? and movement speed", Stat.CastRate, Stat.MovementSpeed },
                { "hit rate", Stat.HitRate },
                { "brand activation frequency", Stat.HitRate, With(Keyword.Brand) },
                // regen and recharge
                { "({PoolStatMatchers}) regeneration rate", Reference.AsPoolStat.Regen },
                { "energy shield recharge rate", EnergyShield.Recharge },
                {
                    "recovery rate of life, mana and energy shield",
                    Life.RecoveryRate, Mana.RecoveryRate, EnergyShield.RecoveryRate
                },
                // gain
                // charges
                { "(?<!maximum |have an? )({ChargeTypeMatchers})", Reference.AsChargeType.Amount },
                { "(?<!while at )maximum ({ChargeTypeMatchers})", Reference.AsChargeType.Amount.Maximum },
                {
                    "maximum ({ChargeTypeMatchers}) and maximum ({ChargeTypeMatchers})",
                    ApplyOnce(References[0].AsChargeType.Amount.Maximum, References[1].AsChargeType.Amount.Maximum)
                },
                { "chance to (gain|grant) (an?|1) ({ChargeTypeMatchers})", Reference.AsChargeType.ChanceToGain },
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
                { "warcry cooldown recovery speed", Stat.CooldownRecoverySpeed, With(Keyword.Warcry) },
                { "cooldown recovery speed for throwing traps", Stat.CooldownRecoverySpeed, With(Keyword.Trap) },
                { "mana cost( of skills)?", Mana.Cost },
                {
                    "mana cost of skills that place mines or throw traps",
                    Mana.Cost, Or(With(Keyword.Mine), With(Keyword.Trap))
                },
                { "mana reserved", AllSkills.Reservation },
                { "mana reservation of skills", AllSkills.Reservation },
                { "mana reservation of ({KeywordMatchers}) skills", Skills[Reference.AsKeyword].Reservation },
                { "skill effect duration", Stat.Duration },
                { "skill duration", Stat.Duration },
                { "buff duration", Stat.Duration },
                { "warcry duration", Stat.Duration, With(Keyword.Warcry) },
                { "curse duration", Stat.Duration, With(Keyword.Curse) },
                // traps, mines, totems
                { "trap duration", Stat.Trap.Duration },
                { "mine duration", Stat.Mine.Duration },
                { "totem duration", Stat.Totem.Duration },
                { "traps? placed at a time", Traps.CombinedInstances.Maximum },
                { "remote mines? placed at a time", Mines.CombinedInstances.Maximum },
                { "totems? summoned at a time", Totems.CombinedInstances.Maximum },
                { "maximum number of summoned totems", Totems.CombinedInstances.Maximum },
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
                { "minion duration", Stat.Duration, With(Keyword.Minion) },
                { "skeleton duration", Stat.Duration, WithSkeletonSkills },
                { "skeleton movement speed", Stat.MovementSpeed.For(Entity.Minion), WithSkeletonSkills },
                { "golem at a time", Golems.CombinedInstances.Maximum },
                // buffs
                // - effect
                { "({BuffMatchers}) effect", Reference.AsBuff.Effect },
                { "effect of ({BuffMatchers})", Reference.AsBuff.Effect },
                { "effect of ({BuffMatchers}) on you", Reference.AsBuff.EffectOn(Self) },
                { "({SkillMatchers}) has buff effect", Reference.AsSkill.Buff.Effect },
                { "effect of buffs granted by your golems", Buffs(Entity.Minion).With(Keyword.Golem).Effect },
                {
                    "effect of buffs granted by your elemental golems",
                    Buffs(Entity.Minion).With(Keyword.Golem, Fire).Effect,
                    Buffs(Entity.Minion).With(Keyword.Golem, Cold).Effect,
                    Buffs(Entity.Minion).With(Keyword.Golem, Lightning).Effect
                },
                { "effect of heralds on you", Buffs(targets: Self).With(Keyword.Herald).Effect },
                { "effect of your curses", Buffs(Self).With(Keyword.Curse).Effect },
                { "effect of curses on you", Buffs(targets: Self).With(Keyword.Curse).Effect },
                {
                    "effect of non-curse auras from your skills",
                    Buffs(Self).With(Keyword.Aura).Without(Keyword.Curse).Effect
                },
                { "warcry buff effect", Buffs(targets: Self).With(Keyword.Warcry).Effect },
                { "aura effect", Skills.ModifierSourceSkill.Buff.Effect },
                { "(?<!area of )effect of aura", Skills.ModifierSourceSkill.Buff.Effect },
                { "effect of supported curses", Skills.ModifierSourceSkill.Buff.Effect },
                { "non-curse auras from supported skills have effect", Skills.ModifierSourceSkill.Buff.Effect },
                { "effect of curse against players", Skills.ModifierSourceSkill.Buff.EffectOn(Entity.Character) },
                // - chance
                { "chance to (gain|grant) ({BuffMatchers})", Reference.AsBuff.Chance },
                { "chance to fortify", Buff.Fortify.Chance },
                { "chance to maim", Buff.Maim.Chance },
                { "chance for attacks to maim", Buff.Maim.Chance.With(DamageSource.Attack) },
                { "chance to taunt", Buff.Taunt.Chance },
                { "chance to blind( enemies)?", Buff.Blind.Chance },
                { "chance to cover rare or unique enemies in ash", Buff.CoveredInAsh.Chance, Enemy.IsRareOrUnique },
                { "chance to impale enemies", Buff.Impale.Chance },
                // - duration
                { "({BuffMatchers}) duration", Reference.AsBuff.Duration },
                { "blinding duration", Buff.Blind.Duration },
                // ailments
                { "chance to ({AilmentMatchers})( the enemy| enemies)?", Reference.AsAilment.Chance },
                {
                    "chance to freeze, shock and ignite",
                    Ailment.Freeze.Chance, Ailment.Shock.Chance, Ailment.Ignite.Chance
                },
                { "chance to cause bleeding", Ailment.Bleed.Chance.With(DamageSource.Attack) },
                { "chance to avoid being ({AilmentMatchers})", Reference.AsAilment.Avoidance },
                { "chance to avoid elemental ailments", Ailment.Elemental.Select(a => a.Avoidance) },
                { "({AilmentMatchers}) duration( on enemies)?", Reference.AsAilment.Duration },
                {
                    "duration of ailments on enemies",
                    Ailment.Elemental.Append(Ailment.Bleed, Ailment.Poison).Select(a => a.Duration)
                },
                { "duration of elemental ailments on enemies", Ailment.Elemental.Select(a => a.Duration) },
                { "effect of shock", Ailment.ShockEffect },
                { "effect of chill( on enemies)?", Ailment.ChillEffect },
                { "effect of non-damaging ailments on enemies", Ailment.ShockEffect, Ailment.ChillEffect },
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
                { "quantity of items dropped by enemies slain", Stat.ItemQuantity },
                { "rarity of items found", Stat.ItemRarity },
                { "rarity of items dropped by enemies slain", Stat.ItemRarity },
                // range and area of effect
                { "area of effect", Stat.AreaOfEffect },
                { "aura area of effect", Stat.AreaOfEffect, With(Keyword.Aura) },
                { "radius", Stat.Radius },
                { "explosion radius", Stat.Radius },
                { "melee weapon and unarmed( attack)? range", Stat.Range.With(Keyword.Melee) },
                { "melee range", Stat.Range.With(Keyword.Melee) },
                { "melee weapon range", Stat.Range.With(Keyword.Melee), MainHand.HasItem },
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