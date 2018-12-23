using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using PoESkillTree.GameModel.Items;

namespace PoESkillTree.GameModel.Modifiers
{
    public static class ModifierLocalityTester
    {
        private const string Increase = "#% (increased|reduced) ";
        private const string BaseAdd = @"(\+|-)?#%? to ";
        private const string Chance = @"\+?#% chance to ";
        private static readonly string OptionalChance = $"({Chance})?";

        private static readonly IReadOnlyList<Regex> LocalUnconditionally = new RegexCollection
        {
            "level requirement (increased|reduced) by #",
            BaseAdd + "quality",
            @"(has no|no|#% increased|#% reduced|\+#|-#) (attribute|intelligence|dexterity|strength) requirements?",
        }.ToList();

        private static readonly IReadOnlyList<Regex> WeaponLocal = new RegexCollection
        {
            "adds # to # (physical|fire|cold|lightning|chaos) damage( in (main|off) hand)?",
            "no physical damage",
            Increase + "(physical damage|attack speed|critical strike chance|weapon range)",
            OptionalChance + "impale enemies on hit with attacks",
            OptionalChance + "(blind enemies|poison|causes? bleeding|maim) on hit",
            OptionalChance + "cause (poison|bleeding) on critical strike",
            "always poison on hit",
            "#% of physical attack damage leched as (life|mana|life and mana)",
            BaseAdd + "accuracy rating",
            "hits can't be evaded",
            "# (life|mana) gained for each enemy hit by attacks",
            "# life and mana gained for each enemy hit",
            ".*(attacks|hits|on hit|inflicted|critical strikes) with this weapon.*",
            ".*hit by this weapon.*",
            Increase + "enemy stun threshold with this weapon",
        }.ToList();

        private static readonly IReadOnlyList<Regex> ArmourLocal = new RegexCollection
        {
            $"({Increase}|{BaseAdd})" +
            "(armour|evasion|energy shield|armour and evasion|armour and energy shield|evasion and energy shield|armour, evasion and energy Shield)",
            "item has no level requirement and energy shield",
        }.ToList();

        private static readonly IReadOnlyList<Regex> ShieldLocal = new RegexCollection
        {
            Chance + "block",
            "cannot block",
            "no chance to block",
        }.ToList();

        private static readonly IReadOnlyList<Regex> FlaskLocal = new RegexCollection
        {
            Increase + "(recovery rate|(life|mana|amount) recovered|effect|duration|charges used)",
            BaseAdd + "maximum charges",
            Increase + "(charge recovery|recovery when on low life)",
            OptionalChance + "gain a flask charge when you deal a critical strike",
            "gains no charges during effect of any (soul ripper|overflowing chalice) flask",
            "recharges # charges? when you ((deal|take) a critical strike|consume an ignited corpse)",
            "instant recovery(when on low life)?",
            "#% of recovery applied instantly",
            "grants #% of life recovery to minions",
        }.ToList();

        private static readonly Regex NumberRegex = new Regex(@"\d+(\.\d+)?");

        public static bool IsLocal(string modifier, Tags itemTags)
        {
            var canonicalModifier = NumberRegex.Replace(modifier, "#").ToLowerInvariant();
            if (LocalUnconditionally.Any(r => r.IsMatch(canonicalModifier)))
                return true;
            if (itemTags.HasFlag(Tags.Weapon) && WeaponLocal.Any(r => r.IsMatch(canonicalModifier)))
                return true;
            if (itemTags.HasFlag(Tags.Armour) && ArmourLocal.Any(r => r.IsMatch(canonicalModifier)))
                return true;
            if (itemTags.HasFlag(Tags.Shield) && ShieldLocal.Any(r => r.IsMatch(canonicalModifier)))
                return true;
            if (itemTags.HasFlag(Tags.Flask) && FlaskLocal.Any(r => r.IsMatch(canonicalModifier)))
                return true;
            return false;
        }

        private class RegexCollection : IEnumerable<Regex>
        {
            private readonly List<Regex> _regexes = new List<Regex>();

            public void Add([RegexPattern] string pattern)
                => _regexes.Add(new Regex($"^{pattern}$", RegexOptions.Multiline | RegexOptions.IgnoreCase));

            public IEnumerator<Regex> GetEnumerator() => _regexes.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}