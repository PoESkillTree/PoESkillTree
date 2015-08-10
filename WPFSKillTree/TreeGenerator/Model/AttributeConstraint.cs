using System;

namespace POESKillTree.TreeGenerator.Model
{
    public class AttributeConstraint
    {
        public AttributeConstraint()
        {
            TargetValue = 0;
            Weight = DefaultWeight;
        }

        public AttributeConstraint(string attribute)
            : this()
        {
            Attribute = attribute;
        }

        public static int MinWeight => 1;

        public static int MaxWeight => 100;

        public static int DefaultWeight => 100;

        public static Func<object, string> AttributeSelectorFunc => o => (o as AttributeConstraint)?.Attribute;

        public string Attribute { get; set; }

        public float TargetValue { get; set; }

        public int Weight { get; set; }
    }
}