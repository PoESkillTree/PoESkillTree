using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Utils.Extensions;

namespace PoESkillTree.Computation.Common
{
    /// <summary>
    /// Represents a value in the calculation graph. Consists of a minimum and a maximum <see cref="double"/> value.
    /// <para>
    /// Overloads operators and provides methods to allow for almost as simple usage as when just using a
    /// <see cref="double"/>.
    /// </para>
    /// <para>
    /// Converting from <see cref="double"/> is possible through constructors or an explicit
    /// conversion operator. Converting to a single <see cref="double"/> is not possible (except simply using either
    /// <see cref="Minimum"/> or <see cref="Maximum"/>. <see cref="NodeValue"/> can be compared directly to
    /// <see cref="double"/>s.
    /// </para>
    /// <para>
    /// Comparison operators are overloaded assuming <see cref="Minimum"/> and <see cref="Maximum"/> define a range
    /// of possible values. I.e. they only return true if the comparison is true for all possible values, not if
    /// the comparison has the possibility to be true.
    /// </para>
    /// </summary>
    /// <remarks>
    /// The differentiation between min and max values is necessary, e.g. for BaseSet and BaseAdd damage values.
    /// These will propagate through the whole graph, making min and max values in general necessary.
    /// <para>
    /// The operator overloads currently provided are not exhausting, they are only what is actually used. Just add
    /// more operators if the need arises.
    /// </para>
    /// </remarks>
    public struct NodeValue : IEquatable<NodeValue>
    {
        public NodeValue(double value)
        {
            Minimum = value;
            Maximum = value;
        }

        public NodeValue(double minimum, double maximum)
        {
            if (minimum > maximum)
                throw new ArgumentException($"Minimum must be <= maximum, was {minimum} > {maximum}");

            Minimum = minimum;
            Maximum = maximum;
        }

        public double Minimum { get; }

        public double Maximum { get; }


        public static bool operator ==(NodeValue left, NodeValue right) =>
            left.Equals(right);

        public static bool operator !=(NodeValue left, NodeValue right) =>
            !left.Equals(right);

        public override bool Equals(object obj) =>
            obj is NodeValue other && Equals(other);

        public bool Equals(NodeValue other) =>
            Minimum.AlmostEquals(other.Minimum, 1e-10) && Maximum.AlmostEquals(other.Maximum, 1e-10);
        
        public static bool operator ==(NodeValue left, double right) =>
            left.Equals(right);

        public static bool operator !=(NodeValue left, double right) =>
            !left.Equals(right);

        public bool Equals(double value) =>
            Minimum.AlmostEquals(value, 1e-10) && Maximum.AlmostEquals(value, 1e-10);

        public override int GetHashCode() =>
            (Minimum, Maximum).GetHashCode();

        public static bool operator <(NodeValue left, NodeValue right) =>
            left.Maximum < right.Minimum;

        public static bool operator >(NodeValue left, NodeValue right) =>
            left.Minimum > right.Maximum;
        
        public static bool operator <=(NodeValue left, NodeValue right) =>
            left.Maximum <= right.Minimum;

        public static bool operator >=(NodeValue left, NodeValue right) =>
            left.Minimum >= right.Maximum;

        public static bool operator <(NodeValue left, double right) =>
            left.Maximum < right;

        public static bool operator >(NodeValue left, double right) =>
            left.Minimum > right;
        
        public static bool operator <=(NodeValue left, double right) =>
            left.Maximum <= right;

        public static bool operator >=(NodeValue left, double right) =>
            left.Minimum >= right;


        public static explicit operator NodeValue(double value) => new NodeValue(value);

        /// <summary>
        /// Returns a value that is at least <paramref name="minValue"/> and at most <paramref name="maxValue"/>.
        /// </summary>
        public NodeValue Clip(double minValue, double maxValue) =>
            Select(d => Math.Max(Math.Min(d, maxValue), minValue));

        /// <summary>
        /// Returns the value created by applying <paramref name="operation"/> to <see cref="Minimum"/> and
        /// <see cref="Maximum"/>.
        /// </summary>
        public NodeValue Select(Func<double, double> operation) =>
            new NodeValue(operation(Minimum), operation(Maximum));


        public static NodeValue operator +(NodeValue left, NodeValue right) =>
            new NodeValue(left.Minimum + right.Minimum, left.Maximum + right.Maximum);

        public static NodeValue operator +(double left, NodeValue right) =>
            new NodeValue(left) + right;

        public static NodeValue operator -(NodeValue left, NodeValue right) =>
            new NodeValue(left.Minimum - right.Minimum, left.Maximum - right.Maximum);

        public static NodeValue operator -(double left, NodeValue right) =>
            new NodeValue(left) - right;

        public static NodeValue operator *(NodeValue left, NodeValue right) =>
            new NodeValue(left.Minimum * right.Minimum, left.Maximum * right.Maximum);

        public static NodeValue operator *(double left, NodeValue right) =>
            new NodeValue(left) * right;

        public static NodeValue operator *(NodeValue left, double right) =>
            left * new NodeValue(right);

        public static NodeValue operator /(NodeValue left, NodeValue right) =>
            new NodeValue(left.Minimum / right.Minimum, left.Maximum / right.Maximum);

        public static NodeValue operator /(NodeValue left, double right) =>
            left / new NodeValue(right);

        /// <summary>
        /// Returns the value created by combining <paramref name="left"/> and <paramref name="right"/> using
        /// <paramref name="operation"/>.
        /// </summary>
        public static NodeValue Combine(NodeValue left, NodeValue right, Func<double, double, double> operation) =>
            new NodeValue(operation(left.Minimum, right.Minimum), operation(left.Maximum, right.Maximum));


        public override string ToString() => $"{Minimum} to {Maximum}";
    }


    public static class NodeValueExtensions
    {
        /// <summary>
        /// Returns the value created by applying <paramref name="operation"/> to <paramref name="value"/> if
        /// <paramref name="value"/> is not <c>null</c>. Returns <c>null</c> otherwise.
        /// </summary>
        public static NodeValue? Select(this NodeValue? value, Func<double, double> operation) =>
            value.Select(v => v.Select(operation));

        public static NodeValue Sum(this IEnumerable<NodeValue> values) =>
            values.Aggregate((l, r) => l + r);

        public static NodeValue? Sum(this IEnumerable<NodeValue?> values) =>
            values.AggregateOnValues((l, r) => l + r);

        public static NodeValue Product(this IEnumerable<NodeValue> values) =>
            values.Aggregate((l, r) => l * r);

        public static NodeValue? Product(this IEnumerable<NodeValue?> values) =>
            values.AggregateOnValues((l, r) => l * r);
    }
}