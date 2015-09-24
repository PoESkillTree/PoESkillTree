using System;
using POESKillTree.TreeGenerator.Model.PseudoAttributes;
using POESKillTree.Utils;

namespace POESKillTree.TreeGenerator.Model
{
    public abstract class TargetWeightConstraint<T> : Notifier
        where T : class
    {

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

        private int _weight;

        public int Weight
        {
            get { return _weight; }
            set { SetProperty(ref _weight, value); }
        }
        
        protected TargetWeightConstraint(T data = null)
        {
            TargetValue = 0;
            Weight = DefaultWeight;
            Data = data;
        }
    }

    public class AttributeConstraint : TargetWeightConstraint<string>
    {

        // ReSharper disable once UnusedMember.Global
        // No parameter constructor is necessary for DataGrid to enable user adding rows.
        public AttributeConstraint()
        { }

        public AttributeConstraint(string attribute)
            : base(attribute)
        { }

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