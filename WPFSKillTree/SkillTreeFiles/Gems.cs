using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using POESKillTree.Model;
using Item = POESKillTree.ViewModels.ItemAttributes.Item;
using GemColorClass = POESKillTree.ViewModels.ItemAttributes.Item.GemColorClass;
using Mod = POESKillTree.ViewModels.ItemAttributes.Item.Mod;

namespace POESKillTree.SkillTreeFiles
{
    /* Level of support:
     *     None = Gem is being completely ignored.
     *     Unknown = Gem wasn't tested and it doesn't have DB entry, so its statistics are probably incorrect (in +Level to Gems items they are for sure)
     *     Partial = Gem was partialy tested, but it doesn't have DB entry, so its statistics should be correct (except when used in items with +Level to Gems).
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
     * Leap Slam                                    Partial
     * Lightning Strike                             Full
     * Molten Shell                                 Partial
     * Molten Strike                                Full
     * Punishment                                   Unknown
     * Purity of Fire                               None
     * Rejuvenation Totem                           None
     * Searing Bond                                 None
     * Shield Charge                                Unknown
     * Shockwave Totem                              None
     * Sweep                                        Unknown
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
     * Herald of Ice                                Partial
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
     * Split Arrow                                  Partial
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
     * Fireball                                     Partial
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
     * Lightning Warp                               Partial
     * Power Siphon                                 Partial
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
     * Tempest Shield                               Partial
     * Vulnerability                                None
     * Wrath                                        None
     * 
     * Support gems:
     * =============
     * Added Chaos Damage                           Unknown
     * Added Cold Damage                            Unknown
     * Added Fire Damage                            Unknown
     * Added Lightning Damage                       Partial
     * Additional Accuracy                          None
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
     * Faster Attacks                               None
     * Faster Casting                               None
     * Faster Projectiles                           Partial
     * Fire Penetration                             None
     * Fork                                         None
     * Generosity                                   None
     * Greater Multiple Projectiles                 Unknown
     * Increased Area of Effect                     None
     * Increased Burning Damage                     None
     * Increased Critical Damage                    None
     * Increased Critical Strikes                   None
     * Increased Duration                           None
     * Iron Grip                                    None
     * Iron Will                                    None
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
     * Point Blank                                  None
     * Power Charge On Critical                     None
     * Ranged Attack Totem                          None
     * Reduced Duration                             None
     * Reduced Mana                                 None
     * Remote Mine                                  None
     * Slower Projectiles                           None
     * Spell Echo                                   None
     * Spell Totem                                  None
     * Stun                                         None
     * Trap                                         None
     * Weapon Elemental Damage                      Partial
     */
    public class Gems
    {
        class Gem
        {
            internal Dictionary<string, Value> Attrs;

            internal AttributeSet AttributesAt(float level)
            {
                AttributeSet attrs = new AttributeSet();

                foreach (var mod in Attrs)
                    attrs.Add(mod.Key, mod.Value.ValueAt(level));

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

            override internal List<float> ValueAt(float level)
            {
                return new List<float> { A * level + B };
            }
        }

        class RangeMap : Value
        {
            List<float[]> Ranges;

            // <arg1, arg2>: arg3, <arg4, arg5>: arg6, ...
            internal RangeMap(params float[] arguments)
            {
                if (arguments.Length % 3 > 0) throw new ArgumentException("Invalid number of arguments");

                Ranges = new List<float[]>();
                for (int i = 0; i < arguments.Length; i += 3)
                    Ranges.Add(new float[] { arguments[i], arguments[i + 1], arguments[i + 2] });
            }

            override internal List<float> ValueAt(float level)
            {
                foreach (float[] range in Ranges)
                    if (level >= range[0] && level <= range[1]) return new List<float> { range[2] };

                throw new IndexOutOfRangeException("No range for level: " + level);
            }
        }

        abstract class Value {
            abstract internal List<float> ValueAt(float level);
        }

        class Values : Dictionary<string, Value> { }

        // Returns local attributes and modifiers of gem in item.
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

                // Override modifiers by leveled up gem ones.
                if (level > 0)
                {
                    Gem entry = DB[gem.Name];
                    AttributeSet leveledUp = entry.AttributesAt(level + gem.Attributes["Level:  #"][0]);
                    attrs.Override(leveledUp);
                }
            }

            return attrs;
        }

        // Constant to match any level above first argument of RangeMap triplet.
        const float MaxLevel = float.PositiveInfinity;

        readonly static Regex ReGemLevelClass = new Regex("\\+# to Level of (Strength|Dexterity|Intelligence) Gems in this item");
        readonly static Regex ReGemLevelKeyword = new Regex("\\+# to Level of (.+) Gems in this item");

        readonly static Dictionary<string, Gem> DB = new Dictionary<string, Gem> {
            {
                "Lightning Strike",
                new Gem {
                    Attrs = new Values {
                        { "Mana Cost:  #", new RangeMap(1, 9, 9, 10, MaxLevel, 10) },
                        { "#% increased Physical Damage", new Linear(3, -3) }
                    }
                }
            }, {
                "Molten Strike",
                new Gem {
                    Attrs = new Values {
                        { "Mana Cost:  #", new RangeMap(1, 1, 6, 2, 5, 7, 6, 18, 8, 19, MaxLevel, 10) },
                        { "#% increased Physical Damage", new Linear(4, -4) }
                    }
                }
            }
        };
    }
}
