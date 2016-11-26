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
        private static Regex ReValue = new Regex("(\\d+(\\.\\d+)?)");

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

}
