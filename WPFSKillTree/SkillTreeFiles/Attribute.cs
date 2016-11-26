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
using POESKillTree.Model.Items.Affixes;
using POESKillTree.Utils;
using POESKillTree.Compute;

namespace POESKillTree.SkillTreeFiles
{
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
            // Values must contain only ValueAt instances with either level or quality specified and cannot contain multivalues.
            if (Values.Exists(v => !(v is ValueAt) || v.IsMultiValue() || (v is ValueAt && !((ValueAt)v).LevelSpecified && !((ValueAt)v).QualitySpecified)))
                return;

            List<Value> optimized = new List<Value>();
            List<ValueAt> values;

            // Try per level.
            // 1) Value at level 1 must be empty, zero or not defined.
            if (Values.Exists(v => ((ValueAt)v).Level == 1 && (v.IsEmpty() || v.ToValue()[0] == 0)) || !Values.Exists(v => ((ValueAt)v).Level == 1))
            {
                // 2) At least 2 non-empty values above level 1 must exist.
                values = Values.FindAll(v => ((ValueAt)v).LevelSpecified && !v.IsEmpty() && ((ValueAt)v).Level > 1).Cast<ValueAt>().ToList();
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
            values = Values.FindAll(v => ((ValueAt)v).QualitySpecified && !v.IsEmpty()).Cast<ValueAt>().ToList();
            // 1) At least 2 non-empty values must exist.
            if (values.Count > 2)
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

            // Try for level range.
            values = Values.FindAll(v => ((ValueAt)v).LevelSpecified && !v.IsEmpty()).Cast<ValueAt>().ToList();
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
            values = Values.FindAll(v => ((ValueAt)v).QualitySpecified && !v.IsEmpty()).Cast<ValueAt>().ToList();
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
}
