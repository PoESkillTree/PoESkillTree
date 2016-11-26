using POESKillTree.Compute;
using POESKillTree.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static POESKillTree.SkillTreeFiles.GemDB;

namespace POESKillTree.SkillTreeFiles
{
    public class Gem
    {
        // Deserialized name.
        [XmlAttribute]
        public string Name;
        // Deserialized attributes.
        [XmlElement("Attribute")]
        public List<Attribute> Attributes;

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
        // Defines whether skill can be used unarmed.
        [XmlAttribute]
        public bool Unarmed = false;
        [XmlIgnore]
        public bool UnarmedSpecified { get { return Unarmed; } }

        // Skill tags (Spell, AoE, Fire, etc)
        [XmlAttribute]
        public string Tags { get; set; }

        // Returns all attributes of gem with defined values for specified level.
        internal AttributeSet AttributesAtLevel(int level)
        {
            AttributeSet attrs = new AttributeSet();

            foreach (var lookup in LevelIndex)
            {
                List<float> value = lookup.Value.ValueAt(level);
                if (value != null)
                    attrs.Add(lookup.Key, new List<float>(value));
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
                    attrs.Add(lookup.Key, new List<float>(value));
            }

            return attrs;
        }

        // Indexes attributes.
        internal void Index()
        {
            LevelIndex.Clear();
            QualityIndex.Clear();

            if (Attributes == null) return;

            foreach (Attribute attr in Attributes)
            {
                LookupTable levelTable = new LookupTable();
                LookupTable qualityTable = new LookupTable();
                LookupRanges levelRanges = new LookupRanges();
                LookupRanges qualityRanges = new LookupRanges();
                LookupGain levelGain = null;
                LookupGain qualityGain = null;
                LookupFixed fixedValue = null;

                foreach (Value value in attr.Values)
                {
                    // Ignore values which doesn't have required cardinality of attribute.
                    if (value.Cardinality != attr.Cardinality)
                        continue;

                    if (value is ValueAt)
                    {
                        ValueAt valueAt = (ValueAt)value;
                        if (valueAt.LevelSpecified)
                            levelTable.Add(valueAt.Level, valueAt.ToValue());
                        else if (valueAt.QualitySpecified)
                            qualityTable.Add(valueAt.Quality, valueAt.ToValue());
                        else
                            fixedValue = new LookupFixed { Value = valueAt.ToValue() };
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
                // LookupFixed is added to LevelIndex only (due to quality-based attributes not being defined for non-quality gems).
                LookupBase.Method method = LookupBase.Method.None;
                if (!levelTable.IsEmpty()) method |= LookupBase.Method.Table;
                if (!levelRanges.IsEmpty()) method |= LookupBase.Method.Range;
                if (levelGain != null) method |= LookupBase.Method.Gain;
                if (fixedValue != null) method |= LookupBase.Method.Fixed;
                if (method != LookupBase.Method.None && method != LookupBase.Method.Table && method != LookupBase.Method.Range && method != LookupBase.Method.Gain
                    && method != LookupBase.Method.Fixed)
                    LevelIndex.Add(attr.Name, new LookupMixed
                    {
                        Table = method.HasFlag(LookupBase.Method.Table) ? levelTable : null,
                        Ranges = method.HasFlag(LookupBase.Method.Range) ? levelRanges : null,
                        Gain = method.HasFlag(LookupBase.Method.Gain) ? levelGain : null,
                        Fixed = method.HasFlag(LookupBase.Method.Fixed) ? fixedValue : null
                    });
                else if (method.HasFlag(LookupBase.Method.Table))
                    LevelIndex.Add(attr.Name, levelTable);
                else if (method.HasFlag(LookupBase.Method.Range))
                    LevelIndex.Add(attr.Name, levelRanges);
                else if (method.HasFlag(LookupBase.Method.Gain))
                    LevelIndex.Add(attr.Name, levelGain);
                else if (method.HasFlag(LookupBase.Method.Fixed))
                    LevelIndex.Add(attr.Name, fixedValue);

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

        // Merges attributes of specified gem.
        public void Merge(Gem gem)
        {
            // Take properties from parameter if they are not specified here.
            if (!ExcludeFormSpecified && gem.ExcludeFormSpecified)
                ExcludeForm = gem.ExcludeForm;
            if (!ExcludeFormSupportSpecified && gem.ExcludeFormSupportSpecified)
                ExcludeFormSupport = gem.ExcludeFormSupport;
            if (!ExcludeSourceSpecified && gem.ExcludeSourceSpecified)
                ExcludeSource = gem.ExcludeSource;
            if (!HitsPerAttackSpecified && gem.HitsPerAttackSpecified)
                HitsPerAttack = gem.HitsPerAttack;
            if (!IncludeFormSpecified && gem.IncludeFormSpecified)
                IncludeForm = gem.IncludeForm;
            if (!RequiresEquippedShieldSpecified && gem.RequiresEquippedShieldSpecified)
                RequiresEquippedShield = gem.RequiresEquippedShield;
            if (!RequiredHandSpecified && gem.RequiredHandSpecified)
                RequiredHand = gem.RequiredHand;
            if (!RequiredWeaponSpecified && gem.RequiredWeaponSpecified)
                RequiredWeapon = gem.RequiredWeapon;
            if (!StrikesWithBothWeaponsSpecified && gem.StrikesWithBothWeaponsSpecified)
                StrikesWithBothWeapons = gem.StrikesWithBothWeapons;
            if (!UnarmedSpecified && gem.UnarmedSpecified)
                Unarmed = gem.Unarmed;

            if (gem.Attributes != null)
            {
                if (Attributes == null) Attributes = new List<Attribute>();

                foreach (Attribute attr in gem.Attributes)
                {
                    attr.Optimize();
                    // Find existing attribute to merge with.
                    Attribute with = Attributes.Find(a => a.Name == attr.Name);
                    if (with == null)
                        Attributes.Add(attr);
                    else
                    {
                        if (!with.NoUpdate)
                        {
                            // Sort values by priority so less specific values won't overwrite more specific ones.
                            attr.Values.Sort(new ValuePriorityComparer { Reversed = true });

                            foreach (Value value in attr.Values)
                                with.Merge(value);

                            with.CleanUp();
                        }
                    }
                }

                // Sort gem attribute values logically.
                Comparer<Value> comparer = new ValueLogicalComparer();
                foreach (Attribute attr in Attributes)
                    attr.Values.Sort(comparer);
            }
        }

        // Optimizes gem data.
        public void Optimize()
        {
            if (Attributes != null)
            {
                foreach (Attribute attr in Attributes)
                    attr.Optimize();
            }
        }
    }

}
