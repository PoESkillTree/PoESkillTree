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

        public static int MinWeight
        {
            get { return 1; }
        }

        public static int MaxWeight
        {
            get { return 100; }
        }

        public static int DefaultWeight
        {
            get { return 100; }
        }

        public static Func<object, string> AttributeSelectorFunc
        {
            get
            {
                return o =>
                {
                    var attributeConstraint = o as AttributeConstraint;
                    return attributeConstraint != null ? attributeConstraint.Attribute : null;
                };
            }
        } 

        public string Attribute { get; set; }

        public float TargetValue { get; set; }

        public int Weight { get; set; }
    }
}