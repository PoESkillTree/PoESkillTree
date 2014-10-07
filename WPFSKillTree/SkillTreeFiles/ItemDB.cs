using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using POESKillTree.Model;
using Item = POESKillTree.ViewModels.ItemAttributes.Item;
using Mod = POESKillTree.ViewModels.ItemAttributes.Item.Mod;
using AttackSkill = POESKillTree.SkillTreeFiles.Compute.AttackSkill;
using DamageForm = POESKillTree.SkillTreeFiles.Compute.DamageForm;
using DamageNature = POESKillTree.SkillTreeFiles.Compute.DamageNature;
using DamageSource = POESKillTree.SkillTreeFiles.Compute.DamageSource;
using WeaponHand = POESKillTree.SkillTreeFiles.Compute.WeaponHand;
using WeaponType = POESKillTree.SkillTreeFiles.Compute.WeaponType;
using Weapon = POESKillTree.SkillTreeFiles.Compute.Weapon;
using System.Text.RegularExpressions;

namespace POESKillTree.SkillTreeFiles
{
    public class ItemDB
    {
        // Deserialized attribute.
        public class Attribute
        {
            // Deserialized name.
            [XmlAttribute]
            public string Name;
            // Deserialized array of values.
            [XmlElement(ElementName = "Value", Type = typeof(ValueAt))]
            [XmlElement(ElementName = "ValueForLevel", Type = typeof(ValueForLevelRange))]
            [XmlElement(ElementName = "ValueForQuality", Type = typeof(ValueForQualityRange))]
            [XmlElement(ElementName = "ValuePerLevel", Type = typeof(ValuePerLevel))]
            [XmlElement(ElementName = "ValuePerQuality", Type = typeof(ValuePerQuality))]
            public Value[] Values;
        }

        // The deserialized gem and gem index entry.
        public class Gem
        {
            // Deserialized name.
            [XmlAttribute]
            public string Name;
            // Deserialized array of attributes.
            [XmlElement("Attribute")]
            public Attribute[] Attributes;

            // Indexed level dependant attributes.
            [XmlIgnore]
            private Dictionary<string, LookupBase> LevelIndex = new Dictionary<string, LookupBase>();
            // Indexed quality dependant attributes.
            [XmlIgnore]
            private Dictionary<string, LookupBase> QualityIndex = new Dictionary<string, LookupBase>();

            // Defines which form of skill should be excluded from calculations.
            [XmlAttribute]
            public DamageForm ExcludeForm = DamageForm.Any;
            [XmlIgnore]
            public bool ExcludeFormSpecified { get { return ExcludeForm != DamageForm.Any; } }
            // Defines which form support gem does not support (i.e. skills having specified form won't be supported by gem).
            [XmlAttribute]
            public DamageForm ExcludeFormSupport = DamageForm.Any;
            [XmlIgnore]
            public bool ExcludeFormSupportSpecified { get { return ExcludeFormSupport != DamageForm.Any; } }
            // Defines which source of damage should be excluded from calculations.
            [XmlAttribute]
            public DamageSource ExcludeSource = DamageSource.Any;
            [XmlIgnore]
            public bool ExcludeSourceSpecified { get { return ExcludeSource != DamageSource.Any; } }
            // Defines number of hits skill does per single attack.
            [XmlAttribute]
            public int HitsPerAttack = 1;
            [XmlIgnore]
            public bool HitsPerAttackSpecified { get { return HitsPerAttack != 1; } }
            // Defines which form should be included to calculations.
            [XmlAttribute]
            public DamageForm IncludeForm = DamageForm.Any;
            [XmlIgnore]
            public bool IncludeFormSpecified { get { return IncludeForm != DamageForm.Any; } }
            // Defines whether skill requires shield being equipped.
            [XmlAttribute]
            public bool RequiresEquippedShield = false;
            [XmlIgnore]
            public bool RequiresEquippedShieldSpecified { get { return RequiresEquippedShield; } }
            // Defines requirement of specific hand as source of damage.
            [XmlAttribute]
            public WeaponHand RequiredHand = WeaponHand.Any;
            [XmlIgnore]
            public bool RequiredHandSpecified { get { return RequiredHand != WeaponHand.Any; } }
            // Defines requirement of specific weapon type as source of damage.
            [XmlAttribute]
            public WeaponType RequiredWeapon = WeaponType.Any;
            [XmlIgnore]
            public bool RequiredWeaponSpecified { get { return RequiredWeapon != WeaponType.Any; } }
            // Defines whether skill strikes with both weapons at once instead of alternating weapons while dual wielding.
            [XmlAttribute]
            public bool StrikesWithBothWeapons = false;
            [XmlIgnore]
            public bool StrikesWithBothWeaponsSpecified { get { return StrikesWithBothWeapons; } }

            // Returns all attributes of gem with defined values for specified level.
            internal AttributeSet AttributesAtLevel(int level)
            {
                AttributeSet attrs = new AttributeSet();

                foreach (var lookup in LevelIndex)
                {
                    List<float> value = lookup.Value.ValueAt(level);
                    if (value != null)
                        attrs.Add(lookup.Key, value);
                }

                return attrs;
            }

            // Returns all attributes of gem with defined values for specified quality.
            internal AttributeSet AttributesAtQuality(int quality)
            {
                AttributeSet attrs = new AttributeSet();

                foreach (var lookup in QualityIndex)
                {
                    List<float> value = lookup.Value.ValueAt(quality);
                    if (value != null)
                        attrs.Add(lookup.Key, value);
                }

                return attrs;
            }

            // Indexes attributes.
            internal void Index()
            {
                if (Attributes == null) return;

                foreach (var attr in Attributes)
                {
                    LookupTable levelTable = new LookupTable();
                    LookupTable qualityTable = new LookupTable();
                    LookupRanges levelRanges = new LookupRanges();
                    LookupRanges qualityRanges = new LookupRanges();
                    LookupGain levelGain = null;
                    LookupGain qualityGain = null;

                    foreach (var value in attr.Values)
                    {
                        if (value is ValueAt)
                        {
                            ValueAt valueAt = (ValueAt)value;
                            if (valueAt.LevelSpecified)
                                levelTable.Add(valueAt.Level, valueAt.ToValue());
                            else if (valueAt.QualitySpecified)
                                qualityTable.Add(valueAt.Quality, valueAt.ToValue());
                        }
                        else if (value is ValueForLevelRange)
                        {
                            ValueForLevelRange range = (ValueForLevelRange)value;
                            levelRanges.Add(range.From, range.To, range.ToValue());
                        }
                        else if (value is ValueForQualityRange)
                        {
                            ValueForQualityRange range = (ValueForQualityRange)value;
                            qualityRanges.Add(range.From, range.To, range.ToValue());
                        }
                        else if (value is ValuePerLevel)
                        {
                            levelGain = new LookupGain { From = 2, Value = ((ValuePerLevel)value).ToValue() };
                        }
                        else // value is ValuePerQuality
                        {
                            qualityGain = new LookupGain { Value = ((ValuePerQuality)value).ToValue() };
                        }
                    }

                    // Add level dependant attribute to index.
                    LookupBase.Method method = LookupBase.Method.None;
                    if (!levelTable.IsEmpty()) method |= LookupBase.Method.Table;
                    if (!levelRanges.IsEmpty()) method |= LookupBase.Method.Range;
                    if (levelGain != null) method |= LookupBase.Method.Gain;
                    if (method != LookupBase.Method.None && method != LookupBase.Method.Table && method != LookupBase.Method.Range && method != LookupBase.Method.Gain)
                        LevelIndex.Add(attr.Name, new LookupMixed
                        {
                            Table = method.HasFlag(LookupBase.Method.Table) ? levelTable : null,
                            Ranges = method.HasFlag(LookupBase.Method.Range) ? levelRanges : null,
                            Gain = method.HasFlag(LookupBase.Method.Gain) ? levelGain : null
                        });
                    else if (method.HasFlag(LookupBase.Method.Table))
                        LevelIndex.Add(attr.Name, levelTable);
                    else if (method.HasFlag(LookupBase.Method.Range))
                        LevelIndex.Add(attr.Name, levelRanges);
                    else if (method.HasFlag(LookupBase.Method.Gain))
                        LevelIndex.Add(attr.Name, levelGain);

                    // Add quality dependant attribute to index.
                    method = LookupBase.Method.None;
                    if (!qualityTable.IsEmpty()) method |= LookupBase.Method.Table;
                    if (!qualityRanges.IsEmpty()) method |= LookupBase.Method.Range;
                    if (qualityGain != null) method |= LookupBase.Method.Gain;
                    if (method != LookupBase.Method.None && method != LookupBase.Method.Table && method != LookupBase.Method.Range && method != LookupBase.Method.Gain)
                        QualityIndex.Add(attr.Name, new LookupMixed
                        {
                            Table = method.HasFlag(LookupBase.Method.Table) ? qualityTable : null,
                            Ranges = method.HasFlag(LookupBase.Method.Range) ? qualityRanges : null,
                            Gain = method.HasFlag(LookupBase.Method.Gain) ? qualityGain : null
                        });
                    else if (method.HasFlag(LookupBase.Method.Table))
                        QualityIndex.Add(attr.Name, qualityTable);
                    else if (method.HasFlag(LookupBase.Method.Range))
                        QualityIndex.Add(attr.Name, qualityRanges);
                    else if (method.HasFlag(LookupBase.Method.Gain))
                        QualityIndex.Add(attr.Name, qualityGain);
                }
            }
        }

        // Base class for all lookup methods.
        abstract internal class LookupBase
        {
            // Maximum level (quality).
            internal const int MAX_LEVEL = 30;
            // Available methods.
            internal enum Method { None, Gain = 1, Range = 2, Table = 4 };

            // Implementation in derived class must return either defined value(s) or null if value isn't defined for specified level.
            abstract internal List<float> ValueAt(int level);
        }

        // Fixed gain per level.
        internal class LookupGain : LookupBase
        {
            internal float[] Value;
            // Per quality gains start from quality 1, while per level gains start from level 2.
            internal int From = 1;

            // Returns value multiplied by level if level is greater or equal to From property, null otherwise.
            override internal List<float> ValueAt(int level)
            {
                if (level < From) return null;

                List<float> values = new List<float>();

                foreach (float value in Value)
                    values.Add(value * (level - From + 1));

                return values;
            }
        }

        // Mixed lookup in table, ranges and gain.
        internal class LookupMixed : LookupBase
        {
            internal LookupGain Gain;
            internal LookupRanges Ranges;
            internal LookupTable Table;

            // Returns defined value from either table, ranges or gain in this order.
            override internal List<float> ValueAt(int level)
            {
                List<float> value;
                
                if (Table != null)
                {
                    value = Table.ValueAt(level);
                    if (value != null) return value;
                }

                if (Ranges != null)
                {
                    value = Ranges.ValueAt(level);
                    if (value != null) return value;
                }

                return Gain == null ? null : Gain.ValueAt(level);
            }
        }

        // Single value for range of levels.
        internal class LookupRanges : LookupBase
        {
            class Range
            {
                internal int From;
                internal int To;
                internal float[] Value;
            }

            List<Range> Ranges = new List<Range>();

            // Adds value for given level range.
            internal void Add(int from, int to, float[] value)
            {
                Ranges.Add(new Range { From = from == 0 ? 1 : from, To = to == 0 ? MAX_LEVEL : to, Value = value });
            }

            // Returns true if there are no ranges, false otherwise.
            internal bool IsEmpty()
            {
                return Ranges.Count == 0;
            }

            // Returns value for level inside of range, null otherwise.
            override internal List<float> ValueAt(int level)
            {
                foreach (Range range in Ranges)
                    if (level >= range.From && level <= range.To)
                        return new List<float>(range.Value);

                return null;
            }
        }

        // Different value for each level.
        internal class LookupTable : LookupBase
        {
            // Value for each level (initialized to null).
            internal List<float>[] Values = new List<float>[MAX_LEVEL];

            // Adds value for given level to table.
            internal void Add (int level, float[] value)
            {
                Values[level - 1] = value == null ? null : new List<float>(value);
            }

            // Returns true if table is empty, false otherwise.
            internal bool IsEmpty()
            {
                foreach (List<float> value in Values)
                    if (value != null && value.Count > 0)
                        return false;

                return true;
            }

            // Returns value if it is defined for level, null otherwise.
            override internal List<float> ValueAt(int level)
            {
                return Values[level - 1];
            }
        }

        // Base deserialized value.
        public class Value
        {
            [XmlText]
            public string Text;

            // Pattern to match number.
            private static Regex ReValue = new Regex("(\\d+(\\.\\d+)?)");

            // Returns parsed value.
            internal float[] ToValue()
            {
                if (Text == null) return null;

                List<float> value = new List<float>();

                foreach (Match m in ReValue.Matches(Text))
                    value.Add(float.Parse(m.Groups[0].Value, System.Globalization.CultureInfo.InvariantCulture));

                return value.Count > 0 ? value.ToArray() : null;
            }
        }

        // Deserialized value for specific level or quality.
        public class ValueAt : Value
        {
            [XmlAttribute]
            public int Level;
            [XmlIgnore]
            public bool LevelSpecified { get { return Level != 0; } }

            [XmlAttribute]
            public int Quality;
            [XmlIgnore]
            public bool QualitySpecified { get { return Quality != 0; } }
        }

        // Deserialized value for specific range of levels.
        public class ValueForLevelRange : Value
        {
            [XmlAttribute]
            public int From;
            [XmlAttribute]
            public int To;
            [XmlIgnore]
            public bool ToSpecified { get { return To != 0; } }
        }

        // Deserialized value for specific range of qualities.
        public class ValueForQualityRange : Value
        {
            [XmlAttribute]
            public int From;
            [XmlAttribute]
            public int To;
            [XmlIgnore]
            public bool ToSpecified { get { return To != 0; } }
        }

        // Deserialized value gained each level.
        public class ValuePerLevel : Value { }

        // Deserialized value gained each quality.
        public class ValuePerQuality : Value { }

        // Deserialized gems.
        [XmlElement("Gem")]
        public Gem[] Gems;
        // Deserialized ItemDB instance.
        private static ItemDB DB;

        // Indexed gems.
        private static Dictionary<string, Gem> GemIndex = new Dictionary<string, Gem>();

        // Pattern to match +Level item modifiers.
        private static Regex ReGemLevelKeyword = new Regex("\\+# to Level of (.+) Gems in this item");

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
            if (GemIndex.ContainsKey(gem.Name))
            {
                Gem entry = GemIndex[gem.Name];

                // Process +Level modifiers from item.
                int level = 0;
                foreach (Mod mod in item.Mods)
                {
                    if (mod.Attribute == "+# to Level of Gems in this item")
                        level += (int)mod.Value[0];
                    else
                    {
                        Match m = ReGemLevelKeyword.Match(mod.Attribute);
                        if (m.Success && gem.Keywords.Contains(m.Groups[1].Value))
                            level += (int)mod.Value[0];
                    }
                }

                // Override attributes of gem (even not leveled up one).
                AttributeSet overrides = entry.AttributesAtLevel(level + LevelOf(gem));
                attrs.Override(overrides);

                // Override attributes if Quality attributes are defined.
                int quality = QualityOf(gem);
                if (quality > 0)
                {
                    overrides = entry.AttributesAtQuality(quality);
                    attrs.Override(overrides);
                }
            }

            return attrs;
        }

        // Returns attributes of gem at specified level (only for unit tests).
        public static AttributeSet AttributesOf(string name, int level)
        {
            return GemIndex[name].AttributesAtLevel(level);
        }

        // Returns true if gem can support attack skill, false otherwise.
        public static bool CanSupport(AttackSkill skill, Item gem)
        {
            if (GemIndex.ContainsKey(gem.Name))
            {
                Gem entry = GemIndex[gem.Name];

                // No support for excluded forms.
                if (entry.ExcludeFormSupport != DamageForm.Any && skill.Nature.Is(entry.ExcludeFormSupport)) return false;
            }

            return true;
        }

        // Returns true if gem can use weapon, false otherwise.
        public static bool CanUse(Item gem, Weapon weapon)
        {
            if (GemIndex.ContainsKey(gem.Name))
            {
                Gem entry = GemIndex[gem.Name];

                if (entry.RequiredHand != WeaponHand.Any && !weapon.Is(entry.RequiredHand))
                    return false;

                if (entry.RequiresEquippedShield && !Compute.IsWieldingShield)
                    return false;

                // Weapon having "Counts as Dual Wielding" mod cannot be used to perform skills that require a two-handed weapon.
                // @see http://pathofexile.gamepedia.com/Wings_of_Entropy
                if (entry.RequiredWeapon != WeaponType.Any && (entry.RequiredWeapon & WeaponType.TwoHandedMelee) != 0
                    && weapon.Attributes.ContainsKey("Counts as Dual Wielding"))
                    return false;
            }

            return true;
        }

        // Clears database.
        public static void Clear()
        {
            GemIndex.Clear();
        }

        // Returns numbner of hits skill gem does per single attack.
        public static float HitsPerAttackOf(Item gem)
        {
            return GemIndex.ContainsKey(gem.Name) ? GemIndex[gem.Name].HitsPerAttack : 1;
        }

        // Creates lookup indexed of data.
        private void Index()
        {
            if (DB.Gems != null)
            {
                foreach (Gem gem in DB.Gems)
                {
                    gem.Index();
                    GemIndex.Add(gem.Name, gem);
                }

                DB.Gems = null;
            }
        }

        // Initializes item database.
        public static void Initialize(string path)
        {
            if (File.Exists(path + @"\Items.xml"))
            {
                var serializer = new XmlSerializer(typeof(ItemDB));
                var reader = new StreamReader(path + @"\Items.xml");
                DB = (ItemDB)serializer.Deserialize(reader);
                reader.Close();

                DB.Index();
            }
        }

        // Returns true if database is empty, false otherwise.
        public static bool IsEmpty()
        {
            return GemIndex.Count == 0;
        }

        // Returns true if skill strikes with both weapons at once.
        public static bool IsStrikingWithBothWeaponsAtOnce(Item gem)
        {
            return GemIndex.ContainsKey(gem.Name) ? GemIndex[gem.Name].StrikesWithBothWeapons : false;
        }

        // Returns level of gem.
        public static int LevelOf(Item gem)
        {
            return gem.Attributes.ContainsKey("Level: #")
                   ? (int)gem.Attributes["Level: #"][0]
                   : (gem.Attributes.ContainsKey("Level: # (Max)") ? (int)gem.Attributes["Level: # (Max)"][0] : 1);
        }

        // Returns damage nature of gem.
        public static DamageNature NatureOf(Item gem)
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
            }

            return nature;
        }

        // Returns quality of gem.
        public static int QualityOf(Item gem)
        {
            return gem.Attributes.ContainsKey("Quality: +#%")
                   ? (int)gem.Attributes["Quality: +#%"][0]
                   : (gem.Attributes.ContainsKey("Quality: +#% (Max)") ? (int)gem.Attributes["Quality: +#% (Max)"][0] : 0);
        }
    }
}

/* Level of gem support:
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
 * Cleave                                       Partial
 * Decoy Totem                                  None
 * Determination                                None
 * Devouring Totem                              None
 * Dominating Blow                              Partial
 * Enduring Cry                                 None
 * Flame Totem                                  None
 * Glacial Hammer                               Partial
 * Ground Slam                                  Partial
 * Heavy Strike                                 Full
 * Herald of Ash                                None
 * Immortal Call                                None
 * Infernal Blow                                Partial
 * Leap Slam                                    Incomplete
 * Lightning Strike                             Incomplete
 * Molten Shell                                 Incomplete
 * Molten Strike                                Incomplete
 * Punishment                                   None
 * Purity of Fire                               None
 * Rejuvenation Totem                           None
 * Searing Bond                                 None
 * Shield Charge                                Partial
 * Shockwave Totem                              None
 * Sweep                                        Partial
 * Vitality                                     None
 * Warlord's Mark                               None
 * 
 * Dexterity skill gems:
 * =====================
 * Animate Weapon                               None
 * Arctic Armour                                None
 * Barrage                                      Partial
 * Bear Trap                                    None
 * Blood Rage                                   None
 * Burning Arrow                                Partial
 * Cyclone                                      Partial
 * Desecrate                                    None
 * Detonate Dead                                None
 * Double Strike                                Partial
 * Dual Strike                                  Incomplete
 * Elemental Hit                                Partial
 * Ethereal Knives                              Partial
 * Explosive Arrow                              Partial
 * Fire Trap                                    None
 * Flicker Strike                               Partial
 * Freeze Mine                                  None
 * Frenzy                                       Partial
 * Grace                                        None
 * Haste                                        None
 * Hatred                                       None
 * Herald of Ice                                Incomplete
 * Ice Shot                                     Partial
 * Lightning Arrow                              Partial
 * Poacher's Mark                               None
 * Poison Arrow                                 Partial
 * Projectile Weakness                          None
 * Puncture                                     Partial
 * Purity of Ice                                None
 * Rain of Arrows                               Partial
 * Reave                                        Incomplete
 * Smoke Mine                                   None
 * Spectral Throw                               Partial
 * Split Arrow                                  Incomplete
 * Temporal Chains                              None
 * Tornado Shot                                 Partial
 * Viper Strike                                 Partial
 * Whirling Blades                              Partial
 *
 * Intelligence skill gems:
 * ========================
 * Arc                                          Partial
 * Arctic Breath                                Partial
 * Assassin's Mark                              None
 * Ball Lightning                               Partial
 * Bone Offering                                None
 * Clarity                                      None
 * Cold Snap                                    Partial
 * Conductivity                                 None
 * Conversion Trap                              None
 * Convocation                                  None
 * Critical Weakness                            None
 * Discharge                                    None
 * Discipline                                   None
 * Elemental Weakness                           None
 * Enfeeble                                     None
 * Fireball                                     Full
 * Firestorm                                    Partial
 * Flameblast                                   Partial
 * Flame Surge                                  Partial
 * Flammability                                 None
 * Flesh Offering                               None
 * Freezing Pulse                               Partial
 * Frost Wall                                   Partial
 * Frostbite                                    None
 * Glacial Cascade                              Partial
 * Ice Nova                                     Partial
 * Ice Spear                                    Partial
 * Incinerate                                   Partial
 * Lightning Trap                               None
 * Lightning Warp                               Incomplete
 * Power Siphon                                 Incomplete
 * Purity of Elements                           None
 * Purity of Lightning                          None
 * Raise Spectre                                None
 * Raise Zombie                                 None
 * Righteous Fire                               None
 * Shock Nova                                   Partial
 * Spark                                        Partial
 * Storm Call                                   Partial
 * Summon Raging Spirit                         None
 * Summon Skeletons                             None
 * Tempest Shield                               Incomplete
 * Vulnerability                                None
 * Wrath                                        None
 * 
 * Support gems:
 * =============
 * Added Chaos Damage                           Partial
 * Added Cold Damage                            Partial
 * Added Fire Damage                            Partial
 * Added Lightning Damage                       Partial
 * Additional Accuracy                          Partial
 * Blind                                        None
 * Block Chance Reduction                       None
 * Blood Magic                                  None
 * Cast on Critical Strike                      None
 * Cast on Death                                None
 * Cast on Melee Kill                           None
 * Cast when Damage Taken                       None
 * Cast when Stunned                            None
 * Chain                                        Partial
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
 * Faster Attacks                               Partial
 * Faster Casting                               Partial
 * Faster Projectiles                           Partial
 * Fire Penetration                             None
 * Fork                                         Partial
 * Generosity                                   None
 * Greater Multiple Projectiles                 Partial
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
 * Lesser Multiple Projectiles                  Partial
 * Life Gain on Hit                             None
 * Life Leech                                   None
 * Lightning Penetration                        None
 * Mana Leech                                   None
 * Melee Damage on Full Life                    Partial
 * Melee Physical Damage                        Partial
 * Melee Splash                                 Partial
 * Minion and Totem Elemental Resistance        None
 * Minion Damage                                None
 * Minion Life                                  None
 * Minion Speed                                 None
 * Multiple Traps                               None
 * Multistrike                                  Partial
 * Physical Projectile Attack Damage            Partial
 * Pierce                                       None
 * Point Blank                                  Partial
 * Power Charge On Critical                     None
 * Ranged Attack Totem                          None
 * Reduced Duration                             None
 * Reduced Mana                                 None
 * Remote Mine                                  None
 * Slower Projectiles                           Partial
 * Spell Echo                                   Partial
 * Spell Totem                                  None
 * Stun                                         None
 * Trap                                         None
 * Weapon Elemental Damage                      Partial
 */