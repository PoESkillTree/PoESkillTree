using System;
using PoESkillTree.Utils;

namespace PoESkillTree.TreeGenerator.Model
{
    /// <summary>
    /// Data class for Constraints with a data object, a target value and a weight.
    /// </summary>
    /// <typeparam name="T">Type of the stored data object.</typeparam>
    public class TargetWeightConstraint<T> : Notifier, ICloneable
    {
        /// <summary>
        /// Minimum allowed weight (inclusive).
        /// </summary>
        public static int MinWeight => 1;

        /// <summary>
        /// Maximum allowed weight (inclusive).
        /// </summary>
        public static int MaxWeight => 100;

        private const int DefaultWeight = 100;
        private const float DefaultTargetValue = 1;

        private T _data;

        public T Data
        {
            get => _data;
            set => SetProperty(ref _data, value);
        }

        private float _targetValue = DefaultTargetValue;

        public float TargetValue
        {
            get => _targetValue;
            set => SetProperty(ref _targetValue, value);
        }

        private int _weight = DefaultWeight;

        public int Weight
        {
            get => _weight;
            set
            {
                if (value < MinWeight || value > MaxWeight)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "must be between MinWeight and MaxWeight");
                SetProperty(ref _weight, value);
            }
        }

        public TargetWeightConstraint(T data = default!)
        {
            _data = data;
        }

        private TargetWeightConstraint(TargetWeightConstraint<T> toClone)
            : this(toClone.Data)
        {
            TargetValue = toClone.TargetValue;
            Weight = toClone.Weight;
        }

        public object Clone()
        {
            return new TargetWeightConstraint<T>(this);
        }
    }

    // Just here to specify Constraints in Xaml as DataContext with
    // d:DataContext="{d:DesignInstance model:NonGenericTargetWeightConstraint}"
    // because Xaml doesn't support generic classes for that.
    public abstract class NonGenericTargetWeightConstraint : TargetWeightConstraint<object>
    { }
}