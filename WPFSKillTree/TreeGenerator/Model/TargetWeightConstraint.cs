using System;
using POESKillTree.TreeGenerator.Model.PseudoAttributes;
using POESKillTree.Utils;

namespace POESKillTree.TreeGenerator.Model
{
    /// <summary>
    /// Abstract data class for Constraints with a data object, a target value and a weight.
    /// </summary>
    /// <typeparam name="T">Type of the stored data object.</typeparam>
    public abstract class TargetWeightConstraint<T> : Notifier
    {
        /// <summary>
        /// Minimum allowed weight (inclusive).
        /// </summary>
        public static int MinWeight
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximim allowed weight (inclusive).
        /// </summary>
        public static int MaxWeight
        {
            get { return 100; }
        }
        
        private static int DefaultWeight
        {
            get { return 100; }
        }

        private T _data;

        public T Data
        {
            get { return _data; }
            set { SetProperty(ref _data, value); }
        }

        private float _targetValue;

        public float TargetValue
        {
            get { return _targetValue; }
            set { SetProperty(ref _targetValue, value); }
        }

        private int _weight = DefaultWeight;

        public int Weight
        {
            get { return _weight; }
            set
            {
                if (value < MinWeight || value > MaxWeight)
                    throw new ArgumentOutOfRangeException("value", value, "must be between MinWeight and MaxWeight");
                SetProperty(ref _weight, value);
            }
        }
        
        protected TargetWeightConstraint(T data = default(T))
        {
            Data = data;
        }
    }

    /// <summary>
    /// Data class for Constraint that contain a string describing the constrained attribute,
    /// a target value and a weight.
    /// </summary>
    public class AttributeConstraint : TargetWeightConstraint<string>
    {

        // ReSharper disable once UnusedMember.Global
        // No parameter constructor is necessary for DataGrid to enable user adding rows.
        public AttributeConstraint()
        { }

        public AttributeConstraint(string attribute)
            : base(attribute)
        { }

        /// <summary>
        /// Gets a function that returns the attribute property of the parameter if it is an AttributeConstraint.
        /// </summary>
        public static Func<object, string> AttributeSelectorFunc
        {
            get
            {
                return o =>
                {
                    var attributeConstraint = o as AttributeConstraint;
                    return attributeConstraint != null ? attributeConstraint.Data : null;
                };
            }
        }
    }

    /// <summary>
    /// Data class for Constraint that contain the constrained PseudoAttribute,
    /// a target value and a weight.
    /// </summary>
    public class PseudoAttributeConstraint : TargetWeightConstraint<PseudoAttribute>, ICloneable
    {
        public PseudoAttributeConstraint(PseudoAttribute data)
            : base(data)
        { }

        public object Clone()
        {
            return new PseudoAttributeConstraint(Data)
            {
                TargetValue = TargetValue,
                Weight = Weight
            };
        }
    }
}