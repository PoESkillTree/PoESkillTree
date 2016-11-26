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
using static POESKillTree.SkillTreeFiles.GemDB;

namespace POESKillTree.SkillTreeFiles
{
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
            Ranges.Sort(delegate (Range range1, Range range2) { return range1.From.CompareTo(range2.From); });
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
        internal void Add(int level, float[] value)
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
}
