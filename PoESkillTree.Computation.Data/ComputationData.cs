using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Providers;
using static PoESkillTree.Computation.Providers.ActionProviders;
using static PoESkillTree.Computation.Providers.EffectProviders;
using static PoESkillTree.Computation.Providers.BuffProviders;
using static PoESkillTree.Computation.Providers.TargetProviders;
using static PoESkillTree.Computation.Providers.ChargeTypeProviders;
using static PoESkillTree.Computation.Providers.ConditionProviders;
using static PoESkillTree.Computation.Providers.DamageSourceProviders;
using static PoESkillTree.Computation.Providers.DamageTypeProviders;
using static PoESkillTree.Computation.Providers.EquipmentProviders;
using static PoESkillTree.Computation.Providers.FormProviders;
using static PoESkillTree.Computation.Providers.GemModifierProviders;
using static PoESkillTree.Computation.Providers.GroundEffectProviders;
using static PoESkillTree.Computation.Providers.GroupConverters;
using static PoESkillTree.Computation.Providers.KeywordProviders;
using static PoESkillTree.Computation.Providers.MatchConditionProviders;
using static PoESkillTree.Computation.Providers.PoolStatProviders;
using static PoESkillTree.Computation.Providers.SkillProviders;
using static PoESkillTree.Computation.Providers.StatProviders;
using static PoESkillTree.Computation.Providers.ValueProviders;

using DamageStat = PoESkillTree.Computation.Providers.IDamageStatProvider;
using PoolStat = PoESkillTree.Computation.Providers.IPoolStatProvider;
using FlagStat = PoESkillTree.Computation.Providers.IFlagStatProvider;

namespace PoESkillTree.Computation.Data
{
    // most local stats are already added to properties and don't need to be handled here
    // (those stats are handled elsewhere and don't even get here)
    // locality information for local stats not handled elsewhere gets passed to Computation

    /* TODO not handled yet
     * - unique complex value conversion
    each Spectral Throw Projectile gains 5% increased Damage each time it Hits
     * - unique complex condition
     * - adds new sub skill
    can summon up to 3 Skeleton Mages with Summon Skeletons
    Animate Weapon can Animate up to 8 Ranged Weapons
    Minions explode when reduced to Low Life, dealing 33% of their maximum Life as Fire Damage to surrounding Enemies
    Your Minions spread Caustic Cloud on Death, dealing #% of their maximum Life as Chaos Damage per second
     * - adds new parts for skills
    Barrage fires an additional 2 projectiles simultaneously on the first and final attacks
    Burning Arrow has a 10% chance to spread Burning Ground if it Ignites an Enemy
     * - adds new parts for skills: splash damage
    Dual Strike deals Off-Hand Splash Damage to surrounding targets
    Glacial Hammer deals Cold-only Splash Damage to surrounding targets
    Single-target Melee attacks deal Splash Damage to surrounding targets
    #% less Damage to surrounding targets
     * - modifies skills
    Desecrate creates # additional Corpses
     */

    /* not handled: (no useful usage)
     * Jewels:
    Your Golems are aggressive
    #% chance to gain an additional Vaal Soul per Enemy Shattered
    #% chance to gain an additional Vaal Soul on Kill
    Frostbolt Projectiles gain 40% increased Projectile Speed per second
    Ethereal Knives fires Projectiles in a Nova
    #% increased Experience Gain for Corrupted Gems
    Fireball Projectiles gain Radius as they travel farther, up to +4 Radius
    Raised Spectres have a 50% chance to gain Soul Eater for 30 seconds on Kill
    Ground Slam has a 35% increased angle
    Vigilant Strike Fortifies you and Nearby Allies for 12 seconds
    Burning Arrow has a 10% chance to spread Tar if it does not Ignite an Enemy
    #% additional Chance to receive a Critical Strike
    #% increased Rarity of Items dropped by Enemies Shattered by Glacial Hammer
    Magma Orb has 10% increased Area of Effect per Chain
     * passive skills:
    #% Chance for Traps to Trigger an additional time
    Spend Energy Shield before Mana for Skill Costs
    Spend Life instead of Mana for Skills
    Share Endurance, Frenzy and Power Charges with nearby party members
    You can't deal Damage with your Skills yourself
    #% chance to gain a Power, Frenzy or Endurance Charge on Kill
    Enemies Become Chilled as they Unfreeze
    Light Radius is based on Energy Shield instead of Life
    Traps cannot be Damaged for 5 seconds after being Thrown
    Mines cannot be Damaged for 5 seconds after being Placed
    #% chance to Steal Power, Frenzy and Endurance Charges on Hit with Claws
    Enemies Cannot Leech Life From You
    Hits that Stun Enemies have Culling Strike
     * Ascendancies:
    Bleeding Enemies you Kill Explode, dealing #% of their Maximum Life as Physical Damage
    Elemental Ailments are removed when you reach Low Life
    Enemies you Taunt deal #% less Damage against other targets
    Kill Enemies that have #% or lower Life when Hit by your Skills
    #% of Overkill Damage is Leeched as Life
    Life Leech effects are not removed at Full Life
    Remove Bleeding on Flask use
    Gain a Flask Charge when you deal a Critical Strike
    #% chance to gain a Flask Charge when you deal a Critical Strike
    Flasks gain a Charge every 3 seconds
    #% chance for your Flasks to not consume Charges
    Enemies you Kill that are affected by Elemental Ailments grant 100% increased Flask Charges
    Attack Projectiles Return to You after hitting targets
    Moving while Bleeding doesn't cause you to take extra Damage
    When your Traps Trigger, your nearby Traps also Trigger
    #% chance when Placing Mines to Place an additional Mine
    #% increased Mine Arming Speed
    Cannot be Blinded
    #% chance to create a Smoke Cloud when Hit
    #% chance to create a Smoke Cloud when you place a Mine or throw a Trap
    #% chance to Recover #% of Maximum Mana when you use a Skill
    Critical Strikes have Culling Strike
    Elemental Ailments caused by your Skills spread to other nearby Enemies Radius: #
    Energy Shield Recharge is not interrupted by Damage if Recharge began Recently
    #% reduced life regeneration rate
    Cursed Enemies you Kill Explode, dealing a quarter of their maximum Life as Chaos Damage
    Nearby Enemies cannot gain Power, Frenzy or Endurance Charges
    You and Nearby Party Members Share Power, Frenzy and Endurance Charges with each other
    Every # seconds, remove Curses and Elemental Ailments from you
    Every # seconds, #% of Maximum Life Regenerated over one second
    #% additional Block Chance for # second every # seconds
    #% chance to create Consecrated Ground when Hit, lasting # seconds
    #% chance to create Consecrated Ground on Kill, lasting # seconds
    When you or your Totems Kill a Burning Enemy, #% chance for you and your Totems to each gain an Endurance Charge
    Totems Reflect #% of their maximum Life as Fire Damage to nearby Enemies when Hit
    Recover #% of Life and Mana when you use a Warcry
    #% chance that if you would gain Endurance Charges, you instead gain up to your maximum number of Endurance Charges
     */

    public class ComputationData : IComputationData
    {
        // stat lines that are always given independent of gear/tree/gems
        // see http://pathofexile.gamepedia.com/Character
        public IReadOnlyList<string> GivenStats { get; } = new[]
        {
            // while Dual Wielding
            "10% more Attack Speed while Dual Wielding",
            "15% additional Block Chance while Dual Wielding",
            "20% more Attack Physical Damage while Dual Wielding",
            // charges
            "4% additional Physical Damage Reduction per Endurance Charge",
            "+4% to all Elemental Resistances per Endurance Charge",
            "4% increased Attack Speed per Frenzy Charge",
            "4% increased Cast Speed per Frenzy Charge",
            "4% more Damage per Frenzy Charge",
            "50% increased Critical Strike Chance per Power Charge",
            // Rampage
            "1% increased Movement Speed per 10 Rampage Stacks",
            "2% increased Damage per 10 Rampage Stacks",
            "Minions deal 2% increased Damage per 10 Rampage Stacks",
            "Minions gain 1% increased Movement Speed per 10 Rampage Stacks",
            // level based
            "+12 to maximum Life per Level",
            "+2 to Accuracy Rating per Level",
            "+3 to Evasion Rating per Level",
            "+6 to maximum Mana per Level",
            // attribute conversions
            "+1 to maximum Life per 2 Strength",
            "+1 to Strength Damage Bonus per Strength",
            "1% increased Melee Physical Damage per 5 Strength Damage Bonus ceiled",
            "+2 to Accuracy Rating per 1 Dexterity",
            "+1 to Dexterity Evasion Bonus per Dexterity",
            "1% increased Evasion Rating per 5 Dexterity Evasion Bonus ceiled",
            "+1 Mana per 2 Intelligence ceiled",
            "1% increased maximum Energy Shield per 5 Intelligence ceiled",
            // ailments
            "100% chance to ignite on critical strike",
            "100% chance to shock on critical strike",
            "100% chance to chill",
            "100% chance to freeze on critical strike",
            // other
            "100% of non-chaos damage is taken from energy shield before life"
        };

        // Some need to be in here because they need to be of Form.BaseSet for conversion purposes 
        // or to overwrite default base values.
        // Others are here because they don't need handlers only for themselves when in here.
        // For the rest it doesn't matter whether they are in here or in GivenStats.
        public GivenBaseStatsCollection GivenBaseStats { get; } = new GivenBaseStatsCollection
        {
            // base stats
            { Mana.Regen.Percent, 1.75 },
            { EnergyShield.Recharge, 20 },
            { Evasion, 53 },
            { Crit.Multiplier, 150 },
            { Crit.AilmentMultiplier, 150 },
            // minima and maxima
            // - crit
            { Crit.Chance.Maximum, 95 },
            { Crit.Chance.Minimum, 5 },
            // - evasion
            { ChanceToEvade.Maximum, 95 },
            { ChanceToEvade.Minimum, 5 },
            { ChanceToHit.Maximum, 95 },
            { ChanceToHit.Minimum, 5 },
            // - block
            { BlockChance.Maximum, 75 },
            { SpellBlockChance.Maximum, 75 },
            // - dodge
            { AttackDodgeChance.Maximum, 75 },
            { SpellDodgeChance.Maximum, 75 },
            // - charges
            { Charge.Endurance.Amount.Maximum, 3 },
            { Charge.Frenzy.Amount.Maximum, 3 },
            { Charge.Power.Amount.Maximum, 3 },
            // - Rampage
            { RampageStacks.Maximum, 1000 },
            // - leech
            { Life.Leech.RateLimit, 20 },
            { Mana.Leech.RateLimit, 20 },
            // - resistances
            { Elemental.Resistance.Maximum, 75 },
            { Chaos.Resistance.Maximum, 75 },
            { Physical.Resistance.Maximum, 90 },
            // - traps, mines and totems
            { Traps.CombinedInstances.Maximum, 3 },
            { Mines.CombinedInstances.Maximum, 5 },
            { Totems.CombinedInstances.Maximum, 1 },
            // - buffs
            { Buffs(target: Self).With(Keyword.Curse).CombinedLimit, 1 },
        };

        public EffectStatCollection EffectStats { get; } = new EffectStatCollection
        {
            // ailments
            { Ailment.Shock, "50% increased damage taken" },
            { Ailment.Chill, "30% reduced animation speed" },
            { Ailment.Freeze, "100% reduced animation speed" },
            // buffs
            { Buff.Fortify, "20% reduced damage taken from hits" },
            { Buff.Maim, "30% reduced movement speed" },
            { Buff.Intimidate, "10% increased damage taken" },
            { Buff.Blind, "50% less chance to hit" },
            { Buff.Conflux.Igniting, Ailment.Ignite.AddSource(AllDamage) },
            { Buff.Conflux.Shocking, Ailment.Shock.AddSource(AllDamage) },
            { Buff.Conflux.Chilling, Ailment.Chill.AddSource(AllDamage) },
            {
                Buff.Conflux.Elemental,
                Ailment.Ignite.AddSource(AllDamage), Ailment.Shock.AddSource(AllDamage),
                Ailment.Chill.AddSource(AllDamage)
            },
            // ground effects
            { Ground.Consecrated, "4% of maximum life regenerated per second" },
            // flags
            { Flag.Onslaught, "20% increased attack, cast and movement speed" },
            { Flag.UnholyMight, "gain 30% of physical damage as extra chaos damage" },
        };

        // These simply replace a stat line that would get thrown into the matchers with another
        // Reduces redundant handling in some cases and allows elegantly solving other cases
        // As opposed to the matchers, values are not replaced by # placeholders here
        public StatReplacerCollection StatReplacers { get; } = new StatReplacerCollection
        {
            {
                // Grand Spectrum
                @"(.+) per grand spectrum",
                "grand spectrum", "$0"
            },
            {
                // Corrupted Energy Cobalt Jewel
                @"(with \d corrupted items Equipped:) (\d+% of chaos damage does not bypass energy shield), and (\d+% of physical damage bypasses energy shield)",
                "$1 $2", "$1 $3"
            },
            // keystones
            // (some need to be manually split, others are renamed to need no further custom handling)
            {
                // Acrobatics
                @"(\d+% chance to dodge attacks)\. (\d+% less armour and energy shield), (\d+% less chance to block spells and attacks)",
                "$1", "$2", "$3"
            },
            {
                // Eldritch Battery, second stat
                "energy shield protects mana instead of life",
                "100% of non-chaos damage is taken from energy shield before mana",
                "-100% of non-chaos damage is taken from energy shield before life"
            },
            {
                // Chaos Inoculation
                "(maximum life becomes 1), (immune to chaos damage)",
                "$1", "$2"
            },
            {
                // Blood Magic
                @"(removes all mana)\. (spend .*)",
                "$1", "$2"
            },
            {
                // Iron Reflexes
                @"(converts all evasion rating to armour)\. (dexterity provides no bonus to evasion rating)",
                "$1", "-1 to dexterity evasion bonus per dexterity"
            },
            {
                // Iron Grip
                "the increase to physical damage from strength applies to projectile attacks as well as melee attacks",
                "1% increased physical projectile attack damage per 5 strength damage bonus ceiled"
            },
        };
        // other necessary preprocessing (done before anything else is done with the mod line): 
        // - convert to all lowercase
        // - replace all sequences of whitespace characters with a single space

        // Each entry in the matchers arrays is one matcher.
        // The first value of a matcher is the (case-insensitive) regex that has to be matched.
        // The other values specify the effects should a stat line match the regex
        // (except for 'matchesIf', which is another condition that must be satisfied for the entry 
        // to be considered a match).

        /* Order of matcher application:
         * 
         * First: try to find a match in SpecialMatchers against the whole mod line.
         * If success: no further matching
         * If no success: proceed below with unchanged mod line
         * 
         * Then: try the below matchers in order. 
         * - Each matcher regex is appended and prepended by "\b" to make sure they only match whole
         *   words
         * - If one matcher collection has multiple matches, take the one with the longest regex or 
         *   matched substring (first match if multiple of same length) 
         *   (longest regex or longest match?)
         * - If a matcher matches, remove the match in the mod line 
         *   (or replace by substitution string if one is specified)
         * - Once the mod line is empty or consists only of whitespace, the mod line was successfully 
         *   matched.
         * - Merge multiple spaces to a single space before each matching step
         * - If FormAndStatMatchers matchers, skip FormMatchers, StatMatchers and all XStatMatchers
         * StatManipulationMatchers
         * FormAndStatMatchers
         * ValueConversionMatchers (order between this and FormAndStatMatchers doesn't matter yet)
         * FormMatchers
         * StatMatchers
         * DamageStatMatchers
         * PoolStatMatchers        (order between this and DamageStatMatchers doesn't matter yet)
         * ConditionMatchers until no more match
         * remove "(Hidden)"
         */
        // As another form of showing that a mod is not supported, matcher implementations 
        // themselves may signal that
        // they are not yet supported, e.g. by throwing an exception.

        // Groups like "({DamageStatMatchers})" in regexes need to be replaced by all regex strings
        // from the specified matcher collection joined with "|" characters. Group() returns the 
        // matched entry of the matcher collection and is cast to the appropriate Provider type.
        // "{StatMatchers}" matches StatMatchers and all other StatMatcherCollections
        /* Order of matcher referencing: 
         * (matchers are allowed to reference everything further to the right)
         * 
         * SpecialMatchers
         * -> FormAndStatMatchers
         *    ValueConversionMatchers
         *   -> FormMatchers
         *      StatMatchers
         *     -> DamageStatMatchers
         *        PoolStatMatchers
         *        ConditionMatchers
         *       -> everything else
         */

        /* Keystones:
         * - stats are automatically added to EffectMatchers (split at line breaks)
         * - a IFlagStatProvider is automatically created and added to FormAndStatMatchers 
         *   in the form:
         *   { "keystone name", BaseOverride, KeystoneStat, 1 }
         */

        // "{SkillMatchers}" mostly matches skills by name, which does not need to be implemented 
        // in ComputationData
        // TODO something needs to be done to match things like below (specify aliases for skills?)
        //      and parts need to be defined somewhere

        // Skills can be hierarchical, e.g.:
        // Raised Zombie -> default attack (-> default part)
        //               -> slam attack    (-> default part)
        // Shrapnel Shot -> arrow part
        //               -> cone part
        // Frenzy       (-> default part)
        // skill [-> sub skill -> sub skill -> ...] -> part
        // Stats can apply to specific levels and may get inherited by lower levels, e.g. 
        // "Raise Zombie deals 10% increased Damage" applies to both attacks,
        // but "Raised Zombies' Slam Attack deals 10% increased Damage" only applies to the slam attack.
        // SkillMatchers and ISkillProvider can represent any level in a skill's hierarchy.
        // Each specific level has names it can be referenced by, e.g. 
        // "Raised Zombies' Slam Attack" and  "Shrapnel Shot's cone".

        public FormMatcherCollection FormMatchers { get; } = new FormMatcherCollection
        {
            { "#% increased", PercentIncrease },
            { "#% reduced", PercentReduction },
            { "#% more", PercentMore },
            { "#% less", PercentLess},
            { @"\+#%? to", BaseAdd },
            { @"\+?#%?(?= chance)", BaseAdd },
            { @"\+?#% of", BaseAdd },
            { "gain #% of", BaseAdd },
            { "gain #", BaseAdd },
            { "#% additional", BaseAdd },
            { "an additional", BaseAdd, 1 },
            { @"-#% of", BaseSubtract },
            { "-#%? to", BaseSubtract },
            { "can (have|summon) up to # additional", MaximumAdd },
        };

        // serves as match for both FormMatchers and StatMatchers
        // matching this has priority over matching FormMatchers and StatMatchers separately
        // second value is the form, third the stat
        public FormAndStatMatcherCollection FormAndStatMatchers { get; } =
            new FormAndStatMatcherCollection
            {
                // attributes
                // offense
                // - damage
                {
                    @"adds # to # ({DamageStatMatchers})",
                    MultiValued(MinBaseAdd, MaxBaseAdd), Group.As<DamageStat>()
                },
                {
                    @"# to # additional ({DamageStatMatchers})",
                    MultiValued(MinBaseAdd, MaxBaseAdd), Group.As<DamageStat>()
                },
                {
                    @"adds # maximum ({DamageStatMatchers})",
                    MaxBaseAdd, Group.As<DamageStat>()
                },
                {
                    "deal no ({DamageTypeMatchers}) damage", Zero,
                    Group.AsDamageType.Damage
                },
                // - penetration
                {
                    "({DamageStatMatchers}) penetrates #% ({DamageTypeMatchers}) resistances?",
                    BaseAdd, Groups[0].As<DamageStat>().PenetrationOf(Groups[1].AsDamageType)
                },
                {
                    "damage (with .*) penetrates #% ({DamageTypeMatchers}) resistances?",
                    BaseAdd, Damage.PenetrationOf(Group.AsDamageType), "$1"
                },
                {
                    "penetrate #% ({DamageTypeMatchers}) resistances?",
                    BaseAdd, Damage.PenetrationOf(Group.AsDamageType), "$1"
                },
                // - crit
                { @"\+#% critical strike chance", BaseAdd, Crit.Chance },
                { "no critical strike multiplier", Zero, Crit.Multiplier },
                {
                    "no damage multiplier for ailments from critical strikes", Zero,
                    Crit.AilmentMultiplier
                },
                { "never deal critical strikes", Zero, Crit.Chance },
                // - speed
                // - projectiles
                { "skills fire an additional projectile", BaseAdd, Projectile.Count, 1 },
                { "pierces # additional targets", BaseAdd, Projectile.PierceCount },
                { "projectiles pierce an additional target", BaseAdd, Projectile.PierceCount, 1 },
                { "projectiles pierce # targets", BaseAdd, Projectile.PierceCount },
                {
                    "projectiles pierce all nearby targets",
                    TotalOverride, Projectile.PierceCount, double.PositiveInfinity, Enemy.IsNearby
                },
                { @"skills chain \+# times", BaseAdd, Projectile.ChainCount },
                // - other
                { "you hits can't be evaded", Always, ChanceToHit },
                // defense
                // - life, mana, defences
                { "maximum life becomes #", TotalOverride, Life },
                { "removes all mana", Zero, Mana },
                {
                    "converts all evasion rating to armour",
                    TotalOverride, Evasion.ConvertTo(Armour), 100
                },
                { "cannot evade enemy attacks", Zero, ChanceToEvade },
                // - resistances
                {
                    "immune to ({DamageTypeMatchers}) damage",
                    TotalOverride, Group.AsDamageType.Resistance, 100
                },
                // - leech
                {
                    "life leech is applied to energy shield instead", SetFlag,
                    Life.Leech.AppliesTo(EnergyShield)
                },
                { "gain life from leech instantly", SetFlag, Life.InstantLeech },
                { "leech #% of damage as life", BaseAdd, Life.Leech.Of(Damage) },
                // - block
                // - other
                {
                    "chaos damage does not bypass energy shield",
                    Always, DamageTakenFrom(Chaos, EnergyShield).Before(Life)
                },
                {
                    "#% of chaos damage does not bypass energy shield",
                    BaseAdd, DamageTakenFrom(Chaos, EnergyShield).Before(Life),
                    DamageTakenFrom(Chaos, EnergyShield).Before(Mana)
                },
                {
                    "#% of physical damage bypasses energy shield",
                    BaseSubtract, DamageTakenFrom(Physical, EnergyShield).Before(Life)
                },
                {
                    "you take #% reduced extra damage from critical strikes",
                    PercentReduction, Crit.ExtraDamageTaken
                },
                // regen and recharge 
                // (need to be FormAndStatMatcher because they also exist with flat values)
                {
                    "#% of ({PoolStatMatchers}) regenerated per second",
                    BaseAdd, Group.As<PoolStat>().Regen.Percent
                },
                {
                    "#% of ({PoolStatMatchers}) and ({PoolStatMatchers}) regenerated per second",
                    BaseAdd,
                    Groups[0].As<PoolStat>().Regen.Percent, Groups[1].As<PoolStat>().Regen.Percent
                },
                {
                    "regenerate #%( of)?( their)? ({PoolStatMatchers}) per second",
                    BaseAdd, Group.As<PoolStat>().Regen.Percent
                },
                {
                    "# ({PoolStatMatchers}) regenerated per second", BaseAdd,
                    Group.As<PoolStat>().Regen
                },
                {
                    "#% faster start of energy shield recharge", PercentIncrease,
                    EnergyShield.Recharge.Start
                },
                { "life regeneration has no effect", PercentLess, Life.Regen, 100 },
                {
                    "life regeneration is applied to energy shield instead", SetFlag,
                    Life.Regen.AppliesTo(EnergyShield)
                },
                // gain (need to be FormAndStatMatcher because they also exist with flat values)
                {
                    "#% of ({PoolStatMatchers}) gained",
                    BaseAdd, Group.As<PoolStat>().Gain, PercentOf(Group.AsStat)
                },
                {
                    "recover #% of( their)? ({PoolStatMatchers})",
                    BaseAdd, Group.As<PoolStat>().Gain, PercentOf(Group.AsStat)
                },
                {
                    "removes #% of ({PoolStatMatchers})",
                    BaseSubtract, Group.As<PoolStat>().Gain, PercentOf(Group.AsStat)
                },
                { @"\+# ({PoolStatMatchers}) gained", BaseAdd, Group.As<PoolStat>().Gain },
                // charges
                // skills
                // traps, mines, totems
                {
                    "detonating mines is instant",
                    TotalOverride, Skill.DetonateMines.Speed, double.PositiveInfinity
                },
                // minions
                // buffs
                {
                    "you can have one additional curse",
                    BaseAdd, Buffs(target: Self).With(Keyword.Curse).CombinedLimit, 1
                },
                {
                    "enemies can have # additional curse",
                    BaseAdd, Buffs(target: Enemy).With(Keyword.Curse).CombinedLimit
                },
                { "grants fortify", SetFlag, Buff.Fortify.On(Self) },
                { "you have fortify", SetFlag, Buff.Fortify.On(Self) },
                {
                    @"curse enemies with level # ({SkillMatchers})",
                    SetFlag, Buff.Curse(skill: Group.AsSkill, level: Value).On(Enemy)
                },
                { "gain elemental conflux", SetFlag, Buff.Conflux.Elemental.On(Self) },
                // flags
                { "(?<!while )(you have|gain) ({FlagMatchers})", SetFlag, Group.As<FlagStat>() },
                // ailments
                { "causes bleeding", Always, Ailment.Bleed.Chance },
                { "always poison", Always, Ailment.Poison.Chance },
                {
                    "(you )?can afflict an additional ignite on an enemy",
                    BaseAdd, Ailment.Ignite.InstancesOn(Enemy).Maximum, 1
                },
                { "you are immune to ({AilmentMatchers})", Always, Group.AsAilment.Avoidance },
                { "cannot be ({AilmentMatchers})", Always, Group.AsAilment.Avoidance },
                {
                    "(immune to|cannot be affected by) elemental ailments",
                    Always, Ailment.Elemental.Select(a => a.Avoidance)
                },
                // stun
                { "(you )?cannot be stunned", Always, Effect.Stun.Avoidance },
                { "your damaging hits always stun enemies", Always, Effect.Stun.ChanceOn(Enemy) },
                // item quantity/quality
                // range and area of effect
                // other
                { "knocks back enemies", Always, Effect.Knockback.ChanceOn(Enemy) },
            };

        // contains only the stat matcher itself if the stat stands on its own and doesn't interact
        // with anything (e.g. movement speed)
        public StatMatcherCollection StatMatchers { get; } = new StatMatcherCollection
        {
            // attributes
            { "strength", Strength },
            { "strength damage bonus", StrengthDamageBonus },
            { "dexterity", Dexterity },
            { "dexterity evasion bonus", DexterityEvasionBonus },
            { "intelligence", Intelligence },
            { "strength and dexterity", ApplyOnce(Strength, Dexterity) },
            { "strength and intelligence", ApplyOnce(Strength, Intelligence) },
            { "dexterity and intelligence", ApplyOnce(Dexterity, Intelligence) },
            { "attributes", ApplyOnce(Strength, Dexterity, Intelligence) },
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
            { "({DamageStatMatchers}) taken", Group.As<DamageStat>().Taken },
            // - penetration
            // - crit
            { "critical strike multiplier", Crit.Multiplier },
            { "(global )?critical strike chance", Crit.Chance },
            // - projectiles
            { "projectile speed", Projectile.Speed },
            { "arrow speed", Projectile.Speed, With(Skills[Keyword.Bow]) },
            { "projectiles?", Projectile.Count },
            // - other
            { "accuracy rating", Accuracy },
            { "chance to hit", ChanceToHit },
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
                Group.As<PoolStat>().Leech.RateLimit
            },
            {
                "({DamageStatMatchers}) leeched as ({PoolStatMatchers})",
                Groups[1].As<PoolStat>().Leech.Of(Groups[0].As<DamageStat>())
            },
            {
                "({DamageStatMatchers}) leeched as ({PoolStatMatchers}) and ({PoolStatMatchers})",
                Groups[1].As<PoolStat>().Leech.Of(Groups[0].As<DamageStat>()),
                Groups[2].As<PoolStat>().Leech.Of(Groups[0].As<DamageStat>())
            },
            {
                "damage dealt by your totems is leeched to you as life",
                Life.Leech.To(Self).Of(Damage), For(Totem)
            },
            { "({PoolStatMatchers}) leeched per second", Group.As<PoolStat>().Leech.Rate },
            // - block
            { "chance to block", BlockChance },
            { "block chance", BlockChance },
            { "maximum block chance", BlockChance.Maximum },
            { "chance to block spells", SpellBlockChance },
            { "block chance applied to spells", SpellBlockChance, PercentOf(BlockChance) },
            { "chance to block spells and attacks", ApplyOnce(SpellBlockChance, BlockChance) },
            // - other
            { "chance to dodge attacks", AttackDodgeChance },
            { "chance to dodge spell damage", SpellDodgeChance },
            { "chance to evade( attacks)?", ChanceToEvade },
            { "chance to evade projectile attacks", ChanceToEvadeProjectileAttacks },
            { "chance to evade melee attacks", ChanceToEvadeMeleeAttacks },
            {
                "damage is taken from ({PoolStatMatchers}) before ({PoolStatMatchers})",
                DamageTakenFrom(Groups[0].As<PoolStat>()).Before(Groups[1].As<PoolStat>())
            },
            {
                "({DamageTypeMatchers}) damage is taken from ({PoolStatMatchers}) before ({PoolStatMatchers})",
                DamageTakenFrom(Groups[0].AsDamageType, Groups[1].As<PoolStat>())
                    .Before(Groups[2].As<PoolStat>())
            },
            // speed
            { "attack speed", AttackSpeed },
            { "cast speed", CastSpeed },
            { "movement speed", MovementSpeed },
            //  not the most elegant solution but by far the easiest
            {
                @"movement speed \(hidden\)",
                MovementSpeed, Not(Flag.IgnoreMovementSpeedPenalties.IsSet)
            },
            { "attack and cast speed", ApplyOnce(AttackSpeed, CastSpeed) },
            {
                "attack, cast speed and movement speed",
                ApplyOnce(AttackSpeed, CastSpeed, MovementSpeed)
            },
            { "animation speed", AnimationSpeed },
            // regen and recharge
            { "({PoolStatMatchers}) regeneration rate", Group.As<PoolStat>().Regen },
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
            { "trap trigger area of effect", TrapTriggerAoE },
            { "mine detonation area of effect", MineDetonationAoE },
            { "trap throwing speed", Traps.Speed },
            { "mine laying speed", Mines.Speed },
            { "totem placement speed", Totems.Speed },
            { "totem life", Life, For(Totem) },
            // minions
            {
                "maximum number of skeletons",
                Combine(Skill.SummonSkeletons, Skill.VaalSummonSkeletons).CombinedInstances.Maximum
            },
            { "maximum number of spectres", Skill.RaiseSpectre.Instances.Maximum },
            { "maximum number of zombies", Skill.RaiseZombie.Instances.Maximum },
            { "skeleton duration", Skill.SummonSkeletons.Duration, Skill.VaalSummonSkeletons.Duration },
            { "golem at a time", Golems.CombinedInstances.Maximum },
            // buffs
            {
                "effect of buffs granted by your golems",
                Buffs(Minion.With(Keyword.Golem)).EffectIncrease
            },
            {
                "effect of buffs granted by your elemental golems",
                Buffs(Minion.With(Keyword.Golem, Elemental)).EffectIncrease
            },
            { "effect of your curses", Buffs(Self).With(Keyword.Curse).EffectIncrease },
            {
                "effect of curses on you",
                Buffs(target: Self).With(Keyword.Curse).EffectIncrease
            },
            {
                "effect of non-curse auras you cast",
                Buffs(Self).With(Keyword.Aura).Without(Keyword.Curse).EffectIncrease
            },
            { "effect of fortify on you", Buff.Fortify.EffectIncrease },
            { "fortify duration", Buff.Fortify.Duration },
            { "chance for attacks to maim", Buff.Maim.ChanceOn(Enemy), Damage.With(Source.Attack) },
            { "chance to taunt", Buff.Taunt.ChanceOn(Enemy) },
            { "taunt duration", Buff.Taunt.Duration },
            { "chance to blind enemies", Buff.Blind.ChanceOn(Enemy) },
            { "blind duration", Buff.Blind.Duration },
            // flags
            {
                "chance to (gain|grant) ({FlagMatchers})",
                Group.As<FlagStat>() // chance is handled by StatManipulationMatchers
            },
            { "({FlagMatchers}) duration", Group.As<FlagStat>().DurationIncrease },
            { "({FlagMatchers}) effect", Group.As<FlagStat>().EffectIncrease },
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
            { "stun and block recovery", Effect.Stun.Recovery, BlockRecovery },
            { "block recovery", BlockRecovery },
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
            { "quantity of items found", ItemQuantity },
            { "rarity of items found", ItemRarity },
            // range and area of effect
            { "area of effect", AreaOfEffect },
            { "melee weapon and unarmed range", Range, Or(LocalIsMelee, Unarmed) },
            { "melee weapon range", Range, LocalIsMelee },
            { "weapon range", Range, LocalHand.HasItem },
            // other
            { "rampage stacks", RampageStacks },
            { "chance to knock enemies back", Effect.Knockback.ChanceOn(Enemy) },
            { "knockback distance", Effect.Knockback.Distance },
            // not really anything that can be done with them, but should still be summed up
            // (might need names if they should be displayed somewhere specific)
            { "character size" },
            { "reduced reflected elemental damage taken" },
            { "reduced reflected physical damage taken" },
            { "damage taken gained as mana over # seconds when hit" },
            { "light radius" },
        };

        public StatMatcherCollection<DamageStat> DamageStatMatchers { get; } =
            new StatMatcherCollection<DamageStat>
            {
                // unspecific
                { "damage", Damage },
                // by source
                { "attack damage", Damage, Damage.With(Source.Attack) },
                { "spell damage", Damage, Damage.With(Source.Spell) },
                { "damage over time", Damage, Damage.With(Source.DamageOverTime) },
                // by type
                { "({DamageTypeMatchers}) damage", Group.AsDamageType.Damage },
                { "damage of a random element", RandomElement.Damage },
                // by keyword
                //  (prevent it from matching damage by source)
                {
                    "(?!attack|spell)({KeywordMatchers}) damage",
                    Damage, Damage.With(Group.AsKeyword)
                },
                {
                    "trap and mine damage",
                    Damage, Or(Damage.With(Keyword.Trap), Damage.With(Keyword.Mine))
                },
                // by source and type
                { "attack physical damage", Physical.Damage, Damage.With(Source.Attack) },
                { "physical attack damage", Physical.Damage, Damage.With(Source.Attack) },
                {
                    "({DamageTypeMatchers}) damage to attacks",
                    Group.AsDamageType.Damage, Damage.With(Source.Attack)
                },
                {
                    "({DamageTypeMatchers}) attack damage",
                    Group.AsDamageType.Damage, Damage.With(Source.Attack)
                },
                {
                    "({DamageTypeMatchers}) spell damage",
                    Group.AsDamageType.Damage, Damage.With(Source.Spell)
                },
                { "burning damage", Fire.Damage, Damage.With(Source.DamageOverTime) },
                // other combinations
                { "physical melee damage", Physical.Damage, Damage.With(Keyword.Melee) },
                { "claw physical damage", Physical.Damage, Damage.With(Tags.Claw) },
                { "physical weapon damage", Physical.Damage, Damage.With(Tags.Weapon) },
                {
                    "physical projectile attack damage",
                    Physical.Damage, Damage.With(Source.Attack).And(Keyword.Projectile)
                },
            };

        public StatMatcherCollection<PoolStat> PoolStatMatchers { get; } =
            new StatMatcherCollection<PoolStat>
            {
                { "(maximum )?life", Life },
                { "(maximum )?mana", Mana },
                { "(maximum )?energy shield", EnergyShield },
            };

        public DamageTypeMatcherCollection DamageTypeMatchers { get; } =
            new DamageTypeMatcherCollection
            {
                { "physical", Physical },
                { "fire", Fire },
                { "lightning", Lightning },
                { "cold", Cold },
                { "chaos", Chaos },
                // combinations
                { "elemental", Elemental },
                { "physical, cold and lightning", Physical.And(Cold).And(Lightning) },
                { "physical and fire", Physical.And(Fire) },
                // inverse
                { "non-fire", Fire.Invert },
                { "non-chaos", Chaos.Invert },
            };

        public ChargeTypeMatcherCollection ChargeTypeMatchers { get; } =
            new ChargeTypeMatcherCollection
            {
                { "endurance charges?", Charge.Endurance },
                { "power charges?", Charge.Power },
                { "endurance charges?", Charge.Endurance },
            };

        public AilmentMatcherCollection AilmentMatchers { get; } = new AilmentMatcherCollection
        {
            // chance to x/x duration
            { "ignite", Ailment.Ignite },
            { "shock", Ailment.Shock },
            { "chill", Ailment.Chill },
            { "freeze", Ailment.Freeze },
            { "bleed", Ailment.Bleed },
            { "cause bleeding", Ailment.Bleed },
            { "poison", Ailment.Poison },
            // being/while/against x
            { "ignited", Ailment.Ignite },
            { "shocked", Ailment.Shock },
            { "chilled", Ailment.Chill },
            { "frozen", Ailment.Freeze },
            { "bleeding", Ailment.Bleed },
            { "poisoned", Ailment.Poison },
        };

        public FlagMatcherCollection FlagMatchers { get; } = new FlagMatcherCollection
        {
            { "onslaught", Flag.Onslaught },
            { "unholy might", Flag.UnholyMight },
            { "phasing", Flag.Phasing },
        };

        public KeywordMatcherCollection KeywordMatchers { get; } = new KeywordMatcherCollection
        {
            { "melee", Keyword.Melee },
            { "attacks?", Keyword.Attack },
            { "bows?", Keyword.Bow },
            { "projectiles?", Keyword.Projectile },
            { "golems?", Keyword.Golem },
            { "traps?", Keyword.Trap },
            { "mines?", Keyword.Mine },
            { "totems?", Keyword.Totem },
            { "curses?", Keyword.Curse },
            { "auras?", Keyword.Aura },
            { "area", Keyword.AreaOfEffect },
            { "spells?", Keyword.Spell },
            { "warcry", Keyword.Warcry },
        };

        public IReadOnlyDictionary<string, ItemSlot> ItemSlotMatchers { get; } =
            new Dictionary<string, ItemSlot>
            {
                // Helmet in Hierophant and Helm in Ascendant's Hierophant ...
                { "helmet", ItemSlot.Helm},
                { "helm", ItemSlot.Helm}, 
                { "gloves", ItemSlot.Gloves },
                { "boots", ItemSlot.Boots }
            };

        public ActionMatcherCollection ActionMatchers { get; } = new ActionMatcherCollection
        {
            { "kill(ed)?", Kill },
            { "block(ed)?", Block },
            { "hit", Hit },
            { "critical strike", CriticalStrike },
            { "non-critical strike", NonCriticalStrike },
        };

        public ConditionMatcherCollection ConditionMatchers { get; } =
            new ConditionMatcherCollection
            {
                // actions
                { "on ({ActionMatchers})", Group.AsAction.On() },
                { "if you've ({ActionMatchers}) recently", Group.AsAction.Recently() },
                { "if you haven't ({ActionMatchers}) recently", Not(Group.AsAction.Recently()) },
                {
                    "when you ({ActionMatchers}) a rare or unique enemy",
                    Group.AsAction.Against(Enemy).On(t => t.IsRareOrUnique)
                },
                { "on ({KeywordMatchers}) kill", Kill.On(withKeyword: Group.AsKeyword) },
                { "when you kill an enemy,", Kill.Against(Enemy).On() },
                {
                    "if you've killed a maimed enemy recently",
                    Kill.Against(Enemy).Recently(Buff.Maim.IsOn)
                },
                {
                    "if you've killed a bleeding enemy recently",
                    Kill.Against(Enemy).Recently(Ailment.Bleed.IsOn)
                },
                {
                    "if you've killed a cursed enemy recently",
                    Kill.Against(Enemy).Recently(
                        e => Buffs(target: e).With(Keyword.Curse).Any())
                },
                {
                    "if you or your totems have killed recently",
                    Or(Kill.Recently(), Kill.By(Totem).Recently())
                },
                { "when they block", Block.On() },
                { "when you block", Block.On() },
                {
                    "if you've blocked a hit from a unique enemy recently",
                    Block.Against(Enemy).Recently(s => s.IsUnique)
                },
                { "(from|with) hits", Hit.On() },
                { "hits deal", Hit.On() },
                { "when you are hit", Hit.Taken.On() },
                { "if you've been hit recently", Hit.Taken.Recently() },
                { "if you haven't been hit recently", Not(Hit.Taken.Recently()) },
                { "if you were damaged by a hit recently", Hit.Taken.Recently() },
                { "if you've taken no damage from hits recently", Not(Hit.Taken.Recently()) },
                { "if you've taken a savage hit recently", SavageHit.Taken.Recently()  },
                { "when you deal a critical strike", CriticalStrike.On() },
                {
                    "if you've crit in the past # seconds",
                    CriticalStrike.InPastXSeconds(Value)
                },
                { "if you've shattered an enemy recently", Shatter.Against(Enemy).Recently() },
                { "when you stun an enemy", Effect.Stun.On() },
                { "after spending # mana", SpendMana(Value).On() },
                { "if you have consumed a corpse recently", ConsumeCorpse.Recently() },
                // damage
                { "with weapons", Damage.With(Tags.Weapon) },
                { "weapon", Damage.With(Tags.Weapon) },
                { "with bows", Damage.With(Tags.Bow) },
                { "with swords", Damage.With(Tags.Sword) },
                { "with claws", Damage.With(Tags.Claw) },
                { "with daggers", Damage.With(Tags.Dagger) },
                { "with wands", Damage.With(Tags.Wand) },
                { "with axes", Damage.With(Tags.Axe) },
                { "with staves", Damage.With(Tags.Staff) },
                {
                    "with maces",
                    Or(Damage.With(Tags.Mace), Damage.With(Tags.Sceptre))
                },
                {
                    "with one handed melee weapons",
                    And(Damage.With(Tags.OneHandWeapon), Not(Damage.With(Tags.Ranged)))
                },
                {
                    "with two handed melee weapons",
                    And(Damage.With(Tags.TwoHandWeapon), Not(Damage.With(Tags.Ranged)))
                },
                { "with the main-hand weapon", Damage.With(ItemSlot.MainHand) },
                { "with main hand", Damage.With(ItemSlot.MainHand) },
                { "with off hand", Damage.With(ItemSlot.OffHand) },
                {
                    "melee attacks have",
                    Damage.With(Source.Attack).And(Keyword.Melee)
                },
                { "attacks have", Damage.With(Source.Attack) },
                { "from damage over time", Damage.With(Source.DamageOverTime) },
                { "melee", Damage.With(Keyword.Melee) },
                {
                    "with hits and ailments",
                    Or(Hit.On(), Ailment.Damaging.Any(Damage.With))
                },
                // action and damage combinations
                {
                    "on melee hit",
                    And(Hit.On(), Damage.With(Keyword.Melee))
                },
                {
                    "for each enemy hit by your attacks",
                    And(Hit.Against(Enemy).On(), Damage.With(Source.Attack))
                },
                {
                    "if you get a critical strike with a bow",
                    And(CriticalStrike.On(), Damage.With(Tags.Bow))
                },
                {
                    "if you get a critical strike with a staff",
                    And(CriticalStrike.On(), Damage.With(Tags.Staff))
                },
                {
                    "critical strikes with daggers have a",
                    And(CriticalStrike.On(), Damage.With(Tags.Dagger))
                },
                {
                    "on melee critical strike",
                    And(CriticalStrike.On(), Damage.With(Keyword.Melee))
                },
                // equipment
                { "while unarmed", Unarmed },
                { "while wielding a staff", MainHand.Has(Tags.Staff) },
                { "while dual wielding", OffHand.Has(Tags.Weapon) },
                { "while holding a shield", OffHand.Has(Tags.Shield) },
                {
                    "while dual wielding or holding a shield",
                    Or(OffHand.Has(Tags.Weapon), OffHand.Has(Tags.Shield))
                },
                { "with shields", OffHand.Has(Tags.Shield) },
                {
                    "from equipped shield",
                    And(BaseValueComesFrom(OffHand), OffHand.Has(Tags.Shield))
                },
                {
                    "with # corrupted items equipped",
                    Equipment.Count(e => e.IsCorrupted) >= Value
                },
                // stats
                { "when on low life", Or(Life.Reservation.Value >= 65, Life.IsLow) },
                { "when not on low life", And(Life.Reservation.Value < 65, Not(Life.IsLow)) },
                { "while no mana is reserved", Mana.Reservation.Value == 0 },
                { "while energy shield is full", EnergyShield.IsFull },
                { "while on full energy shield", EnergyShield.IsFull },
                { "while not on full energy shield", Not(EnergyShield.IsFull) },
                {
                    "if energy shield recharge has started recently",
                    EnergyShield.Recharge.StartedRecently
                },
                {
                    "while you have no ({ChargeTypeMatchers})",
                    Group.AsChargeType.Amount.Value == 0
                },
                {
                    "while (at maximum|on full) ({ChargeTypeMatchers})",
                    Group.AsChargeType.Amount.Value == Group.AsChargeType.Amount.Maximum.Value
                },
                {
                    "if you have # primordial jewels,",
                    PrimordialJewelsSocketed.Value >= Value
                },
                { "while you have ({FlagMatchers})", Group.As<FlagStat>().IsSet },
                { "during onslaught", Flag.Onslaught.IsSet },
                { "while phasing", Flag.Phasing.IsSet },
                // stats on enemy
                { "(against enemies )?that are on low life", Enemy.Stat(Life).IsLow },
                { "(against enemies )?that are on full life", Enemy.Stat(Life).IsFull },
                { "against rare and unique enemies", Enemy.IsRareOrUnique },
                // buffs
                { "while you have fortify", Buff.Fortify.IsOn(Self) },
                { "if you've taunted an enemy recently", Buff.Taunt.Action.Recently() },
                { "enemies you taunt( deal)?", And(For(Enemy), Buff.Taunt.IsOn(Enemy)) },
                {
                    "enemies you curse (take|have)",
                    And(For(Enemy), Buffs(Self, Enemy).With(Keyword.Curse).Any())
                },
                { "(against|from) blinded enemies", Buff.Blind.IsOn(Enemy) },
                { "from taunted enemies", Buff.Taunt.IsOn(Enemy) },
                {
                    "you and allies affected by your auras have",
                    Or(For(Self),
                        And(For(Ally), Buffs(target: Ally).With(Keyword.Aura).Any()))
                },
                {
                    "you and allies deal while affected by auras you cast",
                    Or(For(Self),
                        And(For(Ally), Buffs(target: Ally).With(Keyword.Aura).Any()))
                },
                // ailments
                { "while ({AilmentMatchers})", Group.AsAilment.IsOn(Self) },
                { "(against|from) ({AilmentMatchers}) enemies", Group.AsAilment.IsOn(Enemy) },
                {
                    "against frozen, shocked or ignited enemies",
                    Or(Ailment.Freeze.IsOn(Enemy), Ailment.Shock.IsOn(Enemy), Ailment.Ignite.IsOn(Enemy))
                },
                { "enemies which are ({AilmentMatchers})", Group.AsAilment.IsOn(Enemy) },
                {
                    "against enemies( that are)? affected by elemental ailments",
                    Ailment.Elemental.Any(a => a.IsOn(Enemy))
                },
                {
                    "against enemies( that are)? affected by no elemental ailments",
                    Not(Ailment.Elemental.Any(a => a.IsOn(Enemy)))
                },
                {
                    "poison you inflict with critical strikes deals",
                    And(Damage.With(Ailment.Poison), CriticalStrike.On())
                },
                {
                    "bleeding you inflict on maimed enemies deals",
                    And(Damage.With(Ailment.Bleed), Buff.Maim.IsOn(Enemy))
                },
                {
                    "with ({AilmentMatchers})", Damage.With(Group.AsDamagingAilment)
                },
                // ground effects
                { "while on consecrated ground", Ground.Consecrated.IsOn(Self) },
                // other effects
                { "against burning enemies", Fire.DamageOverTimeIsOn(Enemy) },
                // skills
                { "vaal( skill)?", With(Skills[Keyword.Vaal]) },
                { "with ({KeywordMatchers}) skills", With(Skills[Group.AsKeyword]) },
                { "({KeywordMatchers}) skills have", With(Skills[Group.AsKeyword]) },
                { "of ({KeywordMatchers}) skills", With(Skills[Group.AsKeyword]) },
                { "for ({KeywordMatchers})", With(Skills[Group.AsKeyword]) },
                { "with traps", With(Traps) },
                { "with mines", With(Mines) },
                { "with ({DamageTypeMatchers}) skills", With(Skills[Group.AsDamageType]) },
                {
                    "of totem skills that cast an aura",
                    With(Skills[Keyword.Totem, Keyword.Aura])
                },
                {
                    "({SkillMatchers})('|s)?( fires| has a| have a| has| deals|gain)?",
                    With(Group.AsSkill)
                },
                {
                    "skills (in|from) your ({ItemSlotMatchers})(can have| have)?",
                    With(Skills[Group.AsItemSlot])
                },
                { "if you've cast a spell recently", Skills[Keyword.Spell].Cast.Recently() },
                { "if you've attacked recently", Skills[Keyword.Attack].Cast.Recently() },
                { "if you've used a movement skill recently", Skills[Keyword.Movement].Cast.Recently() },
                { "if you've used a warcry recently", Skills[Keyword.Warcry].Cast.Recently() },
                {
                    "if you've used a ({DamageTypeMatchers}) skill in the past # seconds",
                    Skills[Group.AsDamageType].Cast.InPastXSeconds(Value)
                },
                // traps and mines
                {
                    "traps and mines (deal|have a)",
                    Or(With(Traps), With(Mines))
                },
                {
                    "from traps and mines",
                    Or(With(Traps), With(Mines))
                },
                { "for throwing traps", With(Traps) },
                { "if you detonated mines recently", Skill.DetonateMines.Cast.Recently() },
                {
                    "if you've placed a mine or thrown a trap recently",
                    Or(Traps.Cast.Recently(), Mines.Cast.Recently())
                },
                // totems
                { "totems", For(Totem) },
                { "totems (fire|gain|have)", With(Totems) },
                { "(spells cast|attacks used|skills used) by totems (have a|have)", With(Totems) },
                { "while you have a totem", Totems.Any(s => s.HasInstance) },
                { "if you've summoned a totem recently", Totems.Cast.Recently() },
                { "when you place a totem", Totems.Cast.On() },
                // minions
                { "minions", For(Minion) },
                { "minions (deal|have|gain)", For(Minion) },
                { "you and your minions have", For(Minion, Self) },
                { "golem", For(Minion.With(Keyword.Golem)) },
                { "golems have", For(Minion.With(Keyword.Golem)) },
                { "spectres have", For(Minion.From(Skill.RaiseSpectre)) },
                {
                    // technically this would be per minion summoned by that skill, but DPS will 
                    // only be calculated for a single minion anyway
                    "golems summoned in the past # seconds deal",
                    With(Golems.Where(s => s.Cast.InPastXSeconds(Value)))
                },
                {
                    "if you Summoned a golem in the past # seconds",
                    Golems.Cast.InPastXSeconds(Value)
                },
                // flasks
                { "while using a flask", Flask.IsAnyActive },
                // other
                { "while leeching", WhileLeeching },
                {
                    "with arrows that pierce",
                    And(With(Skills[Keyword.Bow]), Projectile.Pierces)
                },
                { "(you )?gain", Or() }, // may be left over at the end, does nothing
                // other enemy
                { "nearby enemies (have|deal|take)", And(For(Enemy), Enemy.IsNearby) },
                { "enemies near your totems (have|deal|take)", And(For(Enemy), Enemy.IsNearby) },
                // unique
                { "when your trap is triggered by an enemy", UniqueCondition("$1") },
                { "when your mine is detonated targeting an enemy", UniqueCondition("$1") },
                { "when you gain a ({ChargeTypeMatchers})", UniqueCondition("$1") },
                { "if you or your totems kill an enemy", UniqueCondition("$1") },
                {
                    "if you've (killed an enemy affected by your damage over time recently)",
                    UniqueCondition("Have you $1?")
                },
            };

        // with multiple values in the mod line, these apply to the closest value before them
        public ValueConversionMatcherCollection ValueConversionMatchers { get; } =
            new ValueConversionMatcherCollection
            {
                // action
                { "for each enemy you've killed recently", Kill.CountRecently },
                {
                    "per enemy killed by you or your totems recently",
                    Kill.CountRecently + Kill.By(Totem).CountRecently
                },
                { "for each hit you've blocked recently", Block.CountRecently },
                { "for each corpse consumed recently", ConsumeCorpse.CountRecently },
                // equipment
                { "for each type of golem you have summoned", Golems.Count(s => s.HasInstance) },
                {
                    "for each magic item you have equipped",
                    Equipment.Count(e => e.Has(FrameType.Magic))
                },
                // stats
                {
                    "per # ({StatMatchers})",
                    PerStat(stat: Group.AsStat, divideBy: Value)
                },
                {
                    "per # ({StatMatchers}) ceiled",
                    PerStatCeiled(stat: Group.AsStat, divideBy: Value)
                },
                { "per ({StatMatchers})", PerStat(stat: Group.AsStat) },
                { "per Level", PerLevel },
                // buffs
                {
                    "per buff on you",
                    Buffs(target: Self).ExceptFrom(Skill.BloodRage, Skill.MoltenShell).Count()
                },
                { "per curse on you", Buffs(target: Self).With(Keyword.Curse).Count() },
                {
                    "for each curse on that enemy,",
                    Buffs(target: Enemy).With(Keyword.Curse).Count()
                },
                // ailments
                { "for each poison on the enemy", Ailment.Poison.InstancesOn(Enemy).Value },
                { "per poison on enemy", Ailment.Poison.InstancesOn(Enemy).Value },
                // skills
                { "for each zombie you own", Skill.RaiseZombie.Instances.Value },
                // traps, mines, totems
                { "for each trap", Traps.CombinedInstances.Value },
                { "for each mine", Mines.CombinedInstances.Value },
                {
                    "for each trap and mine you have",
                    Traps.CombinedInstances.Value + Mines.CombinedInstances.Value
                },
                { "per totem", Totems.CombinedInstances.Value },
            };

        public StatManipulatorMatcherCollection StatManipulationMatchers { get; } =
            new StatManipulatorMatcherCollection
            {
                { "you and nearby allies( deal| have)?", s => s.AsAura },
                {
                    "auras you cast grant (.*) to you and allies",
                    s => s.AddTo(Skills[Keyword.Aura]), "$1"
                },
                {
                    "consecrated ground you create grant (.*) to you and allies",
                    s => s.AddTo(Ground.Consecrated), "$1"
                },
                {
                    "every # seconds, gain (.*) for # seconds",
                    s => Rotation(Values.First).Step(Values.Last, s.AsBuff), "$1"
                },
                // Keep whole mod line, take is part of the condition matcher
                { ".*enemies.* take", (DamageStat s) => s.Taken, "$0" },
                { "(chance to .*) for # seconds", s => s.ForXSeconds(Value).ChanceOn(Self), "$1" },
                { "for # seconds", s => s.ForXSeconds(Value).On(Self) },
            };

        public SpecialMatcherCollection SpecialMatchers { get; } = new SpecialMatcherCollection
        {
            {
                @"\+# to level of socketed support gems",
                IncreaseLevelBy(Value, onlySocketedGems: true, onlySupportGems: true)
            },
            {
                "primordial",
                BaseAdd, PrimordialJewelsSocketed, 1
            },
            {
                "grand spectrum",
                BaseAdd, GrandSpectrumJewelsSocketed, 1
            },
            { "ignore all movement penalties from armour", SetFlag, Flag.IgnoreMovementSpeedPenalties },
            {
                "life leech is based on your chaos damage instead", SetFlag,
                Life.Leech.BasedOn(Chaos)
            },
            // Keystones
            {
                // Point Blank
                "projectile attacks deal up to #% more damage to targets at the start of their movement, " +
                "dealing less damage to targets as the projectile travels farther",
                PercentMore, Damage, Value,
                // 0 to 10: Value; 10 to 35: Value to 0; 35 to 150: 0 to -Value
                LinearScale(Projectile.TravelDistance, (0, 1), (10, 1), (35, 0), (150, -1)),
                Damage.With(Source.Attack).And(Keyword.Projectile)
            },
            {
                // Elemental Equilibrium
                "enemies you hit with elemental damage temporarily get +#% resistance to those elements " +
                "and -#% resistance to other elements",
                ElementalEquilibrium().ToArray()
            },
            {
                // Necromantic Aegis
                "all bonuses from an equipped shield apply to your minions instead of you",
                (TotalOverride, OffHand.AppliesToMinions, 1, OffHand.Has(Tags.Shield)),
                (TotalOverride, OffHand.AppliesToSelf, 0, OffHand.Has(Tags.Shield))
            },
            {
                // Perfect Agony
                "modifiers to critical strike multiplier also apply to damage multiplier for " +
                "ailments from critical strikes at #% of their value",
                TotalOverride,
                Crit.Multiplier.ApplyModifiersTo(Crit.AilmentMultiplier, 
                    percentOfTheirValue: Value),
                1
            },
            // Ascendancies
            {
                "your hits permanently intimidate enemies that are on full life",
                SetFlag, Buff.Intimidate.On(Enemy),
                UniqueCondition("on Hit against Enemies that are on Full Life")
            },
            { "movement skills cost no mana", Zero, Skills[Keyword.Movement].Cost },
            {
                "your offering skills also affect you",
                SetFlag, Skills[Keyword.Offering].AddTargetToStats(Self)
            },
            {
                "far shot",
                PercentMore, Damage, 30,
                LinearScale(Projectile.TravelDistance, (0, 0), (150, 1)),
                Damage.With(Source.Attack).And(Keyword.Projectile)
            },
            {
                "projectiles gain damage as they travel further, dealing up to #% increased damage to targets",
                PercentIncrease, Damage, Value,
                LinearScale(Projectile.TravelDistance, (0, 0), (150, 1)),
                Damage.With(Keyword.Projectile)
            },
            {
                "your critical strikes with attacks maim enemies",
                SetFlag, Buff.Maim.On(Enemy),
                And(Damage.With(Source.Attack), CriticalStrike.Against(Enemy).On())
            },
            {
                "gain #% of maximum mana as extra maximum energy shield",
                BaseAdd, Mana.ConvertTo(EnergyShield), Value
            },
            {
                "critical strikes ignore enemy monster elemental resistance",
                SetFlag, Damage.IgnoreResistance(Elemental), CriticalStrike.Against(Enemy).On()
            },
            {
                "non-critical strikes penetrate #% of enemy elemental resistances",
                BaseAdd, Damage.PenetrationOf(Elemental), NonCriticalStrike.Against(Enemy).On()
            },
            { "totems are immune to fire damage", TotalOverride, Fire.Resistance, 100, For(Totem) },
            {
                "totems have #% of your armour",
                BaseAdd, Armour, Value.AsPercentage * Self.Stat(Armour).Value, For(Totem)
            },
            // Elementalist
            {
                "#% increased damage of each damage type for which you have a matching golem",
                LiegeOfThePrimordialDamage().ToArray()
            },
            {
                "your elemental golems are immune to elemental damage",
                TotalOverride, Elemental.Resistance, 100,
                For(Minion.With(Keyword.Golem, Elemental))
            },
            {
                "every # seconds: " +
                "gain chilling conflux for # seconds " +
                "gain shocking conflux for # seconds " +
                "gain igniting conflux for # seconds " +
                "gain chilling, shocking and igniting conflux for # seconds",
                SetFlag, Rotation(Values[0])
                    .Step(Values[1], Buff.Conflux.Chilling)
                    .Step(Values[2], Buff.Conflux.Shocking)
                    .Step(Values[3], Buff.Conflux.Igniting)
                    .Step(Values[4], Buff.Conflux.Chilling, Buff.Conflux.Igniting, Buff.Conflux.Shocking)
            },
            {
                "for each element you've been hit by damage of recently, " +
                "#% increased damage of that element",
                ParagonOfCalamity(PercentIncrease, Damage, Value).ToArray()
            },
            {
                "for each element you've been hit by damage of recently, " +
                "#% reduced damage taken of that element",
                ParagonOfCalamity(PercentReduction, Damage.Taken, Value).ToArray()
            },
            // Guardian
            {
                "grants armour equal to #% of your reserved life to you and nearby allies",
                BaseAdd, Armour.AsAura, Value.AsPercentage * Life.Value * Life.Reservation.Value
            },
            {
                "grants maximum energy shield equal to #% of your reserved mana to you and nearby allies",
                BaseAdd, EnergyShield.AsAura,
                Value.AsPercentage * Mana.Value * Mana.Reservation.Value
            },
            { "warcries cost no mana", Zero, Skills[Keyword.Warcry].Cost },
            {
                "using warcries is instant",
                TotalOverride, Skills[Keyword.Warcry].Speed, double.PositiveInfinity
            },
            // Juggernaut
            {
                "you cannot be slowed to below base speed",
                TotalOverride, AnimationSpeed.Minimum, 1
            },
            {
                "movement speed cannot be modified to below base value",
                TotalOverride, MovementSpeed.Minimum, 1
            },
            {
                "armour received from body armour is doubled",
                PercentMore, Armour, 100, BaseValueComesFrom(Equipment[ItemSlot.BodyArmour])
            },
        };

        private static IEnumerable<(IFormProvider form, IStatProvider stat, ValueProvider value,
            IConditionProvider condition)> ElementalEquilibrium()
        {
            foreach (var type in ElementalDamageTypes)
            {
                yield return (BaseAdd, Enemy.Stat(type.Resistance), Values[0],
                    Enemy.HitByInPastXSeconds(type, 5));
                yield return (BaseSubtract, Enemy.Stat(type.Resistance), Values[1],
                    And(Not(Enemy.HitByInPastXSeconds(type, 5)),
                        Enemy.HitByInPastXSeconds(Elemental.Except(type), 5)));
            }
        }

        private static IEnumerable<(IFormProvider form, IStatProvider stat, ValueProvider value,
            IConditionProvider condition)> LiegeOfThePrimordialDamage()
        {
            foreach (var type in AllDamageTypes)
            {
                yield return (PercentIncrease, type.Damage, Value, 
                    Golems[type].Single.HasInstance);
            }
        }

        private static IEnumerable<(IFormProvider form, IStatProvider stat, ValueProvider value,
            IConditionProvider condition)> ParagonOfCalamity(IFormProvider form, 
            IStatProvider stat, ValueProvider value)
        {
            foreach (var type in ElementalDamageTypes)
            {
                yield return (form, stat, value,  
                    And(type.Damage.With(), Self.HitByRecently(type)));
            }
        }

        // Properties of items are matched using this collection
        public StatMatcherCollection PropertyMatchers { get; } = new StatMatcherCollection
        {
            { "quality" }, // do nothing with it
            { "attacks per second", AttackSpeed },
            { "cast time", CastSpeed, TimeToPerSecond },
            { "elemental damage", Fire.Damage, MatchHas(ValueColoring.Fire) },
            { "damage effectiveness", Skills.DamageEffectiveness }
        };
    }
}