using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Data;
using PoESkillTree.Computation.Parsing.ModifierBuilding;
using static PoESkillTree.Computation.Parsing.Builders.Values.ValueBuilderUtils;

namespace PoESkillTree.Computation.Data
{
    public class GeneralStatMatchers : UsesMatchContext, IStatMatchers
    {
        private readonly IModifierBuilder _modifierBuilder;

        public GeneralStatMatchers(IBuilderFactories builderFactories, 
            IMatchContexts matchContexts, IModifierBuilder modifierBuilder) 
            : base(builderFactories, matchContexts)
        {
            _modifierBuilder = modifierBuilder;
        }

        public bool MatchesWholeLineOnly => false;

        public IEnumerable<MatcherData> Matchers => new StatMatcherCollection(
            _modifierBuilder, ValueFactory)
        {
            // attributes
            { "strength", Attribute.Strength },
            { "strength damage bonus", Attribute.StrengthDamageBonus },
            { "dexterity", Attribute.Dexterity },
            { "dexterity evasion bonus", Attribute.DexterityEvasionBonus },
            { "intelligence", Attribute.Intelligence },
            { "strength and dexterity", ApplyOnce(Attribute.Strength, Attribute.Dexterity) },
            { "strength and intelligence", ApplyOnce(Attribute.Strength, Attribute.Intelligence) },
            {
                "dexterity and intelligence", ApplyOnce(Attribute.Dexterity, Attribute.Intelligence)
            },
            {
                "attributes",
                ApplyOnce(Attribute.Strength, Attribute.Dexterity, Attribute.Intelligence)
            },
            // offense
            // - damage: see also DamageStatMatchers
            { "chance to deal double damage", Damage.ChanceToDouble },
            {
                "({DamageTypeMatchers}) damage as extra ({DamageTypeMatchers}) damage",
                Groups[0].AsDamageType.Damage.AddAs(Groups[1].AsDamageType.Damage)
            },
            {
                "wand ({DamageTypeMatchers}) damage added as ({DamageTypeMatchers}) damage",
                Groups[0].AsDamageType.Damage.AddAs(Groups[1].AsDamageType.Damage),
                Damage.With(Tags.Wand)
            },
            {
                "({DamageTypeMatchers}) damage converted to ({DamageTypeMatchers}) damage",
                Groups[0].AsDamageType.Damage.ConvertTo(Groups[1].AsDamageType.Damage)
            },
            { "({DamageTypeMatchers}) damage taken", Group.AsDamageType.Damage.Taken },
            { "damage taken", Damage.Taken },
            // - penetration
            // - crit
            { "critical strike multiplier", CriticalStrike.Multiplier },
            { "(global )?critical strike chance", CriticalStrike.Chance },
            // - projectiles
            { "projectile speed", Projectile.Speed },
            { "arrow speed", Projectile.Speed, Damage.With(Tags.Bow) },
            { "projectiles?", Projectile.Count },
            // - other
            { "accuracy rating", Stat.Accuracy },
            // defense
            // - life, mana, defences; see also PoolStatMatchers
            { "armour", Armour },
            { "evasion( rating)?", Evasion },
            { "evasion rating and armour", ApplyOnce(Armour, Evasion) },
            { "armour and energy shield", ApplyOnce(Armour, EnergyShield) },
            { "(global )?defences", ApplyOnce(Armour, Evasion, EnergyShield) },
            // - resistances
            { "({DamageTypeMatchers}) resistance", Group.AsDamageType.Resistance },
            { "all elemental resistances", Elemental.Resistance },
            {
                "maximum ({DamageTypeMatchers}) resistance",
                Group.AsDamageType.Resistance.Maximum
            },
            { "all maximum resistances", Elemental.And(Chaos).Resistance.Maximum },
            { "physical damage reduction", Physical.Resistance },
            // - leech
            {
                @"({PoolStatMatchers}) per second to \1 Leech rate",
                Group.AsPoolStat.Leech.RateLimit
            },
            {
                "({DamageStatMatchers}) leeched as ({PoolStatMatchers})",
                Groups[1].AsPoolStat.Leech.Of(Groups[0].AsDamageStat)
            },
            {
                "({DamageStatMatchers}) leeched as ({PoolStatMatchers}) and ({PoolStatMatchers})",
                Groups[1].AsPoolStat.Leech.Of(Groups[0].AsDamageStat),
                Groups[2].AsPoolStat.Leech.Of(Groups[0].AsDamageStat)
            },
            {
                "damage dealt by your totems is leeched to you as life",
                Life.Leech.To(Self).Of(Damage), For(Entity.Totem)
            },
            { "({PoolStatMatchers}) leeched per second", Group.AsPoolStat.Leech.Rate },
            // - block
            { "chance to block", Block.AttackChance },
            { "block chance", Block.AttackChance },
            { "maximum block chance", Block.AttackChance.Maximum },
            { "chance to block spells", Block.SpellChance },
            {
                "block chance applied to spells",
                Block.SpellChance, PercentOf(Block.AttackChance)
            },
            {
                "chance to block spells and attacks",
                ApplyOnce(Block.SpellChance, Block.AttackChance)
            },
            // - other
            { "chance to dodge attacks", Stat.Dodge.AttackChance },
            { "chance to dodge spell damage", Stat.Dodge.SpellChance },
            { "chance to evade( attacks)?", Evasion.Chance },
            { "chance to evade projectile attacks", Evasion.ChanceAgainstProjectileAttacks },
            { "chance to evade melee attacks", Evasion.ChanceAgainstMeleeAttacks },
            {
                "damage is taken from ({PoolStatMatchers}) before ({PoolStatMatchers})",
                Damage.TakenFrom(Groups[0].AsPoolStat).Before(Groups[1].AsPoolStat)
            },
            {
                "({DamageTypeMatchers}) damage is taken from ({PoolStatMatchers}) before ({PoolStatMatchers})",
                Groups[0].AsDamageType.Damage.TakenFrom(Groups[1].AsPoolStat)
                    .Before(Groups[2].AsPoolStat)
            },
            // speed
            { "attack speed", Skills[Keyword.Attack].Speed },
            { "cast speed", Skills.Speed, Not(With(Skills[Keyword.Attack])) },
            { "movement speed", Stat.MovementSpeed },
            {
                // not the most elegant solution but by far the easiest
                @"movement speed \(hidden\)",
                Stat.MovementSpeed, Not(Flag.IgnoreMovementSpeedPenalties.IsSet)
            },
            { "attack and cast speed", Skills.Speed },
            {
                "attack, cast speed and movement speed",
                ApplyOnce(Skills.Speed, Stat.MovementSpeed)
            },
            { "animation speed", Stat.AnimationSpeed },
            // regen and recharge
            { "({PoolStatMatchers}) regeneration rate", Group.AsPoolStat.Regen },
            { "energy shield recharge rate", EnergyShield.Recharge },
            {
                "recovery rate of life, mana and energy shield",
                Life.RecoveryRate, Mana.RecoveryRate, EnergyShield.RecoveryRate
            },
            // gain
            // charges
            { "({ChargeTypeMatchers})", Group.AsChargeType.Amount },
            { "maximum ({ChargeTypeMatchers})", Group.AsChargeType.Amount.Maximum },
            {
                "chance to (gain|grant) a ({ChargeTypeMatchers})",
                Group.AsChargeType.ChanceToGain
            },
            { "({ChargeTypeMatchers}) duration", Group.AsChargeType.Duration },
            {
                "endurance, frenzy and power charge duration",
                Charge.Endurance.Duration, Charge.Frenzy.Duration, Charge.Power.Duration
            },
            // skills
            { "cooldown recovery speed", Skills.CooldownRecoverySpeed },
            {
                "({KeywordMatchers}) cooldown recovery speed",
                Skills[Group.AsKeyword].CooldownRecoverySpeed
            },
            { "mana cost( of skills)?", Skills.Cost },
            { "skill effect duration", Skills.Duration },
            { "mana reserved", Skills.Reservation },
            { "({KeywordMatchers}) duration", Skills[Group.AsKeyword].Duration },
            // traps, mines, totems
            { "traps? placed at a time", Traps.CombinedInstances.Maximum },
            { "remote mines placed at a time", Mines.CombinedInstances.Maximum },
            { "totem summoned at a time", Totems.CombinedInstances.Maximum },
            { "trap trigger area of effect", Stat.TrapTriggerAoE },
            { "mine detonation area of effect", Stat.MineDetonationAoE },
            { "trap throwing speed", Traps.Speed },
            { "mine laying speed", Mines.Speed },
            { "totem placement speed", Totems.Speed },
            { "totem life", Life, For(Entity.Totem) },
            // minions
            {
                "maximum number of skeletons",
                Combine(Skill.SummonSkeleton, Skill.VaalSummonSkeletons).CombinedInstances.Maximum
            },
            { "maximum number of spectres", Skill.RaiseSpectre.Instances.Maximum },
            { "maximum number of zombies", Skill.RaiseZombie.Instances.Maximum },
            {
                "skeleton duration",
                Skill.SummonSkeleton.Duration, Skill.VaalSummonSkeletons.Duration
            },
            { "golem at a time", Golems.CombinedInstances.Maximum },
            // buffs
            {
                "effect of buffs granted by your golems",
                Buffs(Entity.Minion.With(Keyword.Golem)).Effect
            },
            {
                "effect of buffs granted by your elemental golems",
                Buffs(Entity.Minion.With(Keyword.Golem, Elemental)).Effect
            },
            { "effect of your curses", Buffs(Self).With(Keyword.Curse).Effect },
            {
                "effect of curses on you",
                Buffs(target: Self).With(Keyword.Curse).Effect
            },
            {
                "effect of non-curse auras you cast",
                Buffs(Self).With(Keyword.Aura).Without(Keyword.Curse).Effect
            },
            { "effect of fortify on you", Buff.Fortify.Effect },
            { "fortify duration", Buff.Fortify.Duration },
            { "chance for attacks to maim", Buff.Maim.ChanceOn(Enemy), Damage.With(Source.Attack) },
            { "chance to taunt", Buff.Taunt.ChanceOn(Enemy) },
            { "taunt duration", Buff.Taunt.Duration },
            { "chance to blind enemies", Buff.Blind.ChanceOn(Enemy) },
            { "blind duration", Buff.Blind.Duration },
            // flags
            {
                "chance to (gain|grant) ({FlagMatchers})",
                Group.AsFlagStat // chance is handled by StatManipulationMatchers
            },
            { "({FlagMatchers}) duration", Group.AsFlagStat.Duration },
            { "({FlagMatchers}) effect", Group.AsFlagStat.Effect },
            // ailments
            { "chance to ({AilmentMatchers})(the enemy)?", Group.AsAilment.Chance },
            {
                "chance to freeze, shock and ignite",
                Ailment.Freeze.Chance, Ailment.Shock.Chance, Ailment.Ignite.Chance
            },
            { "chance to avoid being ({AilmentMatchers})", Group.AsAilment.Avoidance },
            { "chance to avoid elemental ailments", Ailment.Elemental.Select(a => a.Avoidance) },
            { "({AilmentMatchers}) duration( on enemies)?", Group.AsAilment.Duration },
            {
                "duration of elemental ailments on enemies",
                Ailment.Elemental.Select(a => a.Duration)
            },
            // stun
            { "chance to avoid being stunned", Effect.Stun.Avoidance },
            { "stun and block recovery", Effect.Stun.Recovery, Block.Recovery },
            { "block recovery", Block.Recovery },
            { "stun threshold", Effect.Stun.Threshold },
            { "enemy stun threshold", Enemy.Stat(Effect.Stun.Threshold) },
            { "stun duration( on enemies)?", Enemy.Stat(Effect.Stun.Duration) },
            { "stun duration (with .*) on enemies", Enemy.Stat(Effect.Stun.Duration), "$1" },
            {
                "chance to avoid interruption from stuns while casting",
                Effect.Stun.ChanceToAvoidInterruptionWhileCasting
            },
            { "chance to double stun duration", Effect.Stun.Duration.ChanceToDouble },
            // flasks
            { "effect of flasks", Flask.Effect },
            { "flask effect duration", Flask.Duration },
            { "life recovery from flasks", Flask.LifeRecovery },
            { "mana recovery from flasks", Flask.ManaRecovery },
            { "flask charges used", Flask.ChargesUsed },
            { "flask charges gained", Flask.ChargesGained },
            { "flask recovery speed", Flask.RecoverySpeed },
            // item quantity/quality
            { "quantity of items found", Stat.ItemQuantity },
            { "rarity of items found", Stat.ItemRarity },
            // range and area of effect
            { "area of effect", Skills.AreaOfEffect },
            { "melee weapon and unarmed range", Stat.Range, Or(LocalIsMelee, Unarmed) },
            { "melee weapon range", Stat.Range, LocalIsMelee },
            { "weapon range", Stat.Range, LocalHand.HasItem },
            // other
            { "rampage stacks", Stat.RampageStacks },
            { "chance to knock enemies back", Effect.Knockback.ChanceOn(Enemy) },
            { "knockback distance", Effect.Knockback.Distance },
            // Not really anything that can be done with them (yet), but should still be summed up
            { "character size", Stat.Unique() },
            { "reduced reflected elemental damage taken", Stat.Unique() },
            { "reduced reflected physical damage taken", Stat.Unique() },
            { "damage taken gained as mana over # seconds when hit", Stat.Unique() },
            { "light radius", Stat.Unique() },
        };
    }
}