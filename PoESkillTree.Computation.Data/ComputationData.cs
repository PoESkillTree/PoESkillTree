using System.Collections.Generic;
using PoESkillTree.Common.Model.Items.Enums;
using static PoESkillTree.Computation.Providers.BuffProviders;
using static PoESkillTree.Computation.Providers.BuffTargetProviders;
using static PoESkillTree.Computation.Providers.ConditionProviders;
using static PoESkillTree.Computation.Providers.ConverterProviders;
using static PoESkillTree.Computation.Providers.DamageSourceProviders;
using static PoESkillTree.Computation.Providers.DamageTypeProviders;
using static PoESkillTree.Computation.Providers.EquipmentProviders;
using static PoESkillTree.Computation.Providers.FormProviders;
using static PoESkillTree.Computation.Providers.GemModifierProviders;
using static PoESkillTree.Computation.Providers.GroupConverters;
using static PoESkillTree.Computation.Providers.KeywordProviders;
using static PoESkillTree.Computation.Providers.MatchConditionProviders;
using static PoESkillTree.Computation.Providers.MultiplierProviders;
using static PoESkillTree.Computation.Providers.StatProviders;
using static PoESkillTree.Computation.Providers.ValueProviders;

namespace PoESkillTree.Computation.Data
{
    // most local stats are already added to properties and don't need to be handled here
    // (those stats are handled elsewhere and don't even get here)

    /* handled mod lines (also handled: GivenStats):
    #% increased Movement Speed
    +#% to Fire Resistance
    Curse Enemies with level 10 Flammability on Hit
    +# to Level of Socketed Support Gems
    Causes Bleeding on Hitd
    #% increased Evasion Rating and Armour
    #% reduced MovementSpeed
    Adds # to # Cold Damage to Attacks
    +#% to all Elemental Resistances
    +#% Chance to Block
    +#% to maximum Cold Resistance
    #% increased Elemental Damage with Weapons
    +# to maximum Life
    -#% to maximum Block Chance
    +#% to all maximum Resistances while you have no Endurance Charges
    You have Onslaught while at maximum Endurance Charges
    -# to Maximum Endurance Charges
    +# to Accuracy Rating
    +# to Evasion Rating
    #% increased Attack Damage per # Evasion Rating
     */

    /* not handled: (no useful usage)
    nothing yet
     */

    public class ComputationData : IComputationData
    {
        // stat lines that are always given independent of gear/tree/gems
        // see http://pathofexile.gamepedia.com/Character
        public IReadOnlyList<string> GivenStats { get; } = new[]
        {
            // base stats
            "1.75% of Mana Regenerated per second",
            "20% of Energy Shield Recharged per second",
            "+53 to Evasion Rating",
            "+150% to Critical Strike Multiplier",
            // while Dual Wielding
            "10% more Attack Speed while Dual Wielding",
            "15% additional Block Chance while Dual Wielding",
            "20% more Attack Physical Damage while Dual Wielding",
            // charges
            "+3 to Maximum Endurance Charges",
            "4% additional Physical Damage Reduction per Endurance Charge",
            "+4% to all Elemental Resistances per Endurance Charge",
            "+3 to Maximum Frenzy Charges",
            "4% increased Attack Speed per Frenzy Charge",
            "4% increased Cast Speed per Frenzy Charge",
            "4% more Damage per Frenzy Charge",
            "+3 to Maximum Power Charges",
            "50% increased Critical Strike Chance per Power Charge",
            // Rampage
            //[...]
            // maxima
            //[...]
            // level based
            //[...]
            // attribute conversions
            "+5 to maximum Life per 10 Strength",
            "2% increased Melee Physical Damage per 10 Strength",
            "+20 to Accuracy Rating per 10 Dexterity",
            "2% increased Evasion Rating per 10 Dexterity",
            "+5 Mana per 10 Intelligence",
            "2% increased maximum Energy Shield per 10 Intelligence",
        };

        // Each entry in the matchers arrays is one matcher.
        // The first value of a matcher is the (case-insensitive) regex that has to be matched.
        // The other values specify the effects should a stat line match the regex
        // (except for 'matchesIf', which is another condition that must be satisfied for the entry 
        // to be considered a match).
        // The match with the longest regex from each list is taken (first match if multiple of same length).
        // Each substring of the mod line can only be matched once.

        public FormMatcherCollection FormMatchers { get; } = new FormMatcherCollection
        {
            { "^#% increased", PercentIncrease },
            { "^#% more", PercentMore },
            { @"^\+#%? to", BaseAdd },
            { "^-#%? to", BaseSubtract },
            { "#% additional", BaseAdd },
        };

        // serves as match for both FormMatchers and ModifiedStatMatchers
        // second value is the form, third the stat
        public FormMatcherCollection FormAndStatMatchers { get; } = new FormMatcherCollection
        {
            {
                @"^adds # to # (\w+) damage to attacks", ValueDependent(MinBaseAdd, MaxBaseAdd),
                Damage(source: Attack, type: Group(0).AsDamageType)
            },
            {
                // Not sure how this could be done, but ({RegenTypeMatchers}) should be replaced by
                // all Regex strings from ModifiedStatMatchers joined with "|" characters.
                // If there is a match, the group match is again matched with ModifiedStatMatchers itself
                // and EvaluatedGroup returns the matched entry, which is cast to the resulting stat.
                "^#% of ({RegenTypeMatchers}) regenerated per second", PercentRegen,
                Regen(regenType: EvaluatedGroup(0).AsStat)
            },
            { "^#% of energy shield recharged per second", PercentRegen, Recharge(regenType: EnergyShield) }
        };

        // second value is the regenerated stat
        public StatMatcherCollection RegenTypeMatchers { get; } = new StatMatcherCollection
        {
            { "mana", Mana }
        };

        // contains only the stat matcher itself if the stat stands on its own and doesn't interact with 
        // anything (e.g. movement speed)
        public StatMatcherCollection StatMatchers { get; } = new StatMatcherCollection
        {
            // attributes
            { "strength", Strength },
            { "dexterity", Dexterity },
            { "intelligence", Intelligence },
            // offense
            // - damage
            { "damage", Damage() },
            { "attack damage", Damage(source: Attack) },
            { "physical damage", Damage(type: Physical) },
            { "fire damage", Damage(type: Fire) },
            { "elemental damage", Damage(type: Elemental) },
            { "melee physical damage", Damage(type: Physical, keyword: MeleeKeyword) },
            { "attack physical damage", Damage(type: Physical, source: Attack) },
            // - crit
            { "critical strike multiplier", CritMultiplier },
            { "critical strike chance", CritChance },
            // - attacks
            { "attack speed", AttackSpeed },
            { "weapon range" },
            { "accuracy rating", Accuracy },
            // - casts
            { "cast speed", CastSpeed },
            // - other
            { "projectile speed" },
            // defense
            { "life", Life },
            { "maximum life", Life },
            { "mana", Mana },
            { "armour", Armour },
            { "evasion", Evasion },
            { "evasion rating", Evasion },
            { "energy shield", EnergyShield },
            { "maximum energy shield", EnergyShield },
            { "evasion rating and armour", ApplyOnce(Armour, Evasion) },
            // - resistances
            { "fire resistance", Resistance(type: Fire) },
            { "all elemental resistances", Resistance(type: Elemental) },
            { "maximum cold resistance", Resistance(type: Cold).Maximum },
            { "all maximum resistances", Resistance(type: Elemental.And(Chaos)).Maximum },
            // - block
            { "chance to block", BlockChance },
            { "block chance", BlockChance },
            { "maximum block chance", BlockChance.Maximum },
            // - other
            { "physical damage reduction", PhysicalDamageReduction },
            // charges
            { "endurance charges?", EnduranceCharges },
            { "maximum endurance charges?", EnduranceCharges.Maximum },
            { "maximum frenzy charges?", FrenzyCharges.Maximum },
            { "maximum power charges?", PowerCharges.Maximum },
            // other
            { "movement speed" },
        };

        public BuffMatcherCollection BuffMatchers { get; } = new BuffMatcherCollection
        {
            { "you have onslaught", Buff(Onslaught, target: Self) },
        };

        public ConditionMatcherCollection ConditionMatchers { get; } = new ConditionMatcherCollection
        {
            { "with weapons", DamageCondition(hasKeyword: AttackKeyword, isUnarmed: false) },
            { "while you have no endurance charges", EnduranceCharges.ValueEquals(0) },
            { "while at maximum endurance charges", EnduranceCharges.ValueEquals(EnduranceCharges.Maximum) },
            { "while dual wielding", And(MainHand.Has(Tags.Weapon), OffHand.Has(Tags.Weapon)) }
        };

        public MultiplierMatcherCollection MultiplierMatchers { get; } = new MultiplierMatcherCollection
        {
            { "per # ({StatMatchers})", PerStat(stat: EvaluatedGroup(0).AsStat, divideBy: Value(0)) },
            { "per ({StatMatchers})", PerStat(stat: EvaluatedGroup(0).AsStat) },
        };

        // if a line matches one of these, don't apply other matchers
        public SpecialMatcherCollection SpecialMatchers { get; } = new SpecialMatcherCollection
        {
            {
                @"curse enemies with level # (\w+) on hit",
                Curse(skill: Group(0).AsSkill, level: Value(0), target: Enemy), Hit
            },
            {
                @"\+# to level of socketed support gems",
                IncreaseLevelBy(Value(0), onlySocketedGems: true, onlySupportGems: true)
            },
            {
                "causes bleeding on hit",
                BaseOverride, ChanceToBleed, FixedValue(100), WeaponLocalHit
            }
        };

        public StatMatcherCollection PropertyMatchers { get; } = new StatMatcherCollection
        {
            { "quality" }, // do nothing with it
            { "attacks per second", AttackSpeed }, // this or cast time below has to be converted
            { "cast time", CastSpeed, TimeToPerSecond },
            { "elemental damage", Damage(type: Fire), MatchHas(ValueColoring.Fire) },
            { "damage effectiveness", DamageEffectiveness }
        };
    }
}