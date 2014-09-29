using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using POESKillTree.Model;
using Item = POESKillTree.ViewModels.ItemAttributes.Item;
using GemColorClass = POESKillTree.ViewModels.ItemAttributes.Item.GemColorClass;
using Mod = POESKillTree.ViewModels.ItemAttributes.Item.Mod;
using DamageArea = POESKillTree.SkillTreeFiles.Compute.DamageArea;
using DamageForm = POESKillTree.SkillTreeFiles.Compute.DamageForm;
using DamageNature = POESKillTree.SkillTreeFiles.Compute.DamageNature;
using DamageSource = POESKillTree.SkillTreeFiles.Compute.DamageSource;
using WeaponHand = POESKillTree.SkillTreeFiles.Compute.WeaponHand;
using WeaponType = POESKillTree.SkillTreeFiles.Compute.WeaponType;
using Weapon = POESKillTree.SkillTreeFiles.Compute.Weapon;

namespace POESKillTree.SkillTreeFiles
{
    /* Level of support:
     *     None = Gem is being completely ignored.
     *     Unknown = Gem wasn't tested and it doesn't have DB entry, so its statistics are probably incorrect (in +Level to Gems items they are for sure)
     *     Partial = Gem was partialy tested, but it doesn't have DB entry, so its statistics should be correct (except when used in items with +Level to Gems).
     *     Incomplete = Gem was tested, but DB entries are incomplete, so statistics at certain level could be incorrect.
     *     Full = Gem was tested and it has DB entry. It should show correct statistics or all of its modifiers should be applied in full range.
     * 
     * Strength skill gems:
     * ====================
     * Anger                                        None
     * Animate Guardian                             None
     * Cleave                                       None
     * Decoy Totem                                  None
     * Determination                                None
     * Devouring Totem                              None
     * Dominating Blow                              None
     * Enduring Cry                                 None
     * Flame Totem                                  None
     * Glacial Hammer                               Unknown
     * Ground Slam                                  Unknown
     * Heavy Strike                                 Unknown
     * Herald of Ash                                None
     * Immortal Call                                None
     * Infernal Blow                                Unknown
     * Leap Slam                                    Incomplete
     * Lightning Strike                             Incomplete
     * Molten Shell                                 Incomplete
     * Molten Strike                                Incomplete
     * Punishment                                   Unknown
     * Purity of Fire                               None
     * Rejuvenation Totem                           None
     * Searing Bond                                 None
     * Shield Charge                                Unknown
     * Shockwave Totem                              None
     * Sweep                                        Partial
     * Vitality                                     None
     * Warlord's Mark                               None
     * 
     * Dexterity skill gems:
     * =====================
     * Animate Weapon                               None
     * Arctic Armour                                None
     * Barrage                                      Unknown
     * Bear Trap                                    None
     * Blood Rage                                   None
     * Burning Arrow                                Unknown
     * Cyclone                                      Unknown
     * Desecrate                                    None
     * Detonate Dead                                None
     * Double Strike                                Partial
     * Dual Strike                                  Unknown
     * Elemental Hit                                Unknown
     * Ethereal Knives                              Partial
     * Explosive Arrow                              Unknown
     * Fire Trap                                    None
     * Flicker Strike                               Unknown
     * Freeze Mine                                  None
     * Frenzy                                       Unknown
     * Grace                                        None
     * Haste                                        None
     * Hatred                                       None
     * Herald of Ice                                Incomplete
     * Ice Shot                                     Unknown
     * Lightning Arrow                              Partial
     * Poacher's Mark                               None
     * Poison Arrow                                 Unknown
     * Projectile Weakness                          None
     * Puncture                                     Unknown
     * Purity of Ice                                None
     * Rain of Arrows                               Partial
     * Reave                                        Unknown
     * Smoke Mine                                   None
     * Spectral Throw                               Unknown
     * Split Arrow                                  Incomplete
     * Temporal Chains                              None
     * Tornado Shot                                 Partial
     * Viper Strike                                 Partial
     * Whirling Blades                              Unknown
     *
     * Intelligence skill gems:
     * ========================
     * Arc                                          Partial
     * Arctic Breath                                Unknown
     * Assassin's Mark                              None
     * Ball Lightning                               Unknown
     * Bone Offering                                None
     * Clarity                                      None
     * Cold Snap                                    Unknown
     * Conductivity                                 None
     * Conversion Trap                              None
     * Convocation                                  None
     * Critical Weakness                            None
     * Discharge                                    None
     * Discipline                                   None
     * Elemental Weakness                           None
     * Enfeeble                                     None
     * Fireball                                     Full
     * Firestorm                                    Unknown
     * Flameblast                                   Unknown
     * Flame Surge                                  Unknown
     * Flammability                                 None
     * Flesh Offering                               None
     * Freezing Pulse                               Unknown
     * Frost Wall                                   Unknown
     * Frostbite                                    Unknown
     * Glacial Cascade                              Unknown
     * Ice Nova                                     Unknown
     * Ice Spear                                    Unknown
     * Incinerate                                   Unknown
     * Lightning Trap                               None
     * Lightning Warp                               Incomplete
     * Power Siphon                                 Incomplete
     * Purity of Elements                           None
     * Purity of Lightning                          None
     * Raise Spectre                                None
     * Raise Zombie                                 None
     * Righteous Fire                               None
     * Shock Nova                                   Unknown
     * Spark                                        Unknown
     * Storm Call                                   Unknown
     * Summon Raging Spirit                         None
     * Summon Skeletons                             None
     * Tempest Shield                               Incomplete
     * Vulnerability                                None
     * Wrath                                        None
     * 
     * Support gems:
     * =============
     * Added Chaos Damage                           Unknown
     * Added Cold Damage                            Unknown
     * Added Fire Damage                            Unknown
     * Added Lightning Damage                       Partial
     * Additional Accuracy                          Unknown
     * Blind                                        None
     * Block Chance Reduction                       None
     * Blood Magic                                  None
     * Cast on Critical Strike                      None
     * Cast on Death                                None
     * Cast on Melee Kill                           None
     * Cast when Damage Taken                       None
     * Cast when Stunned                            None
     * Chain                                        None
     * Chance to Flee                               None
     * Chance to Ignite                             None
     * Cold Penetration                             None
     * Cold to Fire                                 Partial
     * Concentrated Effect                          Partial
     * Culling Strike                               None
     * Curse on Hit                                 None
     * Elemental Proliferation                      None
     * Empower                                      None
     * Endurance Charge on Melee Stun               None
     * Enhance                                      None
     * Enlighten                                    None
     * Faster Attacks                               Unknown
     * Faster Casting                               Unknown
     * Faster Projectiles                           Partial
     * Fire Penetration                             None
     * Fork                                         None
     * Generosity                                   None
     * Greater Multiple Projectiles                 Unknown
     * Increased Area of Effect                     None
     * Increased Burning Damage                     None
     * Increased Critical Damage                    Partial
     * Increased Critical Strikes                   Partial
     * Increased Duration                           None
     * Iron Grip                                    Partial
     * Iron Will                                    Partial
     * Item Quantity                                None
     * Item Rarity                                  None
     * Knockback                                    None
     * Lesser Multiple Projectiles                  Unknown
     * Life Gain on Hit                             None
     * Life Leech                                   None
     * Lightning Penetration                        None
     * Mana Leech                                   None
     * Melee Damage on Full Life                    Partial
     * Melee Physical Damage                        Unknown
     * Melee Splash                                 Unknown
     * Minion and Totem Elemental Resistance        None
     * Minion Damage                                None
     * Minion Life                                  None
     * Minion Speed                                 None
     * Multiple Traps                               None
     * Multistrike                                  Unknown
     * Physical Projectile Attack Damage            Unknown
     * Pierce                                       None
     * Point Blank                                  Unknown
     * Power Charge On Critical                     None
     * Ranged Attack Totem                          None
     * Reduced Duration                             None
     * Reduced Mana                                 None
     * Remote Mine                                  None
     * Slower Projectiles                           Unknown
     * Spell Echo                                   Partial
     * Spell Totem                                  None
     * Stun                                         None
     * Trap                                         None
     * Weapon Elemental Damage                      Partial
     */
    public class Gems
    {
        class Gem
        {
            // Defines attribute value progression along the gem levels.
            internal Dictionary<string, Value> Attrs;
            // Defines requirement of specific hand.
            internal WeaponHand RequiredHand = WeaponHand.Any;
            // Defines requirement of specific weapon type.
            internal WeaponType RequiredWeapon = WeaponType.Any;
            // Defines whether AoE nature of skill should be ignored.
            internal bool IgnoreArea = false;
            // Defines which form of skill should be ignored.
            internal DamageForm IgnoreForm = DamageForm.Any;
            // Defines which damage source nature of skill should be ignored.
            internal DamageSource IgnoreSource = DamageSource.Any;
            // Defines which damage form should be included.
            internal DamageForm IncludeForm = DamageForm.Any;
            // Defines whether damage dealt by skill is cast by character and thus affected by cast speed.
            internal bool NonCastDamage = false;

            // Returns attributes of gem which have defined values for specified level.
            internal AttributeSet AttributesAt(float level)
            {
                AttributeSet attrs = new AttributeSet();

                if (Attrs != null)
                    foreach (var attr in Attrs)
                    {
                        List<float> value = attr.Value.ValueAt(level);
                        if (value != null)
                            attrs.Add(attr.Key, value);
                    }

                return attrs;
            }
        }

        class Linear : Value
        {
            float A;
            float B;

            // f(level) = A * level + B
            internal Linear(float a, float b)
            {
                A = a;
                B = b;
            }

            // Returns value for specified level, based on linear equation with defined coefficients.
            override internal List<float> ValueAt(float level)
            {
                return new List<float> { A * level + B };
            }
        }

        class RangeMap : Value
        {
            internal class Range
            {
                internal int From;
                internal int To;
                internal float[] Fixed;
                internal Value Expr;

                // Return true if range containes specified level, false otherwise.
                internal bool Contains(float level)
                {
                    return level >= From && level <= To;
                }

                // Returns fixed or linear/table value at specified level.
                internal List<float> ValueAt(float level)
                {
                    return Expr == null ? new List<float>(Fixed) : (Expr is Table ? Expr.ValueAt(level - From + 1) : Expr.ValueAt(level));
                }
            }

            List<Range> Ranges = new List<Range>();

            // Arguments define following ranges: level in range <arg1, arg2> => arg3, level in range <arg4, arg5> => arg6, etc.
            // The value arguments (arg3, arg6, etc.) can be simple values of type int, float, string, or complex values of type Linear or Table.
            internal RangeMap(params object[] arguments)
            {
                if (arguments.Length % 3 > 0) throw new ArgumentException("Invalid number of arguments");

                for (int i = 0; i < arguments.Length; i += 3)
                {
                    Range range;

                    if (arguments[i + 2] is Value)
                        range = new Range { From = (int)arguments[i], To = (int)arguments[i + 1], Expr = (Value)arguments[i + 2] };
                    else
                        range = new Range { From = (int)arguments[i], To = (int)arguments[i + 1], Fixed = ValueOf(arguments[i + 2]) };

                    Ranges.Add(range);
                }
            }

            // Returns value for specified level.
            override internal List<float> ValueAt(float level)
            {
                foreach (Range range in Ranges)
                    if (range.Contains(level))
                        return range.ValueAt(level);

                return null;
            }
        }

        class Table : Value
        {
            internal int Count { get { return Values.Count; } }
            List<float[]> Values = new List<float[]>();

            // When used directly, values are mapped as follows: level 1 => arg1, level 2 => arg2, ..., level N => argN
            // When used as RangeMap value, values are mapped as follows: from => arg1, from + 1 => arg2, ..., to => argN
            internal Table(params object[] arguments)
            {
                for (int i = 0; i < arguments.Length; ++i)
                    Values.Add(ValueOf(arguments[i]));
            }

            // Returns value for specified level or null if value was not found in table.
            override internal List<float> ValueAt(float level)
            {
                if (level < 1 || level > Values.Count) return null;

                return new List<float>(Values[(int)level - 1]);
            }
        }

        abstract class Value {
            // Implementation in derived class must return either defined value or null if value isn't defined for specified level.
            abstract internal List<float> ValueAt(float level);

            static Regex ReRangeValue = new Regex("(\\d+)[-–](\\d+)");

            internal static float[] ValueOf(object any)
            {
                if (any is string)
                {
                    Match m = ReRangeValue.Match((string)any);
                    if (m.Success)
                        return new float[] { float.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture),
                                         float.Parse(m.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture) };

                    return new float[] { float.NaN, float.NaN };
                }
                else if (any is float)
                    return new float[] { (float)any };
                else if (any is int)
                    return new float[] { (int)any };

                return new float[] { float.NaN };
            }
        }

        class Values : Dictionary<string, Value> { }

        // Returns attributes of gem in item.
        public static AttributeSet AttributesOf(Item gem, Item item)
        {
            AttributeSet attrs = new AttributeSet();

            // Collect gem attributes and modifiers at gem level.
            foreach (var attr in gem.Attributes)
                attrs.Add(attr.Key, new List<float>(attr.Value));
            foreach (Mod mod in gem.Mods)
                attrs.Add(mod.Attribute, new List<float>(mod.Value));

            // Check if gem is in database.
            if (DB.ContainsKey(gem.Name))
            {
                Gem entry = DB[gem.Name];

                // Process +Level modifiers from item.
                float level = 0;
                foreach (Mod mod in item.Mods)
                {
                    if (mod.Attribute == "+# to Level of Gems in this item")
                        level += mod.Value[0];
                    else
                    {
                        Match m = ReGemLevelClass.Match(mod.Attribute);
                        if (m.Success
                            && (m.Groups[1].Value == "Strength" && gem.GemClass == GemColorClass.Str
                                || m.Groups[1].Value == "Dexterity" && gem.GemClass == GemColorClass.Dex
                                || gem.GemClass == GemColorClass.Int))
                            level += mod.Value[0];
                        else
                        {
                            m = ReGemLevelKeyword.Match(mod.Attribute);
                            if (m.Success && gem.Keywords.Contains(m.Groups[1].Value))
                                level += mod.Value[0];
                        }
                    }
                }

                // Override attributes by leveled up gem ones.
                if (level > 0)
                {
                    AttributeSet leveledUp = entry.AttributesAt(level + gem.Attributes["Level:  #"][0]);
                    attrs.Override(leveledUp);
                }
                else // Add additional attributes not found on gem.
                {
                    AttributeSet additional = entry.AttributesAt(gem.Attributes["Level:  #"][0]);
                    foreach (var attr in additional)
                        if (!gem.Attributes.ContainsKey(attr.Key) && !gem.Mods.Exists(m => m.Attribute == attr.Key))
                            attrs.Add(attr);
                }
            }

            return attrs;
        }

        // Returns attributes of gem at specified level (Only used in UnitTests).
        public static AttributeSet AttributesOf(string gemName, float level)
        {
            return DB[gemName].AttributesAt(level);
        }


        // Returns true if gem can use weapon, false otherwise.
        public static bool CanUse(Item gem, Weapon weapon)
        {
            if (DB.ContainsKey(gem.Name))
            {
                Gem entry = DB[gem.Name];
                if (entry.RequiredHand != WeaponHand.Any && weapon.Hand != entry.RequiredHand)
                    return false;

                // Weapon having "Counts as Dual Wielding" mod cannot be used to perform skills that require a two-handed weapon.
                // @see http://pathofexile.gamepedia.com/Wings_of_Entropy
                if (entry.RequiredWeapon != WeaponType.Any && (entry.RequiredWeapon & WeaponType.TwoHandedMelee) != 0
                    && weapon.Attributes.ContainsKey("Counts as Dual Wielding"))
                    return false;
            }

            return true;
        }

        // Returns false if NonCastDamage is set to true, true otherwise.
        public static bool IsDamageCast(Item gem)
        {
            return DB.ContainsKey(gem.Name) && DB[gem.Name].NonCastDamage ? false : true;
        }

        // Returns damage nature of gem.
        public static DamageNature NatureOf(Item gem)
        {
            // Implicit nature from keywords.
            DamageNature nature = new DamageNature(gem.Keywords);

            if (DB.ContainsKey(gem.Name))
            {
                Gem entry = DB[gem.Name];

                // Override weapon type requirement if defined.
                if (entry.RequiredWeapon != WeaponType.Any)
                    nature.WeaponType = entry.RequiredWeapon;

                // Ignore AoE nature.
                if (entry.IgnoreArea)
                    nature.Area ^= DamageArea.Area;

                // Ignore form.
                if (entry.IgnoreForm != DamageForm.Any)
                    nature.Form ^= entry.IgnoreForm;

                // Ignore source.
                if (entry.IgnoreSource != DamageSource.Any)
                    nature.Source ^= entry.IgnoreSource;

                // Include form.
                if (entry.IncludeForm != DamageForm.Any)
                    nature.Form |= entry.IncludeForm;
            }

            return nature;
        }

        // Constant to match any level above first argument of RangeMap triplet.
        const int MaxLevel = int.MaxValue;

        readonly static Regex ReGemLevelClass = new Regex("\\+# to Level of (Strength|Dexterity|Intelligence) Gems in this item");
        readonly static Regex ReGemLevelKeyword = new Regex("\\+# to Level of (.+) Gems in this item");

        readonly static Dictionary<string, Gem> DB = new Dictionary<string, Gem> {
            {
                "Fireball",
                new Gem {
                    Attrs = new Values {
                        { "Mana Cost:  #", new RangeMap(1, 12, new Table(5, 6, 7, 8, 10, 11, 13, 15, 18, 20, 22, 23), 13, MaxLevel, new Linear(1, 12)) },
                        { "Deals #-# Fire Damage", new Table("5–10", "7–11", "9–14", "13–19", "17–25", "23–34", "32–48", "44–67", "63–95", "89–133",
                                                             "110–165", "135–203", "157–236", "183–274", "212–318", "245–368", "283–425", "326–489", "358–537", "393–590",
                                                             "431–647", "472–709", "518–776", "567–850", "620–930", "678–1017", "741–1111", "809–1214", "884–1326", "965–1447") }
                    },
                    IgnoreArea = true
                }
            }, {
                "Glacial Hammer",
                new Gem {
                    Attrs = new Values {
                        { "Mana Cost:  #", new RangeMap(1, 9, 11, 10, 15, 12, 16, MaxLevel, 13) },
                        { "#% increased Physical Damage", new Linear(4, -4) },
                        { "#% Chance to Freeze enemies", new RangeMap(1, 12, new Table(15, 16, 17, 18, 19, 20, 21, 21, 22, 22, 23, 23), 13, 15, 24, 16, 18, 25, 19, MaxLevel, 26) }
                    },
                    RequiredWeapon = WeaponType.Mace | WeaponType.Staff
                }
            }, {
                "Herald of Ice",
                new Gem {
                    Attrs = new Values {
                        { "Deals #-# Cold Damage", new Table("15–22", "18-27", "21-32", "25-38", "28-42", "33-49", "37-55", "44–67", "43-64", "50-75",
                                                             "55-82", "61-91", "67-100", "74-110", "81-121", "89-133", "98-146", "107-161", "117-176", "129-193",
                                                             "141-211", "154-231", "168-253") }
                    },
                    IgnoreSource = DamageSource.Spell,
                    NonCastDamage = true
                }
            }, {
                "Leap Slam",
                new Gem {
                    Attrs = new Values {
                        { "Attacks per Second:  #", new RangeMap(1, MaxLevel, 1 / 1.4f) }, // Leap Slam has its own attack time of 1.40 seconds.
                        { "Mana Cost:  #", new RangeMap(1, 5, 14, 6, 9, 15, 10, 13, 16, 14, 17, 17, 18, MaxLevel, 18) },
                        { "#% increased Physical Damage", new Linear(4, -4) },
                        { "#% Chance to Knock enemies Back on hit", new RangeMap(1, 9, new Table(10, 12, 14, 16, 18, 20, 21, 22, 23), 10, MaxLevel, 24) }
                    },
                    RequiredHand = WeaponHand.Main,
                    RequiredWeapon = WeaponType.Axe | WeaponType.Mace | WeaponType.Sword | WeaponType.Staff
                }
            }, {
                "Lightning Strike",
                new Gem {
                    Attrs = new Values {
                        { "Mana Cost:  #", new RangeMap(1, 9, 9, 10, MaxLevel, 10) },
                        { "#% increased Physical Damage", new Linear(3, -3) }
                    },
                    IgnoreForm = DamageForm.Projectile
                }
            }, {
                "Lightning Warp",
                new Gem {
                    Attrs = new Values {
                        { "Mana Cost:  #", new Table(26, 28, 30, 32, 33, 36, 37, 39, 41, 42, 44, 45, 46, 47, 48, 49, 49, 50, 51, 51, 52, 52) },
                        { "Deals #–# Lightning Damage", new Table("3–51", "3–62", "4–74", "5–88", "5–99", "6–117", "7–130", "8–153", "9–180", "10–199",
                                                                  "12–221", "13–244", "14–270", "16–299", "17–330", "19–364", "21–401", "23–441", "26–485", "28–534",
                                                                  "31–586", "34–644", "34–644", "34–644", "45-850") }
                    },
                    NonCastDamage = true
                }
            }, {
                "Molten Shell",
                new Gem {
                    Attrs = new Values {
                        { "Mana Cost:  #", new RangeMap(1, 17, new Table(22, 24, 28, 30, 34, 36, 38, 42, 46, 50, 54, 58, 60, 62, 64, 66, 66), 18, MaxLevel, new Linear(1, 50)) },
                        { "Deals #–# Fire Damage", new Table("26–39", "35–52", "45–68", "59–88", "75–113", "95–143", "120–180", "161–241", "214–321", "283–425",
                                                             "372–558", "455–682", "554–831", "674–1010", "817–1226", "989–1483", "1195–1792", "1354–2031", "1533–2300", "1735–2602",
                                                             "1962–2943", "2217–3326") }
                    },
                    NonCastDamage = true
                }
            }, {
                "Molten Strike",
                new Gem {
                    Attrs = new Values {
                        { "Mana Cost:  #", new RangeMap(1, 1, 6, 2, 5, 7, 6, 18, 8, 19, MaxLevel, 10) },
                        { "#% increased Physical Damage", new Linear(4, -4) }
                    },
                    IgnoreArea = true,
                    IgnoreForm = DamageForm.Projectile
                }
            }, {
                "Power Siphon",
                new Gem {
                    Attrs = new Values {
                        { "Mana Cost:  #", new RangeMap(1, 10, 13, 11, MaxLevel, 14 ) },
                        { "#% increased Physical Damage", new Linear(4, -4) }
                    },
                    RequiredWeapon = WeaponType.Wand
                }
            }, {
                "Split Arrow",
                new Gem {
                    Attrs = new Values {
                        { "Mana Cost:  #", new RangeMap(1, 1, 5, 2, 4, 6, 5, 8, 7, 9, 16, 8, 17, MaxLevel, 10) },
                        { "#% increased Physical Damage", new Linear(3, -3) },
                        { "# additional Arrows", new RangeMap(1, 4, 2, 5, 9, 3, 10, 16, 4, 17, 20, 5, 21, MaxLevel, 6) }
                    },
                    IncludeForm = DamageForm.Projectile
                }
            }, {
                "Sweep",
                new Gem {
                    Attrs = new Values {
                        { "Attacks per Second:  #", new RangeMap(1, MaxLevel, 1 / 1.15f) }, // Sweep has its own attack time of 1.15 seconds.
                    },
                    RequiredWeapon = WeaponType.Staff | WeaponType.TwoHandedAxe | WeaponType.TwoHandedMace
                }
            }, {
                "Tempest Shield",
                new Gem {
                    Attrs = new Values {
                        { "Deals #–# Lightning Damage", new Table("9–13", "11–17", "13–20", "16–24", "19–29", "23–34", "27–40", "32–49", "39–59", "47–70",
                                                                  "56–84", "64–96", "72–108", "82–123", "92–138", "104–156", "117–175", "126–189", "136–204", "147–220") }
                    },
                    RequiredWeapon = WeaponType.Shield,
                    NonCastDamage = true
                }
            }
        };
    }
}
