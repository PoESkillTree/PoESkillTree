using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Utils.Extensions;

namespace PoESkillTree.Computation.Common
{
    public struct NodeValue : IEquatable<NodeValue>
    {
        // Most use cases don't need separate Minimum and Maximum values. In those cases, this behaves almost the same
        // as a standard double (but needs to be converted explicitly)
        // In some cases, differentiation between min and max is, however, necessary. BaseSet and BaseAdd forms have
        // variants for min and max values, in which case only one value is modified. These different values can
        // propagate all the way through the stat subgraph (and to other nodes referencing them).

        public NodeValue(double value)
        {
            Minimum = value;
            Maximum = value;
        }

        public NodeValue(double minimum, double maximum)
        {
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
            Minimum.Equals(other.Minimum) && Maximum.Equals(other.Maximum);

        public bool AlmostEquals(double value, double delta = 1e-10) =>
            Minimum.AlmostEquals(value, delta) && Maximum.AlmostEquals(value, delta);

        public override int GetHashCode() =>
            (Minimum, Maximum).GetHashCode();

        public static bool operator <(NodeValue left, double right) =>
            left.Maximum < right;

        public static bool operator >(NodeValue left, double right) =>
            left.Minimum > right;
        
        public static bool operator <=(NodeValue left, double right) =>
            left.Maximum <= right;

        public static bool operator >=(NodeValue left, double right) =>
            left.Minimum >= right;


        public static explicit operator NodeValue(double value) => new NodeValue(value);

        public NodeValue Clip(double minValue, double maxValue) =>
            Select(d => Math.Max(Math.Min(d, maxValue), minValue));

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

        public static NodeValue Combine(NodeValue left, NodeValue right, Func<double, double, double> operation) =>
            new NodeValue(operation(left.Minimum, right.Minimum), operation(left.Maximum, right.Maximum));


        public override string ToString() => $"{Minimum} to {Maximum}";
    }


    public static class NodeValueExtensions
    {
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