using System;
using POESKillTree.Utils;

namespace POESKillTree.TreeGenerator.Model
{
#if (PoESkillTree_UseSmallDec_ForAttributes)
    using CSharpGlobalCode.GlobalCode_ExperimentalCode;
#endif
    using SmallDigit =
#if (PoESkillTree_UseSmallDec_ForAttributes&&PoESkillTree_UseSmallDec_ForGeneratorBars)
    SmallDec;
#else
    System.Single;
#endif
    /// <summary>
    /// Data class for Constraints with a data object, a target value and a weight.
    /// </summary>
    /// <typeparam name="T">Type of the stored data object.</typeparam>
    public class TargetWeightConstraint<T> : Notifier, ICloneable
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

        private const int DefaultWeight = 100;
        private const float DefaultTargetValue = 1;

        private T _data;

        public T Data
        {
            get { return _data; }
            set { SetProperty(ref _data, value); }
        }

        private SmallDigit _targetValue = DefaultTargetValue;

        public SmallDigit TargetValue
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
        
        public TargetWeightConstraint(T data = default(T))
        {
            Data = data;
        }

#if (PoESkillTree_EnableMinimumValue)
        private SmallDigit _minimumValue =
#if (PoESkillTree_UseSmallDec_ForAttributes && PoESkillTree_UseSmallDec_ForGeneratorBars)
        SmallDec.Zero;
#else
        0.0f;
#endif

        /// <summary>
        /// Minimum Aimed value for the Attribute(with greater CSVS score contribution effect if get at least this amount) 
        /// </summary>
        public SmallDigit MinimumValue
        {
            get { return _minimumValue; }
            set
            {
                SetProperty(ref _minimumValue, value);
            }
        }
#endif

        private TargetWeightConstraint(TargetWeightConstraint<T> toClone)
            : this(toClone.Data)
        {
            TargetValue = toClone.TargetValue;
            Weight = toClone.Weight;
#if (PoESkillTree_EnableMinimumValue)
            MinimumValue = toClone.MinimumValue;
#endif
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