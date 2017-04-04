using System;
using System.Globalization;
using System.Linq;

namespace MB.Algodat
{
    using CSharpGlobalCode.GlobalCode_ExperimentalCode;
    /// <summary>
    /// Represents a range of values. 
    /// Both values must be of the same type and comparable.
    /// </summary>
    /// <typeparam name="T">Type of the values.</typeparam>
    public struct Range<T> : IComparable<Range<T>> where T : IComparable<T>
    {
        public T From;
        public T To;

        /// <summary>
        /// Initializes a new <see cref="Range&lt;T&gt;"/> instance.
        /// </summary>
        public Range(T value): this()
        {
            From = value;
            To = value;
        }

        /// <summary>
        /// Initializes a new <see cref="Range&lt;T&gt;"/> instance.
        /// </summary>
        public Range(T from, T to): this()
        {
            From = from;
            To = to;
        }

        /// <summary>
        /// Whether the value is contained in the range. 
        /// Border values are considered inside.
        /// </summary>
        public bool Contains(T value)
        {
            return value.CompareTo(From) >= 0 && value.CompareTo(To) <= 0;
        }

        /// <summary>
        /// Whether the value is contained in the range. 
        /// Border values are considered outside.
        /// </summary>
        public bool ContainsExclusive(T value)
        {
            return value.CompareTo(From) > 0 && value.CompareTo(To) < 0;
        }

        /// <summary>
        /// Whether two ranges intersect each other.
        /// </summary>
        public bool Intersects(Range<T> other)
        {
            return other.To.CompareTo(From) >= 0 && other.From.CompareTo(To) <= 0;
        }

        /// <summary>
        /// Whether two ranges intersect each other.
        /// </summary>
        public bool IntersectsExclusive(Range<T> other)
        {
            return other.To.CompareTo(From) > 0 && other.From.CompareTo(To) < 0;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (From is SmallDec)
            {
                return string.Format("{0} - {1}", ((SmallDec)((object)From)).ToOptimalString(), ((SmallDec)((object)To)).ToOptimalString());
            }
            else
            {
                return string.Format("{0} - {1}", From, To);
            }
        }

        public override int GetHashCode()
        {
            int hash = 23;
            hash = hash * 37 + From.GetHashCode();
            hash = hash * 37 + To.GetHashCode();
            return hash;
        }

        #region IComparable<Range<T>> Members

        /// <summary>
        /// Returns -1 if this range's From is less than the other, 1 if greater.
        /// If both are equal, To is compared, 1 if greater, -1 if less.
        /// 0 if both ranges are equal.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public int CompareTo(Range<T> other)
        {
            if (From.CompareTo(other.From) < 0)
                return -1;
            else if (From.CompareTo(other.From) > 0)
                return 1;
            else if (To.CompareTo(other.To) < 0)
                return -1;
            else if (To.CompareTo(other.To) > 0)
                return 1;
            else
                return 0;
        }

        #endregion


        public static Range<T> Parse(string p, IFormatProvider provider = null)
        {
            T TTypeTest = default(T);
            provider = provider ?? CultureInfo.CurrentCulture;
            if (((object)TTypeTest) is SmallDec)
            {
                try
                {
                    var parts = p.Split(new[] { " to " }, StringSplitOptions.RemoveEmptyEntries).Select(pp => pp).ToArray();
                    if (parts.Length < 1 || parts.Length > 2)
                        throw new ArgumentException(string.Format("cannot parse given range ({0}), it doesn't have one or two parts", p));

                    if (parts.Length == 1)
                    {
                        return new Range<T>((T)(Object)((SmallDec)parts[0]));
                    }
                    else
                    {
                        return new Range<T>((T)(Object)((SmallDec)parts[0]), (T)(Object)((SmallDec)parts[1]));
                    }
                }
                catch (System.Exception ex)
                {
                    System.Console.WriteLine("Range Parse Exception of " + ex.ToString());
                }
                return new Range<T>((T)(Object)((SmallDec.Zero)));//Force return empty range if has exception so can see the error on debugging instead of it crashing without seeing it in console
            }
            else
            {
                var parts = p.Split(new[] { " to " }, StringSplitOptions.RemoveEmptyEntries)
                .Select(pp => (T)Convert.ChangeType(pp, typeof(T), provider))
                .ToArray();

                if (parts.Length < 1 || parts.Length > 2)
                    throw new ArgumentException(string.Format("cannot parse given range ({0}), it doesn't have one or two parts", p));

                if (parts.Length == 1)
                {
                    return new Range<T>(parts[0]);
                }
                else
                {
                    return new Range<T>(parts[0], parts[1]);
                }
            }
        }
    }

    /// <summary>
    /// Static helper class to create Range instances.
    /// </summary>
    public static class Range
    {
        /// <summary>
        /// Creates and returns a new <see cref="Range&lt;T&gt;"/> instance.
        /// </summary>
        public static Range<T> Create<T>(T from, T to)where T : IComparable<T>
        {
            return new Range<T>(from, to);
        }
    }

    /// <summary>
    /// Interface for classes which provide a range.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRangeProvider<T> where T : IComparable<T>
    {
        Range<T> Range { get; }
    }
}