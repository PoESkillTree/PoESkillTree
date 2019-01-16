using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using log4net;
using POESKillTree.Model;
using POESKillTree.Model.Items;
using POESKillTree.Model.Items.Mods;
using POESKillTree.Utils;
using DamageForm = POESKillTree.SkillTreeFiles.Compute.DamageForm;
using DamageSource = POESKillTree.SkillTreeFiles.Compute.DamageSource;
using WeaponHand = POESKillTree.SkillTreeFiles.Compute.WeaponHand;
using WeaponType = POESKillTree.SkillTreeFiles.Compute.WeaponType;

namespace POESKillTree.SkillTreeFiles
{
    public class ItemDB
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ItemDB));

        // Maximum level (quality).
        public const int MAX_LEVEL = 40;

        // Deserialized attribute.
        public class Attribute
        {
            // Deserialized comment to perserve.
            [XmlAttribute]
            public string Comment;
            // Deserialized name.
            [XmlAttribute]
            public string Name;
            // Deserialized flag indicating whether attribute should not be updated.
            [XmlAttribute]
            public bool NoUpdate = false;
            [XmlIgnore]
            public bool NoUpdateSpecified { get { return NoUpdate; } }
            // Deserialized values.
            [XmlElement(ElementName = "Value", Type = typeof(ValueAt))]
            [XmlElement(ElementName = "ValueForLevel", Type = typeof(ValueForLevelRange))]
            [XmlElement(ElementName = "ValueForQuality", Type = typeof(ValueForQualityRange))]
            [XmlElement(ElementName = "ValuePerLevel", Type = typeof(ValuePerLevel))]
            [XmlElement(ElementName = "ValuePerQuality", Type = typeof(ValuePerQuality))]
            public List<Value> Values;

            // Cardinality (i.e. number of contained float values it should contain).
            internal int Cardinality
            {
                get
                {
                    int count = 0, pos = -1;

                    while ((pos = Name.IndexOf('#', pos + 1)) != -1)
                        count++;

                    return count;
                }
            }

            // Cleans up redundancies.
            public void CleanUp()
            {
                // Replace ValueForRange where From == To with ValueAt.
                List<Value> values = Values.FindAll(v => v is ValueForLevelRange && ((ValueForLevelRange)v).From == ((ValueForLevelRange)v).To);
                if (values.Count > 0)
                    foreach (Value value in values)
                        Replace(value, new ValueAt { Level = ((ValueForLevelRange)value).From, Text = value.Text });
                values = Values.FindAll(v => v is ValueForQualityRange && ((ValueForQualityRange)v).From == ((ValueForQualityRange)v).To);
                if (values.Count > 0)
                    foreach (Value value in values)
                        Replace(value, new ValueAt { Quality = ((ValueForQualityRange)value).From, Text = value.Text });

                // Get level-based table values and perform redundancy checks.
                LookupTable table;
                List<ValueAt> tableValues = Values.FindAll(v => v is ValueAt && ((ValueAt)v).LevelSpecified).Cast<ValueAt>().ToList();
                bool hasLevelTable = false;
                bool levelTableIsFull = false;
                if (tableValues.Count > 0)
                {
                    float[] dummy = new float[] { 1 };
                    table = new LookupTable();
                    foreach (ValueAt value in tableValues)
                        table.Add(value.Level, dummy);

                    hasLevelTable = true;
                    levelTableIsFull = table.IsFull();

                    // Remove obsolete level ranges and per level gains.
                    Values.RemoveAll(v => (v is ValueForLevelRange || v is ValuePerLevel) && table.IsCovering(v));
                }

                // Get quality-based table values and perform redundancy checks.
                tableValues = Values.FindAll(v => v is ValueAt && ((ValueAt)v).QualitySpecified).Cast<ValueAt>().ToList();
                bool qualityTableIsFull = false;
                if (tableValues.Count > 0)
                {
                    float[] dummy = new float[] { 1 };
                    table = new LookupTable();
                    foreach (ValueAt value in tableValues)
                        table.Add(value.Quality, dummy);

                    qualityTableIsFull = table.IsFull();

                    // Remove obsolete quality ranges and per quality gains.
                    Values.RemoveAll(v => v is ValueForQualityRange && table.IsCovering(v) || v is ValuePerQuality && qualityTableIsFull);
                }

                // Remove obsolete ValueAt for any level if level table is full.
                if (hasLevelTable && levelTableIsFull)
                {
                    Values.RemoveAll(v => v is ValueAt && !((ValueAt)v).LevelSpecified && !((ValueAt)v).QualitySpecified);
                }

                LookupRanges ranges;
                List<ValueForLevelRange> levelRangeValues = Values.FindAll(v => v is ValueForLevelRange).Cast<ValueForLevelRange>().ToList();
                bool hasLevelRanges = false;
                bool levelRangeIsFull = false;
                if (levelRangeValues.Count > 0)
                {
                    float[] dummy = new float[] { 1 };
                    ranges = new LookupRanges();
                    foreach (ValueForLevelRange value in levelRangeValues)
                        ranges.Add(value.From, value.To, dummy);

                    hasLevelRanges = true;
                    levelRangeIsFull = ranges.IsFull();

                    // Remove obsolete level gains.
                    Values.RemoveAll(v => v is ValuePerLevel && ranges.IsCovering(v));
                }

                List<ValueForQualityRange> qualityRangeValues = Values.FindAll(v => v is ValueForQualityRange).Cast<ValueForQualityRange>().ToList();
                bool qualityRangesIsFull = false;
                if (qualityRangeValues.Count > 0)
                {
                    float[] dummy = new float[] { 1 };
                    ranges = new LookupRanges();
                    foreach (ValueForQualityRange value in qualityRangeValues)
                        ranges.Add(value.From, value.To, dummy);

                    qualityRangesIsFull = ranges.IsFull();

                    // Remove obsolete level gains.
                    if (qualityRangesIsFull)
                        Values.RemoveAll(v => v is ValuePerQuality);
                }

                // Remove obsolete ValueAt for any level if level range is full.
                if (hasLevelRanges && levelRangeIsFull)
                    Values.RemoveAll(v => v is ValueAt && !((ValueAt)v).LevelSpecified && !((ValueAt)v).QualitySpecified);
            }

            // Merges value.
            public void Merge(Value value)
            {
                if (value is ValueAt)
                {
                    ValueAt merge = (ValueAt)value;
                    ValueAt with = (ValueAt)Values.Find(v => v is ValueAt && ((ValueAt)v).Level == merge.Level && ((ValueAt)v).Quality == merge.Quality);
                    if (with == null)
                    {
                        if (merge.LevelSpecified)
                        {
                            // No need to add ValueAt, if there is ValueForLevelRange covering specified Level with same Text.
                            ValueForLevelRange covers = (ValueForLevelRange)Values.Find(v => v is ValueForLevelRange && ((ValueForLevelRange)v).From <= merge.Level && ((ValueForLevelRange)v).To >= merge.Level && v.Text == merge.Text);
                            if (covers == null)
                                Values.Add(merge);
                        }
                        else if (merge.QualitySpecified)
                        {
                            // No need to add ValueAt, if there is ValueForQualityRange covering specified Quality with same Text.
                            ValueForQualityRange covers = (ValueForQualityRange)Values.Find(v => v is ValueForQualityRange && ((ValueForQualityRange)v).From <= merge.Quality && ((ValueForQualityRange)v).To >= merge.Quality && v.Text == merge.Text);
                            if (covers == null)
                                Values.Add(merge);
                        }
                        else
                        {
                            // Value with no level nor quality specified replaces all values.
                            Values.Clear();
                            Values.Add(merge);
                        }
                    }
                    else
                        with.Text = merge.Text;
                }
                else if (value is ValueForLevelRange)
                {
                    ValueForLevelRange merge = (ValueForLevelRange)value;
                    // Remove all ValueAt.LevelSpecified inside of range being merged and all ranges covered by it.
                    Values.RemoveAll(v => v is ValueAt && ((ValueAt)v).LevelSpecified && ((ValueAt)v).Level >= merge.From && ((ValueAt)v).Level <= merge.To
                                          || v is ValueForLevelRange && ((ValueForLevelRange)v).From >= merge.From && ((ValueForLevelRange)v).To <= merge.To);
                    // Split range covering merged one (<with.From  <merge.From  with.To>  range.To>).
                    ValueForLevelRange with = (ValueForLevelRange)Values.Find(v => v is ValueForLevelRange && ((ValueForLevelRange)v).From < merge.From && ((ValueForLevelRange)v).To > merge.To);
                    if (with != null)
                    {
                        Values.Add(new ValueForLevelRange { From = merge.To + 1, To = with.To, Text = with.Text });
                        with.To = merge.From - 1;
                    }
                    else
                    {
                        // Shorten range intersecting merged one from left (<with.From  <merge.From  with.To>  merge.To>).
                        with = (ValueForLevelRange)Values.Find(v => v is ValueForLevelRange && ((ValueForLevelRange)v).From < merge.From && ((ValueForLevelRange)v).To >= merge.From);
                        if (with != null)
                        {
                            with.To = merge.From - 1;
                            // Replace single-level shortened range with ValueAt.
                            if (with.From == with.To)
                            {
                                Values.Add(new ValueAt { Level = with.From, Text = with.Text });
                                Values.Remove(with);
                            }
                        }
                        // Shorten range intersecting merged one from right (<merge.From  <with.From  merge.To>  with.To>).
                        with = (ValueForLevelRange)Values.Find(v => v is ValueForLevelRange && ((ValueForLevelRange)v).From <= merge.To && ((ValueForLevelRange)v).To > merge.To);
                        if (with != null)
                        {
                            with.From = merge.To + 1;
                            // Replace single-level shortened range with ValueAt.
                            if (with.From == with.To)
                            {
                                Values.Add(new ValueAt { Level = with.From, Text = with.Text });
                                Values.Remove(with);
                            }
                        }
                    }
                    Values.Add(merge);
                }
                else if (value is ValueForQualityRange)
                {
                    ValueForQualityRange merge = (ValueForQualityRange)value;
                    // Remove all ValueAt.QualitySpecified inside of range being merged and all ranges covered by it.
                    Values.RemoveAll(v => v is ValueAt && ((ValueAt)v).QualitySpecified && ((ValueAt)v).Quality >= merge.From && ((ValueAt)v).Quality <= merge.To
                                          || v is ValueForQualityRange && ((ValueForQualityRange)v).From >= merge.From && ((ValueForQualityRange)v).To <= merge.To);
                    // Split range covering merged one (<with.From  <merge.From  with.To>  range.To>).
                    ValueForQualityRange with = (ValueForQualityRange)Values.Find(v => v is ValueForQualityRange && ((ValueForQualityRange)v).From < merge.From && ((ValueForQualityRange)v).To > merge.To);
                    if (with != null)
                    {
                        Values.Add(new ValueForQualityRange { From = merge.To + 1, To = with.To, Text = with.Text });
                        with.To = merge.From - 1;
                    }
                    else
                    {
                        // Shorten range intersecting merged one from left (<with.From  <merge.From  with.To>  merge.To>).
                        with = (ValueForQualityRange)Values.Find(v => v is ValueForQualityRange && ((ValueForQualityRange)v).From < merge.From && ((ValueForQualityRange)v).To >= merge.From);
                        if (with != null)
                        {
                            with.To = merge.From - 1;
                            // Replace single-quality shortened range with ValueAt.
                            if (with.From == with.To)
                            {
                                Values.Add(new ValueAt { Quality = with.From, Text = with.Text });
                                Values.Remove(with);
                            }
                        }
                        // Shorten range intersecting merged one from right (<merge.From  <with.From  merge.To>  with.To>).
                        with = (ValueForQualityRange)Values.Find(v => v is ValueForQualityRange && ((ValueForQualityRange)v).From <= merge.To && ((ValueForQualityRange)v).To > merge.To);
                        if (with != null)
                        {
                            with.From = merge.To + 1;
                            // Replace single-quality shortened range with ValueAt.
                            if (with.From == with.To)
                            {
                                Values.Add(new ValueAt { Quality = with.From, Text = with.Text });
                                Values.Remove(with);
                            }
                        }
                    }
                    Values.Add(merge);
                }
                else if (value is ValuePerLevel)
                {
                    ValuePerLevel merge = (ValuePerLevel)value;
                    ValuePerLevel with = (ValuePerLevel)Values.Find(v => v is ValuePerLevel);
                    if (with == null)
                    {
                        // Value per level replaces any level specific values and ranges.
                        Values.RemoveAll(v => (v is ValueAt && ((ValueAt)v).LevelSpecified) || v is ValueForLevelRange);
                        Values.Add(merge);
                    }
                    else
                        with.Text = merge.Text;
                }
                else if (value is ValuePerQuality)
                {
                    ValuePerQuality merge = (ValuePerQuality)value;
                    ValuePerQuality with = (ValuePerQuality)Values.Find(v => v is ValuePerQuality);
                    if (with == null)
                    {
                        // Value per quality replaces any quality specific values and ranges.
                        Values.RemoveAll(v => (v is ValueAt && ((ValueAt)v).QualitySpecified) || v is ValueForQualityRange);
                        Values.Add(merge);
                    }
                    else
                        with.Text = merge.Text;
                }
            }

            // Optimizes values.
            // Tries to convert ValueAt instances into ValuePerLevel/ValuePerQuality or ValueForLevelRange/ValueForQualityRange instances.
            public void Optimize()
            {
                // Values must contain only ValueAt instances with either level or quality specified.
                if (Values.Exists(v => !(v is ValueAt) || (v is ValueAt && !((ValueAt)v).LevelSpecified && !((ValueAt)v).QualitySpecified)))
                    return;
                var hasMultiValue = Values.Exists(v => v.IsMultiValue());

                List<Value> optimized = new List<Value>();
                List<ValueAt> values;

                // Try per level.
                // 1) Value at level 1 must be empty, zero or not defined.
                // (does not support multivalues)
                var valuesAt = Values.Cast<ValueAt>().ToList();
                if (!hasMultiValue 
                    && (valuesAt.Exists(v => v.Level == 1 && (v.IsEmpty() || v.ToValue()[0] == 0)) || !valuesAt.Exists(v => v.Level == 1)))
                {
                    // 2) At least 2 non-empty values above level 1 must exist.
                    values = valuesAt.FindAll(v => v.LevelSpecified && !v.IsEmpty() && v.Level > 1).ToList();
                    if (values.Count > 2)
                    {
                        // 3) All values must have same value to level ratio (level - 1 actually).
                        float gain = values[0].ToValue()[0] / (values[0].Level - 1);
                        bool isGain = true;
                        foreach (ValueAt value in values)
                            if (value.ToValue()[0] / (value.Level - 1) != gain)
                            {
                                isGain = false;
                                break;
                            }

                        if (isGain)
                        {
                            // Remove all values with level specified.
                            Values.RemoveAll(v => ((ValueAt)v).LevelSpecified);
                            optimized.Add(new ValuePerLevel { Text = gain.ToString(System.Globalization.CultureInfo.InvariantCulture) });
                        }
                    }
                }

                // Try per quality.
                values = Values.Cast<ValueAt>().Where(v => v.QualitySpecified && !v.IsEmpty()).ToList();
                // 1) At least 2 non-empty values must exist.
                // (does not support multivalues)
                if (!hasMultiValue && values.Count > 2)
                {
                    // 2) All values must have same value to quality ratio.
                    float gain = values[0].ToValue()[0] / values[0].Quality;
                    bool isGain = true;
                    foreach (ValueAt value in values)
                        if (value.ToValue()[0] / value.Quality != gain)
                        {
                            isGain = false;
                            break;
                        }

                    if (isGain)
                    {
                        // Remove all values with quality specified.
                        Values.RemoveAll(v => ((ValueAt)v).QualitySpecified);
                        optimized.Add(new ValuePerQuality { Text = gain.ToString(System.Globalization.CultureInfo.InvariantCulture) });
                    }
                }
                else if (values.Count > 2)
                {
                    // All values must have the same text
                    var firstText = values[0].Text;
                    if (values.Skip(1).All(v => v.Text == firstText))
                    {
                        Values.RemoveAll(v => ((ValueAt) v).QualitySpecified);
                        optimized.Add(new ValuePerQuality { Text = firstText });
                    }
                }

                // Try for level range.
                values = Values.Cast<ValueAt>().Where(v => v.LevelSpecified && !v.QualitySpecified).ToList();
                if (values.Count > 2) // Don't convert 2 or less values.
                {
                    values.Sort(new ValueLogicalComparer());

                    List<ValueForLevelRange> ranges = new List<ValueForLevelRange>();

                    int from = 0, to = 0;
                    string text = null;
                    foreach (ValueAt value in values)
                    {
                        if (value.Text == text)
                            to = value.Level;
                        else
                        {
                            if (text != null && from != 0 && to > from)
                                ranges.Add(new ValueForLevelRange { From = from, To = to, Text = text });

                            from = to = value.Level;
                            text = value.Text;
                        }
                    }
                    if (text != null && from != 0 && to > from)
                        ranges.Add(new ValueForLevelRange { From = from, To = to, Text = text });

                    if (ranges.Count > 0)
                    {
                        // Remove all values with level inside optimized ranges.
                        foreach (ValueForLevelRange range in ranges)
                            Values.RemoveAll(v => ((ValueAt)v).LevelSpecified && ((ValueAt)v).Level >= range.From && ((ValueAt)v).Level <= range.To);
                        optimized.AddRange(ranges);
                    }
                }

                // Try for quality range.
                values = Values.Cast<ValueAt>().Where(v => !v.LevelSpecified && v.QualitySpecified).ToList();
                if (values.Count > 2) // Don't convert 2 or less values.
                {
                    values.Sort(new ValueLogicalComparer());

                    List<ValueForQualityRange> ranges = new List<ValueForQualityRange>();

                    int from = 0, to = 0;
                    string text = null;
                    foreach (ValueAt value in values)
                    {
                        if (value.Text == text)
                            to = value.Quality;
                        else
                        {
                            if (text != null && from != 0 && to > from)
                                ranges.Add(new ValueForQualityRange { From = from, To = to, Text = text });

                            from = to = value.Quality;
                            text = value.Text;
                        }
                    }
                    if (text != null && from != 0 && to > from)
                        ranges.Add(new ValueForQualityRange { From = from, To = to, Text = text });

                    if (ranges.Count > 0)
                    {
                        // Remove all values with quality inside optimized ranges.
                        foreach (ValueForQualityRange range in ranges)
                            Values.RemoveAll(v => ((ValueAt)v).QualitySpecified && ((ValueAt)v).Quality >= range.From && ((ValueAt)v).Quality <= range.To);
                        optimized.AddRange(ranges);
                    }
                }

                if (optimized.Count > 0)
                    Values.AddRange(optimized);
            }

            // Replaces value instance with another.
            public void Replace(Value value, Value replace)
            {
                int index = Values.IndexOf(value);
                if (index > -1)
                {
                    Values.RemoveAt(index);
                    Values.Insert(index, replace);
                }
            }
        }

        // The deserialized gem and gem index entry.
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

        // Base class for all lookup methods.
        abstract internal class LookupBase
        {
            // Available methods.
            [Flags]
            internal enum Method { None, Fixed = 1, Gain = 2, Range = 4, Table = 8 };

            // Implementation in derived class must return either defined value(s) or null if value isn't defined for specified level.
            abstract internal List<float> ValueAt(int level);
        }

        // Fixed value.
        internal class LookupFixed : LookupBase
        {
            internal float[] Value;

            // Returns fixed value regardless of level.
            override internal List<float> ValueAt(int level)
            {
                return new List<float>(Value);
            }
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
            internal LookupFixed Fixed;
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

                if (Gain != null)
                    return Gain.ValueAt(level);
                else if (Fixed != null)
                    return Fixed.ValueAt(level);

                return null;
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
                Ranges.Sort(delegate(Range range1, Range range2) { return range1.From.CompareTo(range2.From); });
            }

            // Returns true if ranges are covering value, false otherwise.
            internal bool IsCovering(Value value)
            {
                int from = 1, to = MAX_LEVEL;

                if (value is ValueAt)
                {
                    ValueAt at = (ValueAt)value;
                    if (at.LevelSpecified)
                        from = to = at.Level;
                    else if (at.QualitySpecified)
                        from = to = at.Quality;
                    // Else keep from = 1 and to = MAX_LEVEL.
                }
                else if (value is ValueForLevelRange)
                {
                    ValueForLevelRange range = (ValueForLevelRange)value;
                    from = range.From;
                    if (range.ToSpecified) to = range.To;
                }
                else if (value is ValueForQualityRange)
                {
                    ValueForQualityRange range = (ValueForQualityRange)value;
                    from = range.From;
                    if (range.ToSpecified) to = range.To;
                }
                else if (value is ValuePerLevel)
                    from = 2;
                // Else keep from = 1 and to = MAX_LEVEL.

                // Traverse all ranges and move 'from' to end of range when 'from' is inside of that range.
                foreach (Range range in Ranges)
                    if (from >= range.From && from <= range.To)
                        from = range.To;

                // A <from, to> range is covered by our ranges when 'from' reached 'to'.
                return from >= to;
            }

            // Returns true if there are no ranges, false otherwise.
            internal bool IsEmpty()
            {
                return Ranges.Count == 0;
            }

            // Returns true if ranges are complete (i.e. contains values from level 1 to MAX_LEVEL), false otherwise.
            internal bool IsFull()
            {
                int from = 1, to = MAX_LEVEL;

                foreach (Range range in Ranges)
                    if (from >= range.From && from <= range.To)
                        from = range.To;

                return from >= to;
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

            // Returns true if table defines all values as specified value.
            internal bool IsCovering(Value value)
            {
                int from = 1, to = MAX_LEVEL;

                if (value is ValueAt)
                {
                    ValueAt at = (ValueAt)value;
                    if (at.LevelSpecified)
                        from = to = at.Level;
                    else if (at.QualitySpecified)
                        from = to = at.Quality;
                    // Else keep from = 1 and to = MAX_LEVEL.
                }
                else if (value is ValueForLevelRange)
                {
                    ValueForLevelRange range = (ValueForLevelRange)value;
                    from = range.From;
                    if (range.ToSpecified) to = range.To;
                }
                else if (value is ValueForQualityRange)
                {
                    ValueForQualityRange range = (ValueForQualityRange)value;
                    from = range.From;
                    if (range.ToSpecified) to = range.To;
                }
                else if (value is ValuePerLevel) // ValuePerLevel doesn't define value for level 1.
                    from = 2;
                // Else keep from = 1 and to = MAX_LEVEL.

                for (int level = from - 1; level < to; ++level)
                    if (Values[level] == null)
                        return false;

                return true;
            }

            // Returns true if table is empty, false otherwise.
            internal bool IsEmpty()
            {
                foreach (List<float> value in Values)
                    if (value != null)
                        return false;

                return true;
            }

            // Returns true if table is full (i.e. contains values from level 1 to MAX_LEVEL), false otherwise.
            internal bool IsFull()
            {
                foreach (List<float> value in Values)
                    if (value == null)
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
        public abstract class Value
        {
            [XmlText]
            public string Text;

            // Cardinality (i.e. number of float values it contains).
            internal int Cardinality
            {
                get
                {
                    return ReValue.Matches(Text).Count;
                }
            }

            // Priority.
            abstract internal ValuePriority Priority { get; }

            // Pattern to match number.
            private static Regex ReValue = new Regex("(-?\\d+(\\.\\d+)?)");

            // Returns true if value is empty, false otherwise.
            public bool IsEmpty()
            {
                return Text == null || ReValue.Matches(Text).Count == 0;
            }

            // Returns true if value is multivalue (i.e. contains more than 1 number), false otherwise.
            public bool IsMultiValue()
            {
                return ReValue.Matches(Text).Count > 1;
            }

            // Returns parsed value.
            internal float[] ToValue()
            {
                return Text == null ? null : Parse(Text);
            }

            public static float[] Parse(string text)
            {
                List<float> value = new List<float>();

                foreach (Match m in ReValue.Matches(text))
                    value.Add(float.Parse(m.Groups[0].Value, System.Globalization.CultureInfo.InvariantCulture));

                return value.Count > 0 ? value.ToArray() : new float[0];
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

            // Priority.
            override internal ValuePriority Priority { get { return Level == 0 && Quality == 0 ? ValuePriority.AtAnyLevelQuality : ValuePriority.AtLevelQuality; } }
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

            // Priority.
            override internal ValuePriority Priority { get { return ValuePriority.ForLevelQualityRange; } }
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

            // Priority.
            override internal ValuePriority Priority { get { return ValuePriority.ForLevelQualityRange; } }
        }

        // Deserialized value gained each level.
        public class ValuePerLevel : Value
        {
            // Priority.
            override internal ValuePriority Priority { get { return ValuePriority.PerLevelQualityGain; } }
        }

        // Deserialized value gained each quality.
        public class ValuePerQuality : Value
        {
            // Priority.
            override internal ValuePriority Priority { get { return ValuePriority.PerLevelQualityGain; } }
        }

        // Comparer for sorting deserialized values in logical manner (i.e. ValueAt { Level=5 } > ValueForLevelRange { From=1, To=4 }).
        internal class ValueLogicalComparer : Comparer<Value>
        {
            override public int Compare(Value x, Value y)
            {
                if (x is ValueAt)
                {
                    ValueAt xAt = (ValueAt)x;
                    if (y is ValueAt)
                    {
                        ValueAt yAt = (ValueAt)y;
                        if (xAt.LevelSpecified)
                            return yAt.LevelSpecified ? xAt.Level.CompareTo(yAt.Level) : -1; // x.Level specified < y.Quality specified/unspecified
                        else if (xAt.QualitySpecified)
                            return yAt.QualitySpecified ? xAt.Quality.CompareTo(yAt.Quality) : (yAt.LevelSpecified ? 1 : -1); // x.Quality specified > y.Level specified, x.Quality specified < y.Level unspecified

                        return yAt.LevelSpecified || yAt.QualitySpecified ? 1 : 0; // x.Level & x.Quality unspecified > y.Level/Quality specified
                    }
                    else if (y is ValueForLevelRange)
                    {
                        ValueForLevelRange yRange = (ValueForLevelRange)y;
                        if (xAt.LevelSpecified && yRange.ToSpecified && xAt.Level > yRange.To) // x.Level vs. y.To => x > y
                            return 1;
                        // Fall through (x < y)
                    }
                    else if (y is ValueForQualityRange)
                    {
                        ValueForQualityRange yRange = (ValueForQualityRange)y;
                        if (xAt.QualitySpecified && yRange.ToSpecified && xAt.Quality > yRange.To) // x.Quality vs. y.To => x > y
                            return 1;
                        // Fall through (x < y)
                    }

                    return -1;
                }
                else if (x is ValueForLevelRange)
                {
                    ValueForLevelRange xRange = (ValueForLevelRange)x;
                    if (y is ValueForLevelRange)
                    {
                        ValueForLevelRange yRange = (ValueForLevelRange)y;
                        if (xRange.From == yRange.From)
                        {
                            if (xRange.ToSpecified)
                                return yRange.ToSpecified ? xRange.To.CompareTo(yRange.To) : -1; // x.To specified < y.To unspecified
                            else
                                return yRange.ToSpecified ? 1 : 0; // x.To unspecified > y.To specified
                        }

                        return xRange.From < yRange.From ? -1 : 1;
                    }
                    else if (y is ValueAt)
                    {
                        ValueAt yAt = (ValueAt)y;
                        if (yAt.LevelSpecified && xRange.To < yAt.Level) // x.To vs. y.Level => x < y
                            return -1;
                        // Fall through (x > y).
                    }

                    return y is ValueAt ? 1 : -1;
                }
                else if (x is ValueForQualityRange)
                {
                    ValueForQualityRange xRange = (ValueForQualityRange)x;
                    if (y is ValueForQualityRange)
                    {
                        ValueForQualityRange yRange = (ValueForQualityRange)y;
                        if (xRange.From == yRange.From)
                        {
                            if (xRange.ToSpecified)
                                return yRange.ToSpecified ? xRange.To.CompareTo(yRange.To) : -1; // x.To specified < y.To unspecified
                            else
                                return yRange.ToSpecified ? 1 : 0; // x.To unspecified > y.To specified
                        }

                        return xRange.From < yRange.From ? -1 : 1;
                    }
                    else if (y is ValueAt)
                    {
                        ValueAt yAt = (ValueAt)y;
                        if (yAt.QualitySpecified && xRange.To < yAt.Quality) // x.To vs. y.Quality => x < y
                            return -1;
                        // Fall through (x > y).
                    }

                    return y is ValueAt || y is ValueForLevelRange ? 1 : -1;
                }
                else if (x is ValuePerLevel)
                {
                    return y is ValuePerLevel ? 0 : (y is ValuePerQuality ? -1 : 1);
                }
                else if (x is ValuePerQuality)
                {
                    return y is ValuePerQuality ? 0 : 1;
                }

                return 0;
            }
        }

        // Priority of value class.
        internal enum ValuePriority { AtLevelQuality, ForLevelQualityRange, PerLevelQualityGain, AtAnyLevelQuality }

        // Comparer for sorting deserialized values according to priority (i.e. ValueAt is always less than ValueForLevelRange regardless of level).
        internal class ValuePriorityComparer : Comparer<Value>
        {
            internal bool Reversed = false;

            override public int Compare(Value x, Value y)
            {
                return Reversed
                       ? (x.Priority == y.Priority ? 0 : (x.Priority < y.Priority ? 1 : -1))
                       : (x.Priority == y.Priority ? 0 : (x.Priority < y.Priority ? -1 : 1));
            }
        }

        // Deserialized gems.
        [XmlElement("Gem")]
        public List<Gem> Gems;
        // Deserialized ItemDB instance.
        private static ItemDB DB;

        // Indexed gems.
        private static Dictionary<string, Gem> GemIndex = new Dictionary<string, Gem>();

        // Pattern to match +Level item modifiers.
        private static Regex ReGemLevelKeyword = new Regex(@"\+# to Level of Socketed (.+) Gems");

        // Adds gem to deserialized gems.
        public static void Add(Gem gem)
        {
            if (DB == null) Create();

            if (! DB.Gems.Exists(g => g.Name == gem.Name))
            {
                gem.Optimize();

                if (gem.Attributes != null)
                {
                    Comparer<Value> comparer = new ValueLogicalComparer();
                    foreach (Attribute attr in gem.Attributes)
                        attr.Values.Sort(comparer);
                }

                DB.Gems.Add(gem);
            }
        }

        // Returns attributes of gem at specified level and quality.
        public static AttributeSet AttributesOf(string gemName, int level, int quality)
        {
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

        // Creates empty database.
        private static void Create()
        {
            DB = new ItemDB { Gems = new List<Gem>() };
        }

        // Returns deserialized gems.
        public static List<Gem> GetAllGems()
        {
            return DB == null ? null : DB.Gems;
        }

        // Indexes items in database.
        public static void Index()
        {
            GemIndex.Clear();

            if (DB != null && DB.Gems != null)
            {
                foreach (Gem gem in DB.Gems)
                {
                    gem.Index();
                    GemIndex.Add(gem.Name, gem);
                }
            }
        }

        // Returns true if database is empty, false otherwise.
        public static bool IsEmpty()
        {
            return GemIndex.Count == 0;
        }

        // Returns level of gem.
        public static int LevelOf(Item gem)
        {
            float ret;
            if (gem.Properties.TryGetValue("Level: #", 0, out ret))
                return (int) ret;
            else
                return (int) gem.Properties.First("Level: # (Max)", 0, 1);
        }

        // Loads items from XML file.
        public static void Load(string file, bool index = false)
        {
            LoadFromCompletePath(AppData.GetFolder(true) + file, index);
        }
        public static void LoadFromCompletePath(string file, bool index = false)
        {
            if (!File.Exists(file))
            {
                Log.WarnFormat("File {0} does not exist.", file);
                if (index) Index();
                return;
            }
            var serializer = new XmlSerializer(typeof(ItemDB));
            var reader = new StreamReader(file);
            DB = (ItemDB)serializer.Deserialize(reader);
            reader.Close();

            if (index) Index();
        }

        // Merges items from XML file.
        public static void Merge(string file)
        {
            MergeFromCompletePath(AppData.GetFolder(true) + file);
        }
        public static void MergeFromCompletePath(string file)
        {
            if (File.Exists(file))
            {
                var serializer = new XmlSerializer(typeof(ItemDB));
                var reader = new StreamReader(file);
                ItemDB merge = (ItemDB)serializer.Deserialize(reader);
                reader.Close();

                if (merge != null)
                {
                    if (DB == null) DB = merge;
                    else
                    {
                        // Merge gems.
                        if (merge.Gems != null)
                        {
                            foreach (Gem gem in merge.Gems)
                            {
                                gem.Optimize();

                                Gem with = DB.Gems.Find(g => g.Name == gem.Name);
                                if (with == null)
                                    DB.Gems.Add(gem);
                                else
                                    with.Merge(gem);
                            }
                        }
                    }
                }
            }
        }

        // Returns quality of gem.
        public static int QualityOf(Item gem)
        {
            return (int) gem.Properties.First("Quality: +#%", 0, 0);
        }

        public static void WriteToCompletePath(string file)
        {
            // Sort gems alphabetically.
            DB.Gems.Sort(delegate (Gem gem1, Gem gem2) { return String.Compare(gem1.Name, gem2.Name, true, System.Globalization.CultureInfo.InvariantCulture); });

            var serializer = new XmlSerializer(typeof(ItemDB));
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineChars = "\n";
            XmlWriter writer = XmlTextWriter.Create(file, settings);
            serializer.Serialize(writer, DB);
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