using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using log4net;
using POESKillTree.Model.Items;
using POESKillTree.Model.Items.Affixes;
using POESKillTree.Utils;
using POESKillTree.Compute;
using POESKillTree.Utils.Extensions;

namespace POESKillTree.Model.Gems
{
    // TODO: Attributes can have negative value (Cast when Damage Taken L20 has 6% more Damage), AttributesOf should handle transition between less/more.
    public class GemDB
    {
        public static GemDB Instance { get; set; } = new GemDB();
        bool IsIndexed { get; set; }


        private static readonly ILog Log = LogManager.GetLogger(typeof(GemDB));

        // Maximum level (quality).
        public const int MAX_LEVEL = 30;

        // Deserialized gems.
        [XmlElement("Gem")]
        public List<Gem> Gems = new List<Gem>();
        // Deserialized ItemDatabase instance.

        // Indexed gems.
        private Dictionary<string, Gem> GemIndex = new Dictionary<string, Gem>();

        // Pattern to match +Level item modifiers.
        private static Regex ReGemLevelKeyword = new Regex(@"\+# to Level of Socketed (.+) Gems");

        // Adds gem to deserialized gems.
        public void Add(Gem gem)
        {
            if (!Gems.Exists(g => g.Name == gem.Name))
            {
                gem.Optimize();

                if (gem.Attributes != null)
                {
                    Comparer<Value> comparer = new ValueLogicalComparer();
                    foreach (GemAttribute attr in gem.Attributes)
                        attr.Values.Sort(comparer);
                }

                Gems.Add(gem);
            }
        }

        // Returns attributes of gem in item.
        public AttributeSet AttributesOf(Item gem, Item item)
        {
            AttributeSet attrs = new AttributeSet();

            // Collect gem attributes and modifiers at gem level.
            foreach (var prop in gem.Properties)
                attrs.Add(prop.Attribute, new List<float>(prop.Value));
            foreach (ItemMod mod in gem.Mods)
                attrs.Add(mod.Attribute, new List<float>(mod.Value));

            // Check if gem is in database.
            if (GemIndex.ContainsKey(gem.Name))
            {
                Gem entry = GemIndex[gem.Name];

                // Process +Level modifiers from item.
                int plusLevel = 0;
                foreach (ItemMod mod in item.Mods)
                {
                    if (mod.Attribute == "+# to Level of Socketed Gems")
                        plusLevel += (int)mod.Value[0];
                    else
                    {
                        Match m = ReGemLevelKeyword.Match(mod.Attribute);
                        if (m.Success)
                        {
                            if (gem.Keywords.Contains(m.Groups[1].Value)
                                || m.Groups[1].Value == "Elemental" && (gem.Keywords.Contains("Cold") || gem.Keywords.Contains("Fire") || gem.Keywords.Contains("Lightning")))
                                plusLevel += (int)mod.Value[0];
                        }
                    }
                }

                // Replace level-based attributes of gem (even without level bonus).
                AttributeSet replace = entry.AttributesAtLevel(plusLevel + LevelOf(gem));

                // Add quality-based attributes if Quality attributes are defined.
                int quality = QualityOf(gem);
                if (quality > 0)
                    replace.Add(entry.AttributesAtQuality(quality));

                attrs.Replace(replace);
            }

            return attrs;
        }

        // Returns attributes of gem at specified level and quality.
        public AttributeSet AttributesOf(string gemName, int level, int quality)
        {
            if (!IsIndexed)
                throw new Exception("Gem Index not started");

            if (!GemIndex.ContainsKey(gemName))
                return new AttributeSet();

            Gem gem = GemIndex[gemName];

            // Get level-based attributes.
            AttributeSet attrs = gem.AttributesAtLevel(level);

            // Add quality-based attributes if quality is 1 or above.
            if (quality > 0)
                attrs.Add(gem.AttributesAtQuality(quality));

            return attrs;
        }

        // Returns true if gem can support attack skill, false otherwise.
        public bool CanSupport(AttackSkill skill, Item gem)
        {
            if (!IsIndexed)
                throw new Exception("Gem Index not started");

            if (GemIndex.ContainsKey(gem.Name))
            {
                Gem entry = GemIndex[gem.Name];

                // No support for excluded forms.
                if (entry.ExcludeFormSupport != DamageForm.Any && skill.Nature.Is(entry.ExcludeFormSupport)) return false;
            }

            return true;
        }

        // Returns true if gem can support attack skill, false otherwise.
        public bool CanSupport(AttackSkill skill, string gemName)
        {
            if (!IsIndexed)
                throw new Exception("Gem Index not started");

            if (GemIndex.ContainsKey(gemName))
            {
                Gem entry = GemIndex[gemName];

                // No support for excluded forms.
                if (entry.ExcludeFormSupport != DamageForm.Any && skill.Nature.Is(entry.ExcludeFormSupport)) return false;
            }

            return true;
        }

        // Returns true if gem can use weapon, false otherwise.
        public bool CanUse(Item gem, Weapon weapon, Computation compute)
        {
            if (!IsIndexed)
                throw new Exception("Gem Index not started");
            if (GemIndex.ContainsKey(gem.Name))
            {
                Gem entry = GemIndex[gem.Name];

                if (entry.RequiredHand != WeaponHand.Any && !weapon.Is(entry.RequiredHand))
                    return false;

                if (entry.RequiresEquippedShield && !compute.IsWieldingShield)
                    return false;

                // Weapon having "Counts as Dual Wielding" mod cannot be used to perform skills that require a two-handed weapon.
                // @see http://pathofexile.gamepedia.com/Wings_of_Entropy
                if (entry.RequiredWeapon != WeaponType.Any && (entry.RequiredWeapon & WeaponType.TwoHandedMelee) != 0
                    && weapon.Attributes.ContainsKey("Counts as Dual Wielding"))
                    return false;
            }

            return true;
        }


        // Returns deserialized gem.
        public Gem GetGem(string gemName)
        {
            return Gems.Find(g => g.Name == gemName);
        }

        // Returns numbner of hits skill gem does per single attack.
        public float HitsPerAttackOf(Item gem)
        {
            return GemIndex.ContainsKey(gem.Name) ? GemIndex[gem.Name].HitsPerAttack : 1;
        }

        // Indexes items in database.
        public void Index()
        {
            GemIndex.Clear();

            foreach (var gem in Gems)
            {
                gem.Index();
                GemIndex.Add(gem.Name, gem);
            }
            IsIndexed = true;
        }

        // Returns true if database is empty, false otherwise.
        public bool IsEmpty()
        {
            return GemIndex.Count == 0;
        }

        // Returns true if skill strikes with both weapons at once.
        public bool IsStrikingWithBothWeaponsAtOnce(Item gem)
        {
            return GemIndex.ContainsKey(gem.Name) ? GemIndex[gem.Name].StrikesWithBothWeapons : false;
        }

        // Returns level of gem.
        public static int LevelOf(Item gem)
        {
            float ret;
            if (gem.Properties.TryGetValue("Level: #", 0, out ret))
                return (int)ret;
            else
                return (int)gem.Properties.First("Level: # (Max)", 0, 1);
        }

        // Loads items from XML file.
        public static GemDB LoadFromText(string text, bool index = false)
        {
            var db = XmlHelpers.DeserializeXml<GemDB>(text);
            if (index)
                db.Index();
            return db;
        }

        public void Merge(GemDB merge)
        {
            if (merge != null)
            {
                // Merge gems.
                foreach (Gem gem in merge.Gems.OrEmptyIfNull())
                {
                    gem.Optimize();

                    Gem with = Gems.Find(g => g.Name == gem.Name);
                    if (with == null)
                        Gems.Add(gem);
                    else
                        with.Merge(gem);
                }
            }
        }

        // Returns damage nature of gem.
        public DamageNature NatureOf(Item gem)
        {
            // Implicit nature from keywords.
            DamageNature nature = new DamageNature(gem.Keywords);

            if (nature.Is(DamageSource.Attack))
            {
                // Attacks with melee form implicitly gets melee weapon type.
                if ((nature.Form & DamageForm.Melee) != 0)
                    nature.WeaponType |= WeaponType.Melee;
                // Attacks with ranged weapons implicitly gets projectile form.
                if (nature.Is(WeaponType.Ranged))
                    nature.Form |= DamageForm.Projectile;
            }
            else if (nature.Is(DamageSource.Cast)) // All Cast skill gems have damage on use form.
                nature.Form |= DamageForm.OnUse;

            if (GemIndex.ContainsKey(gem.Name))
            {
                Gem entry = GemIndex[gem.Name];

                // Override weapon type requirement if defined.
                if (entry.RequiredWeapon != WeaponType.Any)
                    nature.WeaponType = entry.RequiredWeapon;

                // Ignore form.
                if (entry.ExcludeForm != DamageForm.Any)
                    nature.Form ^= entry.ExcludeForm;

                // Ignore source.
                if (entry.ExcludeSource != DamageSource.Any)
                    nature.Source ^= entry.ExcludeSource;

                // Include form.
                if (entry.IncludeForm != DamageForm.Any)
                    nature.Form |= entry.IncludeForm;

                // Unarmed.
                if (entry.Unarmed)
                    nature.WeaponType |= WeaponType.Unarmed;
            }

            return nature;
        }

        // Returns quality of gem.
        public static int QualityOf(Item gem)
        {
            float ret;
            if (gem.Properties.TryGetValue("Quality: #", 0, out ret))
                return (int)ret;
            else
                return (int)gem.Properties.First("Quality: # (Max)", 0, 0);
        }

        // Writes database to file.
        public void WriteTo(string file)
        {
            WriteToCompletePath(AppData.GetFolder(true) + file);
        }
        public void WriteToCompletePath(string file)
        {
            // Sort gems alphabetically.
            Gems.Sort(delegate (Gem gem1, Gem gem2) { return String.Compare(gem1.Name, gem2.Name, true, System.Globalization.CultureInfo.InvariantCulture); });

            var serializer = new XmlSerializer(typeof(GemDB));
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineChars = "\n";
            XmlWriter writer = XmlTextWriter.Create(file, settings);
            serializer.Serialize(writer, this);
            writer.Close();
        }
    }
}

/* List of unsupported gems
 * 
 * Strength skill gems:
 * ====================
 * Anger                                        None
 * Animate Guardian                             None
 * Decoy Totem                                  None
 * Determination                                None
 * Devouring Totem                              None
 * Enduring Cry                                 None
 * Flame Totem                                  None
 * Herald of Ash                                None
 * Immortal Call                                None
 * Punishment                                   None
 * Purity of Fire                               None
 * Rejuvenation Totem                           None
 * Searing Bond                                 None
 * Shockwave Totem                              None
 * Vitality                                     None
 * Warlord's Mark                               None
 * 
 * Dexterity skill gems:
 * =====================
 * Animate Weapon                               None
 * Arctic Armour                                None
 * Bear Trap                                    None
 * Blood Rage                                   None
 * Desecrate                                    None
 * Detonate Dead                                None
 * Fire Trap                                    None
 * Freeze Mine                                  None
 * Grace                                        None
 * Haste                                        None
 * Hatred                                       None
 * Poacher's Mark                               None
 * Projectile Weakness                          None
 * Purity of Ice                                None
 * Smoke Mine                                   None
 * Temporal Chains                              None
 *
 * Intelligence skill gems:
 * ========================
 * Assassin's Mark                              None
 * Bone Offering                                None
 * Clarity                                      None
 * Conductivity                                 None
 * Conversion Trap                              None
 * Convocation                                  None
 * Critical Weakness                            None
 * Discharge                                    None
 * Discipline                                   None
 * Elemental Weakness                           None
 * Enfeeble                                     None
 * Flammability                                 None
 * Flesh Offering                               None
 * Frostbite                                    None
 * Lightning Trap                               None
 * Purity of Elements                           None
 * Purity of Lightning                          None
 * Raise Spectre                                None
 * Raise Zombie                                 None
 * Righteous Fire                               None
 * Summon Raging Spirit                         None
 * Summon Skeletons                             None
 * Vulnerability                                None
 * Wrath                                        None
 * 
 * Support gems:
 * =============
 * Blind                                        None
 * Block Chance Reduction                       None
 * Blood Magic                                  None
 * Chance to Flee                               None
 * Chance to Ignite                             None
 * Cold Penetration                             None
 * Culling Strike                               None
 * Curse on Hit                                 None
 * Elemental Proliferation                      None
 * Empower                                      None
 * Endurance Charge on Melee Stun               None
 * Enhance                                      None
 * Enlighten                                    None
 * Fire Penetration                             None
 * Generosity                                   None
 * Increased Area of Effect                     None
 * Increased Burning Damage                     None
 * Increased Duration                           None
 * Item Quantity                                None
 * Item Rarity                                  None
 * Knockback                                    None
 * Life Gain on Hit                             None
 * Life Leech                                   None
 * Lightning Penetration                        None
 * Mana Leech                                   None
 * Minion and Totem Elemental Resistance        None
 * Minion Damage                                None
 * Minion Life                                  None
 * Minion Speed                                 None
 * Multiple Traps                               None
 * Pierce                                       None
 * Power Charge On Critical                     None
 * Ranged Attack Totem                          None
 * Reduced Duration                             None
 * Reduced Mana                                 None
 * Remote Mine                                  None
 * Spell Totem                                  None
 * Stun                                         None
 * Trap                                         None
 */
