using PoESkillTree.Common.Utils.Extensions;

namespace PoESkillTree.Computation.Core
{
    public struct NodeValue
    {
        // Most use cases don't need separate Minimum and Maximum values. In those cases, this behaves almost the same
        // as a standard double (but needs to be converted explicitly)
        // In the some cases, differentiation between min and max is, however, necessary. BaseSet and BaseAdd forms have
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

        public static NodeValue operator +(NodeValue left, NodeValue right) =>
            new NodeValue(left.Minimum + right.Minimum, left.Maximum + right.Maximum);

        public static NodeValue operator +(double left, NodeValue right) =>
            new NodeValue(left + right.Minimum, left + right.Maximum);

        public static NodeValue operator *(NodeValue left, NodeValue right) =>
            new NodeValue(left.Minimum * right.Minimum, left.Maximum * right.Maximum);

        public static NodeValue operator /(NodeValue left, double right) =>
            new NodeValue(left.Minimum / right, left.Maximum / right);

        public static explicit operator NodeValue(double value) => new NodeValue(value);

        public bool AlmostEquals(double value, double delta) => 
            Minimum.AlmostEquals(value, delta) && Maximum.AlmostEquals(value, delta);

        public override string ToString() => $"{Minimum} to {Maximum}";
    }
}