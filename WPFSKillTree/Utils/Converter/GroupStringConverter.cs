using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;
using POESKillTree.Localization;
using POESKillTree.ViewModels;
using Attribute = POESKillTree.ViewModels.Attribute;

namespace POESKillTree.Utils.Converter
{
	using CSharpGlobalCode.GlobalCode_ExperimentalCode;
    [ValueConversion(typeof (string), typeof (string))]
    //list view sorter here
    public class GroupStringConverter : IValueConverter, IComparer
    {
        public Dictionary<string, AttributeGroup> AttributeGroups = new Dictionary<string, AttributeGroup>();
        private IList<string[]> CustomGroups;
        private static readonly string Keystone = L10n.Message("Keystone");
        private static readonly string Weapon = L10n.Message("Weapon");
        private static readonly string Charges = L10n.Message("Charges");
        private static readonly string Minion = L10n.Message("Minion");
        private static readonly string Trap = L10n.Message("Trap");
        private static readonly string Totem = L10n.Message("Totem");
        private static readonly string Curse = L10n.Message("Curse");
        private static readonly string Aura = L10n.Message("Aura");
        private static readonly string CriticalStrike = L10n.Message("Critical Strike");
        private static readonly string Shield = L10n.Message("Shield");
        private static readonly string Block = L10n.Message("Block");
        private static readonly string General = L10n.Message("General");
        private static readonly string Defense = L10n.Message("Defense");
        private static readonly string Spell = L10n.Message("Spell");
        private static readonly string Flasks = L10n.Message("Flasks");
        private static readonly string CoreAttributes = L10n.Message("Core Attributes");
        private static readonly string MiscLabel = L10n.Message("Everything Else");
        private static readonly string DecimalRegex = "\\d+(\\.\\d*)?";
        private static readonly IReadOnlyList<string[]> DefaultGroups = new List<string[]>
        {
            new[] {"Share Endurance, Frenzy and Power Charges with nearby party members", Keystone},
            new[] {"Critical Strike Chance with Claws", CriticalStrike},
            new[] {"with Claws", Weapon},
            new[] {"Endurance Charge", Charges},
            new[] {"Frenzy Charge", Charges},
            new[] {"Power Charge", Charges},
            new[] {"Chance to Dodge", Keystone},
            new[] {"Spell Damage when on Low Life", Keystone},
            new[] {"Cold Damage Converted to Fire Damage", Keystone},
            new[] {"Lightning Damage Converted to Fire Damage", Keystone},
            new[] {"Physical Damage Converted to Fire Damage", Keystone},
            new[] {"Deal no Non-Fire Damage", Keystone},
            new[] {"All bonuses from an equipped Shield apply to your Minions instead of you", Keystone},
            new[] {"additional totem", Keystone},
            new[] {"Cannot be Stunned", Keystone},
            new[] {"Cannot Evade enemy Attacks", Keystone},
            new[] {"Spend Energy Shield before Mana for Skill Costs", Keystone},
            new[] {"Energy Shield protects Mana instead of Life", Keystone},
            new[] {"Converts all Evasion Rating to Armour. Dexterity provides no bonus to Evasion Rating", Keystone},
            new[] {"Doubles chance to Evade Projectile Attacks", Keystone},
            new[] {"Enemies you hit with Elemental Damage temporarily get", Keystone},
            new[] {"Life Leech applies instantly", Keystone},
            new[] {"Life Leech is Applied to Energy Shield instead", Keystone},
            new[] {"Life Regeneration is applied to Energy Shield instead", Keystone},
            new[] {"No Critical Strike Multiplier", Keystone},
            new[] {"Immune to Chaos Damage", Keystone},
            new[] {"Minions explode when reduced to low life", Keystone},
            new[] {"Never deal Critical Strikes", Keystone},
            new[] {"Projectile Attacks deal up to", Keystone},
            new[] {"Removes all mana", Keystone},
            new[] {"Share Endurance", Keystone},
            new[] {"The increase to Physical Damage from Strength applies to Projectile Attacks as well as Melee Attacks", Keystone},
            new[] {"Damage is taken from Mana before Life", Keystone},
            new[] {"You can't deal Damage with your Skills yourself", Keystone},
            new[] {"Your hits can't be Evaded", Keystone},
            new[] {"less chance to Evade Melee Attacks", Keystone},
            new[] {"more chance to Evade Projectile Attacks", Keystone},
            new[] {"Maximum number of Spectres", Minion},
            new[] {"Maximum number of Zombies", Minion},
            new[] {"Maximum number of Skeletons", Minion},
            new[] {"Minions deal", Minion},
            new[] {"Minions have", Minion},
            new[] {"Minions Leech", Minion},
            new[] {"Minions Regenerate", Minion},
            new[] {"Mine Damage", Trap},
            new[] {"Trap Damage", Trap},
            new[] {"Trap Duration", Trap},
            new[] {"Trap Trigger Radius", Trap},
            new[] {"Mine Duration", Trap},
            new[] {"Mine Laying Speed", Trap},
            new[] {"Trap Throwing Speed", Trap},
            new[] {"Can set up to", Trap},
            new[] {"Can have up to", Trap},
            new[] {"Detonating Mines is Instant", Trap},
            new[] {"Mine Damage Penetrates", Trap},
            new[] {"Mines cannot be Damaged", Trap},
            new[] {"Mine Detonation", Trap},
            new[] {"Trap Damage Penetrates", Trap},
            new[] {"Traps cannot be Damaged", Trap},
            new[] {"throwing Traps", Trap},
            new[] {"Totem Duration", Totem},
            new[] {"Casting Speed for Summoning Totems", Totem},
            new[] {"Totem Life", Totem},
            new[] {"Totem Damage", Totem},
            new[] {"Attacks used by Totems", Totem},
            new[] {"Spells Cast by Totems", Totem},
            new[] {"Totems gain", Totem},
            new[] {"Totems have", Totem},
            new[] {"Totem Placement", Totem},
            new[] {"Radius of Curse", Curse},
            new[] {"Curse Duration", Curse},
            new[] {"Effect of your Curses", Curse},
            new[] {"Radius of Curses", Curse},
            new[] {"Cast Speed for Curses", Curse},
            new[] {"Enemies can have 1 additional Curse", Curse},
            new[] {"Mana Reserved", Aura},
            new[] {"Aura", Aura},
            new[] {"Auras", Aura},
            new[] {"Weapon Critical Strike Chance", CriticalStrike},
            new[] {"increased Critical Strike Chance", CriticalStrike},
            new[] {"to Critical Strike Multiplier", CriticalStrike},
            new[] {"Global Critical Strike", CriticalStrike},
            new[] {"Critical Strikes with Daggers Poison the enemy", CriticalStrike},
            new[] {"Knocks Back enemies if you get a Critical Strike", CriticalStrike},
            new[] {"to Melee Critical Strike Multiplier", CriticalStrike},
            new[] {"increased Melee Critical Strike Chance", CriticalStrike},
            new[] {"Elemental Resistances while holding a Shield", Shield},
            new[] {"Chance to Block Spells with Shields", Block},
            new[] {"Armour from equipped Shield", Shield},
            new[] {"additional Block Chance while Dual Wielding or Holding a shield", Block},
            new[] {"Chance to Block with Shields", Block},
            new[] {"Block and Stun Recovery", Block},
            new[] {"Energy Shield from equipped Shield", Shield},
            new[] {"Block Recovery", Block},
            new[] {"Defences from equipped Shield", Shield},
            new[] {"Damage Penetrates", General}, //needs to be here to pull into the correct tab.
            new[] {"reduced Extra Damage from Critical Strikes", Defense},
            new[] {"Armour", Defense},
            new[] {"Fortify", Defense},
            new[] {"Damage with Weapons Penetrate", Weapon}, //needs to be before resistances
            new[] {"all Elemental Resistances", Defense},
            new[] {"Chaos Resistance", Defense},
            new[] {"Evasion Rating", Defense},
            new[] {"Cold Resistance", Defense},
            new[] {"Lightning Resistance", Defense},
            new[] {"maximum Mana", Defense},
            new[] {"maximum Energy Shield", Defense},
            new[] {"Fire Resistance", Defense},
            new[] {"maximum Life", Defense},
            new[] {"Light Radius", Defense},
            new[] {"Evasion Rating and Armour", Defense},
            new[] {"Energy Shield Recharge", Defense},
            new[] {"Life Regenerated", Defense},
            new[] {"Melee Physical Damage taken reflected to Attacker", Defense},
            new[] {"Avoid Elemental Status Ailments", Defense},
            new[] {"Damage taken Gained as Mana when Hit", Defense},
            new[] {"Avoid being Chilled", Defense},
            new[] {"Avoid being Frozen", Defense},
            new[] {"Avoid being Ignited", Defense},
            new[] {"Avoid being Shocked", Defense},
            new[] {"Avoid being Stunned", Defense},
            new[] {"increased Stun Recovery", Defense},
            new[] {"Mana Regeneration Rate", Defense},
            new[] {"maximum Mana", Defense},
            new[] {"Armour", Defense},
            new[] {"Avoid interruption from Stuns while Casting", Defense},
            new[] {"Movement Speed", Defense},
            new[] {"Enemies Cannot Leech Life From You", Defense},
            new[] {"Enemies Cannot Leech Mana From You", Defense},
            new[] {"Ignore all Movement Penalties", Defense},
            new[] {"Physical Damage Reduction", Defense},
            new[] {"Poison on Hit", General},
            new[] {"Hits that Stun Enemies have Culling Strike", General},
            new[] {"increased Damage against Frozen, Shocked or Ignited Enemies", General},
            new[] {"Shock Duration on enemies", General},
            new[] {"Radius of Area Skills", General},
            new[] {"chance to Ignite", General},
            new[] {"chance to Shock", General},
            new[] {"Mana Gained on Kill", General},
            new[] {"Life gained on General", General},
            new[] {"Burning Damage", General},
            new[] {"Projectile Damage", General},
            new[] {"Knock enemies Back on hit", General},
            new[] {"chance to Freeze", General},
            new[] {"Projectile Speed", General},
            new[] {"Projectiles Piercing", General},
            new[] {"Ignite Duration on enemies", General},
            new[] {"Knockback Distance", General},
            new[] {"Mana Cost of Skills", General},
            new[] {"Chill Duration on enemies", General},
            new[] {"Freeze Duration on enemies", General},
            new[] {"Damage over Time", General},
            new[] {"Chaos Damage", General},
            new[] {"Status Ailments", General},
            new[] {"increased Damage against Enemies", General},
            new[] {"Enemies Become Chilled as they Unfreeze", General},
            new[] {"Skill Effect Duration", General},
            new[] {"Life Gained on Kill", General},
            new[] {"Area Damage", General},
            new[] {"Stun Threshold", General},
            new[] {"Stun Duration", General},
            new[] {"increased Damage against Enemies on Low Life", General},
            new[] {"chance to gain Onslaught", General},
            new[] {"Accuracy Rating", Weapon},
            new[] {"gained for each enemy hit by your Attacks", Weapon},
            new[] {"Melee Weapon and Unarmed range", Weapon},
            new[] {"chance to cause Bleeding", Weapon},
            new[] {"Wand Physical Damage", Weapon},
            new[] {"Attack Speed", Weapon},
            new[] {"Melee Damage", Weapon},
            new[] {"Block Chance With Staves", Block},
            new[] {"Attack Damage", Weapon},
            new[] {"with Daggers", Weapon},
            new[] {"Arrow Speed", Weapon},
            new[] {"Cast Speed while Dual Wielding", Weapon},
            new[] {"Physical Damage with Staves", Weapon},
            new[] {"with Axes", Weapon},
            new[] {"Physical Weapon Damage while Dual Wielding", Weapon},
            new[] {"with One Handed Melee Weapons", Weapon},
            new[] {"with Two Handed Melee Weapons", Weapon},
            new[] {"with Maces", Weapon},
            new[] {"with Bows", Weapon},
            new[] {"Melee Physical Damage", Weapon},
            new[] {"with Swords", Weapon},
            new[] {"with Wands", Weapon},
            new[] {"Cold Damage with Weapons", Weapon},
            new[] {"Fire Damage with Weapons", Weapon},
            new[] {"Lightning Damage with Weapons", Weapon},
            new[] {"Elemental Damage with Weapons", Weapon},
            new[] {"Physical Damage with Wands", Weapon},
            new[] {"Damage with Wands", Weapon},
            new[] {"Damage with Weapons", Weapon},
            new[] {"Spell Damage", Spell},
            new[] {"Elemental Damage with Spells", Spell},
            new[] {"enemy chance to Block Sword Attacks", Block},
            new[] {"additional Block Chance while Dual Wielding", Block},
            new[] {"mana gained when you Block", Block},
            new[] {"Leeched", General},
            new[] {"increased Physical Damage", General},
            new[] {"Elemental Damage", General},
            new[] {"Jewel Socket", General},
            new[] {"Cast Speed", Spell},
            new[] {"Cold Damage", General},
            new[] {"Fire Damage", General},
            new[] {"Lightning Damage", General},
            new[] {"Damage while Leeching", General},
            new[] {"Damage with Poison", General},
            new[] {"Flask", Flasks},
            new[] {"Flasks", Flasks},
            new[] {"Strength", CoreAttributes},
            new[] {"Intelligence", CoreAttributes},
            new[] {"Dexterity", CoreAttributes},
        };

        private static readonly Regex NumberRegex = new Regex(@"[0-9]*\.?[0-9]+");
        private static readonly Dictionary<string, string> AttributeToDefaultGroup = new Dictionary<string, string>();

        public GroupStringConverter()
        {
            CustomGroups = new List<string[]>();
            foreach (var group in DefaultGroups)
            {
                if (!AttributeGroups.ContainsKey(group[1]))
                {
                    AttributeGroups.Add(group[1], new AttributeGroup(group[1]));
                }
            }
            foreach (var group in CustomGroups)
            {
                if (!AttributeGroups.ContainsKey(group[1]))
                {
                    AttributeGroups.Add(group[1], new AttributeGroup("Custom: "+group[1]));
                }
            }
            AttributeGroups.Add(MiscLabel, new AttributeGroup(MiscLabel));
        }

        public void ResetGroups(IList<string[]> newgroups)
        {
            CustomGroups = newgroups;

            AttributeGroups = new Dictionary<string, AttributeGroup>();
            foreach (var group in DefaultGroups)
            {
                if (!AttributeGroups.ContainsKey(group[1]))
                {
                    AttributeGroups.Add(group[1], new AttributeGroup(group[1]));
                }
            }
            foreach (var group in CustomGroups)
            {
                if (!AttributeGroups.ContainsKey(group[1]))
                {
                    AttributeGroups.Add(group[1], new AttributeGroup("Custom: " + group[1]));
                }
            }
            AttributeGroups.Add(MiscLabel, new AttributeGroup(MiscLabel));
        }

        public void AddGroup(string groupname, string[] attributes)
        {
            if (!AttributeGroups.ContainsKey(groupname))
            {
                AttributeGroups.Add(groupname, new AttributeGroup("Custom: "+groupname));
            }
            foreach (string attr in attributes)
            {
                AddAttributeToGroup(attr, groupname);
            }
        }

        private void AddAttributeToGroup(string attribute, string groupname)
        {
            //Remove it from any existing custom groups first
            RemoveFromGroup(new string[] { attribute });
            CustomGroups.Insert(0, new string[] { attribute, groupname });
        }

        public void RemoveFromGroup(string[] attributes)
        {
            List<string[]> linesToRemove = new List<string[]>();
            foreach (string attr in attributes)
            {
                foreach (var gp in CustomGroups)
                {
                    if (NumberRegex.Replace(attr.ToLowerInvariant(), "") == NumberRegex.Replace(gp[0].ToLowerInvariant(), ""))
                    {
                        linesToRemove.Add(gp);
                    }
                }
            }
            foreach (string[] line in linesToRemove)
            {
                CustomGroups.Remove(line);
            }
        }

        public void DeleteGroup(string groupname)
        {
            List<string[]> linesToRemove = new List<string[]>();
            foreach (var gp in CustomGroups)
            {
                if (groupname.ToLower().Equals(gp[1].ToLower()))
                {
                    linesToRemove.Add(gp);
                }
            }
            foreach (string[] line in linesToRemove)
            {
                CustomGroups.Remove(line);
            }
            AttributeGroups.Remove(groupname);
        }

        public void UpdateGroupNames(List<Attribute> attrlist)
        {
#if (PoESkillTree_UseSmallDec_ForAttributes)
            Dictionary<string, SmallDec> groupTotals = new Dictionary<string, SmallDec>();
            Dictionary<string, SmallDec> groupDeltas = new Dictionary<string, SmallDec>();
#else
            Dictionary<string, float> groupTotals = new Dictionary<string, float>();
            Dictionary<string, float> groupDeltas = new Dictionary<string, float>();
#endif
            foreach (var gp in CustomGroups)
            {
                //only sum for the groups that need it
                if (!gp[1].Contains("#"))
                    continue;
                foreach (Attribute attr in attrlist)
                {
                    if (NumberRegex.Replace(attr.Text.ToLowerInvariant(), "") == NumberRegex.Replace(gp[0].ToLowerInvariant(), ""))
                    {
                        Match matchResult = Regex.Match(attr.Text, DecimalRegex);
                        if (matchResult.Success)
                        {
                            if (!groupTotals.ContainsKey(gp[1]))
                            {
                                groupTotals.Add(gp[1], 0);
                                groupDeltas.Add(gp[1], 0);
                            }
#if (PoESkillTree_UseSmallDec_ForAttributes)
                            groupTotals[gp[1]] += (SmallDec) matchResult.Value;
#else
                            groupTotals[gp[1]] += (float)Decimal.Parse(matchResult.Value);
#endif
                            if (attr.Deltas.Length > 0)
                                groupDeltas[gp[1]] += attr.Deltas[0];
                        }

                    }
                }
            }

            string deltaString;
            foreach (string key in groupTotals.Keys)
            {
                if (AttributeGroups.ContainsKey(key))
                {
                    if (groupDeltas[key] == 0)
                        deltaString = "";
                    else if (groupDeltas[key] > 0)
                        deltaString = " +" + groupDeltas[key].ToString();
                    else
                        deltaString = " " + groupDeltas[key].ToString();
                    AttributeGroups[key].GroupName = "Custom: "+key.Replace("#", groupTotals[key].ToString())+deltaString;
                }
            }

        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value.ToString());
        }

        private static bool TryGetDefaultGroup(string attribute, out string group)
        {
            if (AttributeToDefaultGroup.TryGetValue(attribute, out group))
            {
                return group != null;
            }

            foreach (var gp in DefaultGroups)
            {
                if (attribute.Contains(gp[0].ToLowerInvariant()))
                {
                    group = gp[1];
                    AttributeToDefaultGroup[attribute] = gp[1];
                    return true;
                }
            }
            AttributeToDefaultGroup[attribute] = null;
            return false;
        }

        public int Compare(object a, object b)
        {
            string attr1 = ((Attribute)a).Text;
            string attr2 = ((Attribute)b).Text;
            var attr1Lower = attr1.ToLowerInvariant();
            var attr2Lower = attr2.ToLowerInvariant();
            //find the group names and types that the attributes belong in
            //2 = misc group, 1 = default group, 0 = custom group
            int group1 = 2;
            int group2 = 2;
            string attrgroup1 = MiscLabel;
            string attrgroup2 = MiscLabel;
            foreach (var gp in CustomGroups)
            {
                if (NumberRegex.Replace(attr1Lower, "") == NumberRegex.Replace(gp[0].ToLowerInvariant(), ""))
                {
                    attrgroup1 = gp[1];
                    group1 = 0;
                    break;
                }
            }
            if (group1 == 2)
            {
                string group;
                if (TryGetDefaultGroup(attr1Lower, out group))
                {
                    attrgroup1 = group;
                    group1 = 1;
                }
            }
            foreach (var gp in CustomGroups)
            {
                if (NumberRegex.Replace(attr2Lower, "") == NumberRegex.Replace(gp[0].ToLowerInvariant(), ""))
                {
                    attrgroup2 = gp[1];
                    group2 = 0;
                    break;
                }
            }
            if (group2 == 2)
            {
                string group;
                if (TryGetDefaultGroup(attr1Lower, out group))
                {
                    attrgroup2 = group;
                    group2 = 1;
                }
            }

            //primary: if group types are different, sort by group type - custom first, then defaults, then misc
            if (group1 != group2)
            {
                return group1 - group2;
            }
            //secondary: if groups are different, sort by group names, alphabetically, excluding numbers
            if (!attrgroup1.Equals(attrgroup2))
            {
                attrgroup1 = Regex.Replace(attrgroup1, DecimalRegex, "#");
                attrgroup2 = Regex.Replace(attrgroup2, DecimalRegex, "#");
                return attrgroup1.CompareTo(attrgroup2);
            }
            //tertiary: if in the same group, sort by attribute string, alphabetically, excluding numbers
            attr1 = Regex.Replace(attr1, DecimalRegex, "#");
            attr2 = Regex.Replace(attr2, DecimalRegex, "#");
            return attr1.CompareTo(attr2);
        }

        public AttributeGroup Convert(string s)
        {
            foreach (var gp in CustomGroups)
            {
                if (NumberRegex.Replace(s.ToLowerInvariant(), "") == NumberRegex.Replace(gp[0].ToLowerInvariant(), ""))
                {
                    return AttributeGroups[gp[1]];
                }
            }
            string group;
            if (TryGetDefaultGroup(s.ToLowerInvariant(), out group))
            {
                return AttributeGroups[group];
            }
            return AttributeGroups[MiscLabel];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}