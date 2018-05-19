using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Nodes
{
    /// <summary>
    /// Implementation of <see cref="IValue"/> that decorates a base <see cref="IValue"/> and applies
    /// <see cref="IValueTransformation"/>s to it by implementing <see cref="IValueTransformable"/>.
    /// </summary>
    public class TransformableValue : IValue, IValueTransformable
    {
        private readonly IValue _initialValue;
        private IValue _value;

        private readonly List<IValueTransformation> _transformations = new List<IValueTransformation>();

        public TransformableValue(IValue initialValue)
        {
            _initialValue = initialValue;
            _value = initialValue;
        }

        public NodeValue? Calculate(IValueCalculationContext valueCalculationContext) => 
            _value.Calculate(valueCalculationContext);

        public void Add(IValueTransformation transformation)
        {
            _value = transformation.Transform(_value);
            _transformations.Add(transformation);
            OnValueChanged();
        }

        public void Remove(IValueTransformation transformation)
        {
            _transformations.Remove(transformation);
            _value = _transformations.Aggregate(_initialValue, (v, t) => t.Transform(v));
            OnValueChanged();
        }

        public void RemoveAll()
        {
            _transformations.Clear();
            _value = _initialValue;
            OnValueChanged();
        }

        public event EventHandler ValueChanged;

        private void OnValueChanged() => ValueChanged?.Invoke(this, EventArgs.Empty);
    }
}